using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LearnMarathi;
using LearnMarathi.Data;
using LearnMarathi.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IMarathiCharacterRepository, MarathiCharacterRepository>();
builder.Services.AddScoped<IBasicWordRepository, BasicWordRepository>();
builder.Services.AddScoped<IPhraseRepository, PhraseRepository>();
builder.Services.AddScoped<INumberRepository, NumberRepository>();
builder.Services.AddScoped<IVerbRepository, VerbRepository>();
builder.Services.AddScoped<ISentenceRepository, SentenceRepository>();
builder.Services.AddScoped<ICommuteRepository, CommuteRepository>();
builder.Services.AddScoped<ISrsService, SrsService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

await builder.Build().RunAsync();
