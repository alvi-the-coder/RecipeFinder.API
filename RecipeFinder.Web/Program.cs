var builder = WebApplication.CreateBuilder(args);
// No services to register for a pure static site

var app = builder.Build();

// Serve wwwroot/index.html by default
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();