using RecipeFinder.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Required for Swagger
builder.Services.AddSwaggerGen(); // Registers Swagger generator
builder.Services.AddDbContext<CookingContext>(options =>
    options.UseSqlite("Data Source=recipes.db"));
builder.Services.AddCors(o =>
  o.AddPolicy("AllowAll", p =>
    p.AllowAnyOrigin()
     .AllowAnyHeader()
     .AllowAnyMethod()));
builder.Services.AddCors(opts =>
  opts.AddPolicy("AllowWeb",
    policy => policy
      .WithOrigins("https://localhost:7140")
      .AllowAnyHeader()
      .AllowAnyMethod()
  )
);

var app = builder.Build();

// Ensure the database is created and seeded on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CookingContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

// Enable Swagger UI for all environments
// This allows you to test and explore your API interactively
app.UseSwagger(); // Generates Swagger JSON endpoint
app.UseSwaggerUI(); // Serves Swagger UI at /swagger

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseCors("AllowWeb");
app.MapControllers();
app.Run();
