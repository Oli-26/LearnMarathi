using System.Text.Json;
using LearnMarathi.Data;
using LearnMarathi.Models;
using Microsoft.JSInterop;

namespace LearnMarathi.Services;

/// <summary>
/// App-wide Commute playback, so an episode keeps playing while you move between pages.
/// The Commute page and the mini player in the nav bar are both views onto this.
/// </summary>
public class CommutePlayer : IDisposable
{
    public const int MiniHeight = 58;

    private const string ListenedKey = "lm_commute_done";
    private const string PositionsKey = "lm_commute_pos";
    private const string BlindKey = "lm_commute_blind";

    private readonly IJSRuntime _js;
    private readonly ICommuteRepository _repo;
    private readonly IStreakService _streak;
    private DotNetObjectReference<CommutePlayer>? _selfRef;
    private double _lastSavedAt;
    private bool _initialised;

    public event Action? Changed;

    public List<CommuteEpisode> Episodes { get; private set; } = new();
    public Dictionary<string, double> Durations { get; } = new();
    public HashSet<string> Listened { get; private set; } = new();
    public Dictionary<string, double> Positions { get; private set; } = new();

    public CommuteEpisode? Episode { get; private set; }
    public CommuteTiming? Timing { get; private set; }
    public double Position { get; private set; }
    public bool Playing { get; private set; }
    public double Rate { get; private set; } = 1;
    public bool AutoNext { get; set; }

    /// <summary>Blind listening: the transcript stays hidden until the episode's questions are answered.</summary>
    public bool Blind { get; private set; }

    /// <summary>Set by the Commute page while its full player is on screen; hides the mini player.</summary>
    public bool FullPlayerVisible { get; set; }

    public bool MiniVisible => Episode != null && Timing != null && !FullPlayerVisible;

    public CommutePlayer(IJSRuntime js, ICommuteRepository repo, IStreakService streak)
    {
        _js = js;
        _repo = repo;
        _streak = streak;
    }

    public async Task InitAsync()
    {
        if (_initialised) return;
        _initialised = true;
        Episodes = (await _repo.GetAllAsync()).ToList();
        Listened = await LoadAsync<HashSet<string>>(ListenedKey);
        Positions = await LoadAsync<Dictionary<string, double>>(PositionsKey);
        Blind = await _js.InvokeAsync<string?>("lmStorageGet", BlindKey) == "1";
        foreach (var ep in Episodes)
        {
            var t = await _repo.GetTimingAsync(ep);
            if (t != null) Durations[ep.Id] = t.Duration;
        }
        Notify();
    }

    public async Task OpenAsync(CommuteEpisode ep, bool play = false)
    {
        if (Episode?.Id == ep.Id)
        {
            if (play && !Playing) await PlayAsync();
            return;
        }
        await SavePositionAsync();
        Episode = ep;
        Timing = await _repo.GetTimingAsync(ep);
        Playing = false;
        if (Timing == null) { Notify(); return; }

        // Resume where you stopped, unless that was the closing seconds.
        var startAt = Positions.TryGetValue(ep.Id, out var saved) && saved < Timing.Duration - 15 ? saved : 0;
        Position = _lastSavedAt = startAt;
        _selfRef ??= DotNetObjectReference.Create(this);
        await _js.InvokeVoidAsync("commuteLoad", ep.AudioUrl, ep.Title, _selfRef, startAt);
        await _js.InvokeVoidAsync("commuteRate", Rate);
        if (play) await PlayAsync();
        Notify();
    }

    public async Task PlayAllAsync()
    {
        var first = Episodes.FirstOrDefault(e => !Listened.Contains(e.Id) && Durations.ContainsKey(e.Id))
                    ?? Episodes.FirstOrDefault(e => Durations.ContainsKey(e.Id));
        if (first == null) return;
        AutoNext = true;
        await OpenAsync(first, play: true);
    }

