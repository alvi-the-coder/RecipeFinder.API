using RecipeFinder.API.Models;
using Microsoft.EntityFrameworkCore;

namespace RecipeFinder.API.Data
{
    // Database context for the recipe finder application.
    // Handles the connection to SQLite and defines the database schema.
    public class CookingContext : DbContext
    {
        public CookingContext(DbContextOptions<CookingContext> options)
            : base(options)
        { }

        // Define database sets (tables)
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure many-to-many relationship between Recipe and Ingredient
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId }); // Composite key

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            modelBuilder.Entity<RecipeIngredient>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);

            // Seed initial data
            modelBuilder.Entity<Ingredient>().HasData(
                new Ingredient { Id = 1, Name = "Egg" },
                new Ingredient { Id = 2, Name = "Flour" },
                new Ingredient { Id = 3, Name = "Milk" }
            );

            modelBuilder.Entity<Recipe>().HasData(
                new Recipe { Id = 1, Title = "Pancakes", Servings = 4 }
            );

            modelBuilder.Entity<RecipeIngredient>().HasData(
                new RecipeIngredient { RecipeId = 1, IngredientId = 1 },
                new RecipeIngredient { RecipeId = 1, IngredientId = 2 },
                new RecipeIngredient { RecipeId = 1, IngredientId = 3 }
            );
        }
    }
}
