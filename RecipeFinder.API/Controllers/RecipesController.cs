using Microsoft.AspNetCore.Mvc;
using RecipeFinder.API.Data;
using RecipeFinder.API.Models;
using RecipeFinder.API.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace RecipeFinder.API.Controllers
{
    /// <summary>
    /// Controller handling all recipe-related operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly CookingContext _context;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(CookingContext context, ILogger<RecipesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all recipes with their ingredients
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecipeDetailDto>>> GetAll()
        {
            try
            {
                var recipes = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                    .AsNoTracking()
                    .Select(r => new RecipeDetailDto
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Servings = r.Servings,
                        Ingredients = r.RecipeIngredients
                            .Select(ri => new IngredientDto { Name = ri.Ingredient.Name })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recipes");
                return StatusCode(500, "An error occurred while retrieving recipes");
            }
        }

        /// <summary>
        /// Gets a specific recipe by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDetailDto>> GetById(int id)
        {
            try
            {
                var recipe = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipe == null)
                {
                    return NotFound($"Recipe with ID {id} not found");
                }

                var recipeDto = new RecipeDetailDto
                {
                    Id = recipe.Id,
                    Title = recipe.Title,
                    Servings = recipe.Servings,
                    Ingredients = recipe.RecipeIngredients
                        .Select(ri => new IngredientDto { Name = ri.Ingredient.Name })
                        .ToList()
                };

                return Ok(recipeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recipe {RecipeId}", id);
                return StatusCode(500, "An error occurred while retrieving the recipe");
            }
        }

        /// <summary>
        /// Creates a new recipe with ingredients
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RecipeDetailDto>> Create(RecipeDto recipeDto)
        {
            try
            {
                // Validate ingredients exist
                var ingredients = await _context.Ingredients
                    .Where(i => recipeDto.IngredientIds.Contains(i.Id))
                    .ToListAsync();

                if (ingredients.Count != recipeDto.IngredientIds.Count)
                {
                    return BadRequest("One or more ingredients do not exist");
                }

                var recipe = new Recipe
                {
                    Title = recipeDto.Title,
                    Servings = recipeDto.Servings
                };

                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();

                // Add recipe ingredients
                foreach (var ingredientId in recipeDto.IngredientIds)
                {
                    _context.RecipeIngredients.Add(new RecipeIngredient
                    {
                        RecipeId = recipe.Id,
                        IngredientId = ingredientId
                    });
                }

                await _context.SaveChangesAsync();

                return await GetById(recipe.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe");
                return StatusCode(500, "An error occurred while creating the recipe");
            }
        }

        /// <summary>
        /// Updates an existing recipe
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RecipeDetailDto>> Update(int id, RecipeDto recipeDto)
        {
            try
            {
                var recipe = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipe == null)
                {
                    return NotFound($"Recipe with ID {id} not found");
                }

                // Validate ingredients exist
                var ingredients = await _context.Ingredients
                    .Where(i => recipeDto.IngredientIds.Contains(i.Id))
                    .ToListAsync();

                if (ingredients.Count != recipeDto.IngredientIds.Count)
                {
                    return BadRequest("One or more ingredients do not exist");
                }

                // Update basic properties
                recipe.Title = recipeDto.Title;
                recipe.Servings = recipeDto.Servings;

                // Remove existing recipe ingredients
                _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);

                // Add new recipe ingredients
                foreach (var ingredientId in recipeDto.IngredientIds)
                {
                    _context.RecipeIngredients.Add(new RecipeIngredient
                    {
                        RecipeId = recipe.Id,
                        IngredientId = ingredientId
                    });
                }

                await _context.SaveChangesAsync();

                return await GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe {RecipeId}", id);
                return StatusCode(500, "An error occurred while updating the recipe");
            }
        }

        /// <summary>
        /// Deletes a recipe
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var recipe = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipe == null)
                {
                    return NotFound($"Recipe with ID {id} not found");
                }

                _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe {RecipeId}", id);
                return StatusCode(500, "An error occurred while deleting the recipe");
            }
        }

        /// <summary>
        /// Gets recipes that can be made with the provided ingredients
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RecipeDetailDto>>> GetAvailable([FromQuery] string[] ingredients)
        {
            try
            {
                if (ingredients == null || !ingredients.Any())
                {
                    return BadRequest("No ingredients provided");
                }

                var available = ingredients.Select(i => i.ToLower()).ToHashSet();

                var recipes = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                    .AsNoTracking()
                    .ToListAsync();

                var makeable = recipes
                    .Where(r => r.RecipeIngredients.All(ri => 
                        available.Contains(ri.Ingredient.Name.ToLower())))
                    .Select(r => new RecipeDetailDto
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Servings = r.Servings,
                        Ingredients = r.RecipeIngredients
                            .Select(ri => new IngredientDto { Name = ri.Ingredient.Name })
                            .ToList()
                    })
                    .ToList();

                return Ok(makeable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available recipes");
                return StatusCode(500, "An error occurred while getting available recipes");
            }
        }
    }
}
