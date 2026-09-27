using ExamenFront.Components;
using apiexamen;
using Radzen;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Radzen (NotificationService, DialogService, etc.)
builder.Services.AddRadzenComponents();
// apiexamen.dll: se le pasan la cadena de conexión y la URL del Web Service
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp => new clsExamen(
 builder.Configuration.GetConnectionString("BdiExamen") ?? string.Empty,
 builder.Configuration["WebService:UrlBase"] ?? string.Empty,
 sp.GetRequiredService<IHttpClientFactory>().CreateClient()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
