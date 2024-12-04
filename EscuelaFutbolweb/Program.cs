using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de servicios
builder.Services.AddControllersWithViews();

// Configuraci�n de autenticaci�n y autorizaci�n con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirigir al formulario de login si no est� autenticado
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirigir a acceso denegado si no tiene permisos
        options.SlidingExpiration = true; // Renueva autom�ticamente la cookie si est� activa
        options.ExpireTimeSpan = TimeSpan.FromHours(1); // Expiraci�n de la cookie despu�s de 1 hora
    });

var app = builder.Build();

// Configuraci�n de entornos
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Habilitar autenticaci�n
app.UseAuthorization(); // Habilitar autorizaci�n

// Configuraci�n de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"); // Ruta predeterminada al login

app.Run();