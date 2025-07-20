using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeFinder.API.Controllers;
using RecipeFinder.API.Data;
using RecipeFinder.API.Models;
using RecipeFinder.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace RecipeFinder.Tests
{
    public class RecipesControllerTest
    {
        //unit tests for the httpGet GetAll method within the recipes controller
        [Fact]
        public async Task GetAll_ReturnsOkWithRecipes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb1")
                .Options;
            using var context = new CookingContext(options);
            var pancake = new Recipe { Id = 1, Title = "Pancakes", Servings = 4 };
            var egg = new Ingredient { Id = 1, Name = "Egg" };
            var flour = new Ingredient { Id = 2, Name = "Flour" };
            context.Recipes.Add(pancake);
            context.Ingredients.AddRange(egg, flour);
            context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = 1, IngredientId = 1, Recipe = pancake, Ingredient = egg },
                new RecipeIngredient { RecipeId = 1, IngredientId = 2, Recipe = pancake, Ingredient = flour }
            );
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var recipes = Assert.IsAssignableFrom<IEnumerable<RecipeDetailDto>>(okResult.Value);
            var recipe = Assert.Single(recipes);
            Assert.Equal("Pancakes", recipe.Title);
            Assert.Equal(4, recipe.Servings);
            Assert.Equal(2, recipe.Ingredients.Count);
            Assert.Contains(recipe.Ingredients, i => i.Name == "Egg");
            Assert.Contains(recipe.Ingredients, i => i.Name == "Flour");
        }

        [Fact]
        public async Task GetAll_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb2")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetAll();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while retrieving recipes", statusResult.Value);
        }

        //unit tests for the httpGet GetById method within the recipes controller
        [Fact]
        public async Task GetById_ReturnsOkWithRecipe()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb3")
                .Options;
            using var context = new CookingContext(options);
            var omelette = new Recipe { Id = 2, Title = "Omelette", Servings = 2 };
            var egg = new Ingredient { Id = 3, Name = "Egg" };
            context.Recipes.Add(omelette);
            context.Ingredients.Add(egg);
            context.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 2, IngredientId = 3, Recipe = omelette, Ingredient = egg });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetById(2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var recipe = Assert.IsType<RecipeDetailDto>(okResult.Value);
            Assert.Equal("Omelette", recipe.Title);
            Assert.Equal(2, recipe.Servings);
            Assert.Single(recipe.Ingredients);
            Assert.Equal("Egg", recipe.Ingredients[0].Name);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb4")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetById(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Recipe with ID 99 not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetById_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb5")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetById(1);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while retrieving the recipe", statusResult.Value);
        }

        //unit tests for the httpPost Create method within the recipes controller
        [Fact]
        public async Task Create_AddsRecipe_ReturnsRecipeDetailDto()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb6")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.AddRange(
                new Ingredient { Id = 1, Name = "Egg" },
                new Ingredient { Id = 2, Name = "Flour" }
            );
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            var dto = new RecipeDto
            {
                Title = "Cake",
                Servings = 8,
                IngredientIds = new List<int> { 1, 2 }
            };
            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var recipe = Assert.IsType<RecipeDetailDto>(okResult.Value);
            Assert.Equal("Cake", recipe.Title);
            Assert.Equal(8, recipe.Servings);
            Assert.Equal(2, recipe.Ingredients.Count);
            Assert.Contains(recipe.Ingredients, i => i.Name == "Egg");
            Assert.Contains(recipe.Ingredients, i => i.Name == "Flour");
        }

        [Fact]
        public async Task Create_WhenIngredientNotFound_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb7")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.Add(new Ingredient { Id = 1, Name = "Egg" });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            var dto = new RecipeDto
            {
                Title = "Cake",
                Servings = 8,
                IngredientIds = new List<int> { 1, 99 }
            };
            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("One or more ingredients do not exist", badRequest.Value);
        }

        [Fact]
        public async Task Create_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb8")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            var dto = new RecipeDto
            {
                Title = "Pie",
                Servings = 6,
                IngredientIds = new List<int> { 1 }
            };
            // Act
            var result = await controller.Create(dto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while creating the recipe", statusResult.Value);
        }

        ////unit tests for the httpPut Update method within the recipes controller
        [Fact]
        public async Task Update_Recipe_ReturnsUpdatedRecipeDetailDto()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb9")
                .Options;
            using var context = new CookingContext(options);
            var recipe = new Recipe { Id = 10, Title = "Bread", Servings = 2 };
            var flour = new Ingredient { Id = 1, Name = "Flour" };
            var salt = new Ingredient { Id = 2, Name = "Salt" };
            context.Recipes.Add(recipe);
            context.Ingredients.AddRange(flour, salt);
            context.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 10, IngredientId = 1, Recipe = recipe, Ingredient = flour });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            var dto = new RecipeDto
            {
                Title = "Sourdough",
                Servings = 4,
                IngredientIds = new List<int> { 2 }
            };
            // Act
            var result = await controller.Update(10, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var updatedRecipe = Assert.IsType<RecipeDetailDto>(okResult.Value);
            Assert.Equal("Sourdough", updatedRecipe.Title);
            Assert.Equal(4, updatedRecipe.Servings);
            Assert.Single(updatedRecipe.Ingredients);
            Assert.Equal("Salt", updatedRecipe.Ingredients[0].Name);
        }

        [Fact]
        public async Task Update_WhenRecipeNotFound_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb10")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            var dto = new RecipeDto
            {
                Title = "Toast",
                Servings = 1,
                IngredientIds = new List<int> { 1 }
            };
            // Act
            var result = await controller.Update(99, dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Recipe with ID 99 not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Update_WhenIngredientNotFound_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb11")
                .Options;
            using var context = new CookingContext(options);
            var recipe = new Recipe { Id = 20, Title = "Soup", Servings = 3 };
            var carrot = new Ingredient { Id = 1, Name = "Carrot" };
            context.Recipes.Add(recipe);
            context.Ingredients.Add(carrot);
            context.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 20, IngredientId = 1, Recipe = recipe, Ingredient = carrot });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            var dto = new RecipeDto
            {
                Title = "Soup",
                Servings = 3,
                IngredientIds = new List<int> { 1, 99 }
            };
            // Act
            var result = await controller.Update(20, dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("One or more ingredients do not exist", badRequest.Value);
        }

        [Fact]
        public async Task Update_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb12")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            var dto = new RecipeDto
            {
                Title = "Pie",
                Servings = 6,
                IngredientIds = new List<int> { 1 }
            };
            // Act
            var result = await controller.Update(1, dto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while updating the recipe", statusResult.Value);
        }

        //unit tests for the httpDelete method within the recipes controller
        [Fact]
        public async Task Delete_Recipe_ReturnsNoContent()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb13")
                .Options;
            using var context = new CookingContext(options);
            var recipe = new Recipe { Id = 30, Title = "Pizza", Servings = 2 };
            context.Recipes.Add(recipe);
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.Delete(30);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb14")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.Delete(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Recipe with ID 99 not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Delete_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb15")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while deleting the recipe", statusResult.Value);
        }

        //Unit tests for the HttpGet GetAvailable method within the recipe controller
        [Fact]
        public async Task GetAvailable_WithIngredients_ReturnsMakeableRecipes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb16")
                .Options;
            using var context = new CookingContext(options);
            var pancake = new Recipe { Id = 1, Title = "Pancakes", Servings = 4 };
            var egg = new Ingredient { Id = 1, Name = "Egg" };
            var flour = new Ingredient { Id = 2, Name = "Flour" };
            var milk = new Ingredient { Id = 3, Name = "Milk" };
            context.Recipes.Add(pancake);
            context.Ingredients.AddRange(egg, flour, milk);
            context.RecipeIngredients.AddRange(
                new RecipeIngredient { RecipeId = 1, IngredientId = 1, Recipe = pancake, Ingredient = egg },
                new RecipeIngredient { RecipeId = 1, IngredientId = 2, Recipe = pancake, Ingredient = flour },
                new RecipeIngredient { RecipeId = 1, IngredientId = 3, Recipe = pancake, Ingredient = milk }
            );
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetAvailable(new[] { "Egg", "Flour", "Milk" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var recipes = Assert.IsAssignableFrom<IEnumerable<RecipeDetailDto>>(okResult.Value);
            var recipe = Assert.Single(recipes);
            Assert.Equal("Pancakes", recipe.Title);
            Assert.Equal(4, recipe.Servings);
            Assert.Equal(3, recipe.Ingredients.Count);
            Assert.Contains(recipe.Ingredients, i => i.Name == "Egg");
            Assert.Contains(recipe.Ingredients, i => i.Name == "Flour");
            Assert.Contains(recipe.Ingredients, i => i.Name == "Milk");
        }

        [Fact]
        public async Task GetAvailable_NoIngredients_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb17")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetAvailable(new string[0]);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("No ingredients provided", badRequest.Value);
        }

        [Fact]
        public async Task GetAvailable_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "RecipesDb18")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<RecipesController>();
            var controller = new RecipesController(context, logger);

            // Act
            var result = await controller.GetAvailable(new[] { "Egg" });

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while getting available recipes", statusResult.Value);
        }
    }
}
