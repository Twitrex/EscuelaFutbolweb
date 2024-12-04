using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllersWithViews();

// Configuración de autenticación y autorización con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirigir al formulario de login si no está autenticado
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirigir a acceso denegado si no tiene permisos
        options.SlidingExpiration = true; // Renueva automáticamente la cookie si está activa
        options.ExpireTimeSpan = TimeSpan.FromHours(1); // Expiración de la cookie después de 1 hora
    });

var app = builder.Build();

// Configuración de entornos
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Habilitar autenticación
app.UseAuthorization(); // Habilitar autorización

// Configuración de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Ruta predeterminada al login

app.Run();