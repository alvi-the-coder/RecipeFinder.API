using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeFinder.API.Controllers;
using RecipeFinder.API.Data;
using RecipeFinder.API.Models;
using RecipeFinder.API.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace RecipeFinder.Tests
{
    public class IngredientsControllerTest
    {
        //unit tests for the httpGet GetAll method within the ingredients controller
        [Fact]
        public async Task GetAll_ReturnsOkWithIngredients()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb1")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.AddRange(
                new Ingredient { Id = 1, Name = "Egg" },
                new Ingredient { Id = 2, Name = "Flour" }
            );
            context.SaveChanges();

            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var ingredients = Assert.IsAssignableFrom<IEnumerable<IngredientDto>>(okResult.Value);
            Assert.Equal(2, ingredients.Count());
            Assert.Contains(ingredients, i => i.Name == "Egg");
            Assert.Contains(ingredients, i => i.Name == "Flour");
        }

        [Fact]
        public async Task GetAll_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb2")
                .Options;
            var context = new CookingContext(options);
            context.Dispose(); // Will throw on access

            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.GetAll();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while retrieving ingredients", statusResult.Value);
        }

        //unit tests for the httpGet GetBId method within the ingredients controller
        [Fact]
        public async Task GetById_ReturnsOkWithIngredient()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb3")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.Add(new Ingredient { Id = 5, Name = "Sugar" });
            context.SaveChanges();

            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.GetById(5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var ingredient = Assert.IsType<IngredientDto>(okResult.Value);
            Assert.Equal("Sugar", ingredient.Name);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb4")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.GetById(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Ingredient with ID 99 not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetById_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb5")
                .Options;
            var context = new CookingContext(options);
            context.Dispose(); // Will throw on access

            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.GetById(1);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while retrieving the ingredient", statusResult.Value);
        }

        //unit tests for the httpPost Create method within the ingredients controller
        [Fact]
        public async Task Create_AddsIngredient_ReturnsCreated()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb6")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            //Act
            var dto = new IngredientDto { Name = "Salt" };
            var result = await controller.Create(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var ingredient = Assert.IsType<IngredientDto>(createdResult.Value);
            Assert.Equal("Salt", ingredient.Name);
            Assert.Equal("GetById", createdResult.ActionName);
        }

        [Fact]
        public async Task Create_WhenDuplicate_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb7")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.Add(new Ingredient { Name = "Pepper" });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            var dto = new IngredientDto { Name = "Pepper" };
            // Act
            var result = await controller.Create(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Ingredient with name 'Pepper' already exists", badRequest.Value);
        }

        [Fact]
        public async Task Create_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb8")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            var dto = new IngredientDto { Name = "Onion" };
            // Act
            var result = await controller.Create(dto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while creating the ingredient", statusResult.Value);
        }

        //unit tests for the httpPut Update method within the ingredients controller
        [Fact]
        public async Task Update_Ingredient_ReturnsOkWithUpdatedName()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb9")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.Add(new Ingredient { Id = 10, Name = "Butter" });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            var dto = new IngredientDto { Name = "Margarine" };
            // Act
            var result = await controller.Update(10, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var ingredient = Assert.IsType<IngredientDto>(okResult.Value);
            Assert.Equal("Margarine", ingredient.Name);
        }

        [Fact]
        public async Task Update_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb10")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            var dto = new IngredientDto { Name = "Oil" };
            // Act
            var result = await controller.Update(99, dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Ingredient with ID 99 not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Update_WhenNameConflict_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb11")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.Add(new Ingredient { Id = 20, Name = "Honey" });
            context.Ingredients.Add(new Ingredient { Id = 21, Name = "Maple" });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            var dto = new IngredientDto { Name = "Maple" };
            // Act
            var result = await controller.Update(20, dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Ingredient with name 'Maple' already exists", badRequest.Value);
        }

        [Fact]
        public async Task Update_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb12")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            var dto = new IngredientDto { Name = "Jam" };
            // Act
            var result = await controller.Update(1, dto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while updating the ingredient", statusResult.Value);
        }

        //unit tests for the httpDelete method within the ingredients controller
        [Fact]
        public async Task Delete_Ingredient_ReturnsNoContent()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb13")
                .Options;
            using var context = new CookingContext(options);
            context.Ingredients.Add(new Ingredient { Id = 30, Name = "Cinnamon" });
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

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
                .UseInMemoryDatabase(databaseName: "IngredientsDb14")
                .Options;
            using var context = new CookingContext(options);
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.Delete(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Ingredient with ID 99 not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Delete_WhenUsedInRecipe_ReturnsBadRequest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb15")
                .Options;
            using var context = new CookingContext(options);
            var ingredient = new Ingredient { Id = 40, Name = "Nutmeg" };
            ingredient.RecipeIngredients.Add(new RecipeIngredient { RecipeId = 1, IngredientId = 40 });
            context.Ingredients.Add(ingredient);
            context.SaveChanges();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.Delete(40);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cannot delete ingredient 'Nutmeg' as it is used in one or more recipes", badRequest.Value);
        }

        [Fact]
        public async Task Delete_WhenException_ReturnsStatusCode500()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<CookingContext>()
                .UseInMemoryDatabase(databaseName: "IngredientsDb16")
                .Options;
            var context = new CookingContext(options);
            context.Dispose();
            var logger = new LoggerFactory().CreateLogger<IngredientsController>();
            var controller = new IngredientsController(context, logger);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("An error occurred while deleting the ingredient", statusResult.Value);
        }
    }
}