    public async Task PlayAsync() => await _js.InvokeAsync<bool>("commutePlay");

    public async Task TogglePlayAsync()
    {
        if (Playing) await _js.InvokeVoidAsync("commutePause");
        else await PlayAsync();
    }

    public async Task SeekAsync(double t) => await _js.InvokeVoidAsync("commuteSeek", t);

    public async Task SkipAsync(double delta) => await _js.InvokeVoidAsync("commuteSkip", delta);

    public async Task ToggleRateAsync()
    {
        Rate = Rate < 1 ? 1 : 0.75;
        await _js.InvokeVoidAsync("commuteRate", Rate);
        Notify();
    }

    public async Task ToggleBlindAsync()
    {
        Blind = !Blind;
        await _js.InvokeVoidAsync("lmStorageSet", BlindKey, Blind ? "1" : "0");
        Notify();
    }

    public async Task StopAsync()
    {
        await SavePositionAsync();
        await _js.InvokeVoidAsync("commuteUnload");
        Episode = null;
        Timing = null;
        Playing = false;
        Notify();
    }

    public int CurrentSegment => Timing?.Segments.FindLastIndex(s => s.T <= Position) ?? -1;

    public int CurrentLine
    {
        get
        {
            var seg = CurrentSegment;
            return seg >= 0 ? Timing!.Segments[seg].Line : -1;
        }
    }

    public string PhaseLabel
    {
        get
        {
            var seg = CurrentSegment;
            if (Timing == null || seg < 0) return "Ready";
            var firstSlow = Timing.Segments.FindIndex(s => s.Phase == "slow");
            var lastRepeat = Timing.Segments.FindLastIndex(s => s.Phase == "repeat");
            if (seg < firstSlow - 1) return "Listen through";
            if (seg <= lastRepeat) return "Line by line";
            return "Once more";
        }
    }

    [JSInvokable]
    public async Task OnTime(double t)
    {
        Position = t;
        if (Math.Abs(t - _lastSavedAt) >= 5) await SavePositionAsync();
        Notify();
    }

    [JSInvokable]
    public void OnPlaying(bool isPlaying)
    {
        Playing = isPlaying;
        Notify();
    }

    [JSInvokable]
    public async Task OnEnded()
    {
        Playing = false;
        if (Episode == null) return;
        var finished = Episode;
        if (Positions.Remove(finished.Id)) await SaveAsync(PositionsKey, Positions);
        if (Listened.Add(finished.Id))
        {
            await SaveAsync(ListenedKey, Listened);
            await _streak.TouchAsync();
        }

        var next = AutoNext ? NextUnheardAfter(finished) : null;
        if (next != null) await OpenAsync(next, play: true);
        Notify();
    }

    private CommuteEpisode? NextUnheardAfter(CommuteEpisode current)
    {
        var start = Episodes.IndexOf(current);
        return Episodes.Skip(start + 1).Concat(Episodes.Take(start))
                       .FirstOrDefault(e => !Listened.Contains(e.Id) && Durations.ContainsKey(e.Id));
    }

    private async Task SavePositionAsync()
    {
        if (Episode == null || Timing == null || Position < 5) return;
        _lastSavedAt = Position;
        Positions[Episode.Id] = Math.Round(Position, 1);
        await SaveAsync(PositionsKey, Positions);
    }

    private void Notify() => Changed?.Invoke();

    private async Task<T> LoadAsync<T>(string key) where T : new()
    {
        var raw = await _js.InvokeAsync<string?>("lmStorageGet", key);
        if (string.IsNullOrWhiteSpace(raw)) return new T();
        try { return JsonSerializer.Deserialize<T>(raw) ?? new T(); }
        catch (JsonException) { return new T(); }
    }

    private Task SaveAsync<T>(string key, T value) =>
        _js.InvokeVoidAsync("lmStorageSet", key, JsonSerializer.Serialize(value)).AsTask();

    public void Dispose() => _selfRef?.Dispose();
}
