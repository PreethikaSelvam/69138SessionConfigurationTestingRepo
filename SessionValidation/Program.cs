using SessionValidation.Client.Pages;
using SessionValidation.Components;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents(options =>
    options.TempDataProviderType = Microsoft.AspNetCore.Components.Endpoints.TempDataProviderType.SessionStorage)
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(15);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

if (builder.Configuration.GetValue("Validation:UseSession", true))
{
    app.UseSession();
}

app.MapStaticAssets();
app.MapPost("/_validation/corrupt/{key}", async (HttpContext context, string key) =>
{
    context.Session.Set(key, [0xFF, 0x00, 0xFE]);
    await context.Session.CommitAsync();
    return Results.NoContent();
});
app.MapPost("/_validation/wrong-shape/{key}", async (HttpContext context, string key) =>
{
    context.Session.Set(key, Encoding.UTF8.GetBytes("{\"unexpected\":true}"));
    await context.Session.CommitAsync();
    return Results.NoContent();
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SessionValidation.Client._Imports).Assembly);

app.Run();
