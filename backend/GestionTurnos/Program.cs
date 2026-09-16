using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Data;

var builder = WebApplication.CreateBuilder(args);

// Render (y la mayoría de los PaaS) asignan el puerto por variable de entorno.
// En desarrollo local esta variable no existe, así que Kestrel usa su configuración normal.
var puertoAsignado = Environment.GetEnvironmentVariable("PORT");
var corriendoEnPaaS = !string.IsNullOrEmpty(puertoAsignado);
if (corriendoEnPaaS)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{puertoAsignado}");
}

// Servicios
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Render ya termina HTTPS en su proxy y reenvía HTTP hacia la app.
// Forzar HTTPS también acá adentro generaría un loop de redirección.
if (!corriendoEnPaaS)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await MigrarYSembrarAdminAsync(app.Services, app.Configuration);

app.Run();

static async Task MigrarYSembrarAdminAsync(IServiceProvider services, IConfiguration configuration)
{
    using var scope = services.CreateScope();

    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var adminEmail = configuration["AdminSeed:Email"] ?? "admin@gestionturnos.local";
    var adminPassword = configuration["AdminSeed:Password"] ?? "Admin123!";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, adminPassword);
    }
}
