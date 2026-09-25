using Microsoft.JSInterop;

namespace LearnMarathi.Services;

public class StreakInfo
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public bool ActiveToday { get; set; }
}

public interface IStreakService
{
    Task<StreakInfo> GetAsync();
    Task<StreakInfo> TouchAsync();
}

public class StreakService : IStreakService
{
    private const string KeyCurrent = "lm_streak_current";
    private const string KeyLongest = "lm_streak_longest";
    private const string KeyLast = "lm_streak_last";

    private readonly IJSRuntime _js;

    public StreakService(IJSRuntime js) { _js = js; }

    private static DateTime Today => DateTime.Now.Date;

    public async Task<StreakInfo> GetAsync()
    {
        var current = int.TryParse(await _js.InvokeAsync<string?>("lmStorageGet", KeyCurrent), out var c) ? c : 0;
        var longest = int.TryParse(await _js.InvokeAsync<string?>("lmStorageGet", KeyLongest), out var l) ? l : 0;
        var lastRaw = await _js.InvokeAsync<string?>("lmStorageGet", KeyLast);
        DateTime? last = DateTime.TryParse(lastRaw, out var d) ? d.Date : null;

        // Decay if missed a day
        if (last != null)
        {
            var gap = (Today - last.Value).Days;
            if (gap > 1) current = 0;
        }

        return new StreakInfo
        {
            CurrentStreak = current,
            LongestStreak = longest,
            LastActiveDate = last,
            ActiveToday = last == Today
        };
    }

    public async Task<StreakInfo> TouchAsync()
    {
        var info = await GetAsync();
        if (info.ActiveToday) return info;

        var newCurrent = info.LastActiveDate != null && (Today - info.LastActiveDate.Value).Days == 1
            ? info.CurrentStreak + 1
            : 1;
        var newLongest = Math.Max(info.LongestStreak, newCurrent);

        await _js.InvokeVoidAsync("lmStorageSet", KeyCurrent, newCurrent.ToString());
        await _js.InvokeVoidAsync("lmStorageSet", KeyLongest, newLongest.ToString());
        await _js.InvokeVoidAsync("lmStorageSet", KeyLast, Today.ToString("yyyy-MM-dd"));

        return new StreakInfo
        {
            CurrentStreak = newCurrent,
            LongestStreak = newLongest,
            LastActiveDate = Today,
            ActiveToday = true
        };
    }
}
