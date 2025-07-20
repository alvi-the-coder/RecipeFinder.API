using Microsoft.AspNetCore.Mvc;
using RecipeFinder.API.Data;
using RecipeFinder.API.Models;
using RecipeFinder.API.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace RecipeFinder.API.Controllers
{
    // Controller handling all ingredient-related operations
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientsController : ControllerBase
    {
        private readonly CookingContext _context;
        private readonly ILogger<IngredientsController> _logger;

        public IngredientsController(CookingContext context, ILogger<IngredientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Gets all ingredients
        // </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngredientDto>>> GetAll()
        {
            try
            {
                var ingredients = await _context.Ingredients
                    .AsNoTracking()
                    .Select(i => new IngredientDto { Name = i.Name })
                    .ToListAsync();

                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ingredients");
                return StatusCode(500, "An error occurred while retrieving ingredients");
            }
        }

        // Gets a specific ingredient by ID
        // </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<IngredientDto>> GetById(int id)
        {
            try
            {
                var ingredient = await _context.Ingredients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (ingredient == null)
                {
                    return NotFound($"Ingredient with ID {id} not found");
                }

                return Ok(new IngredientDto { Name = ingredient.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ingredient {IngredientId}", id);
                return StatusCode(500, "An error occurred while retrieving the ingredient");
            }
        }

        // Creates a new ingredient
        [HttpPost]
        public async Task<ActionResult<IngredientDto>> Create(IngredientDto ingredientDto)
        {
            try
            {
                // Check if ingredient with same name exists
                var exists = await _context.Ingredients
                    .AnyAsync(i => i.Name.ToLower() == ingredientDto.Name.ToLower());

                if (exists)
                {
                    return BadRequest($"Ingredient with name '{ingredientDto.Name}' already exists");
                }

                var ingredient = new Ingredient
                {
                    Name = ingredientDto.Name
                };

                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = ingredient.Id }, ingredientDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ingredient");
                return StatusCode(500, "An error occurred while creating the ingredient");
            }
        }
        
        // Updates an existing ingredient
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, IngredientDto ingredientDto)
        {
            try
            {
                var ingredient = await _context.Ingredients.FindAsync(id);

                if (ingredient == null)
                {
                    return NotFound($"Ingredient with ID {id} not found");
                }

                // Check if new name conflicts with existing ingredient
                var exists = await _context.Ingredients
                    .AnyAsync(i => i.Id != id && i.Name.ToLower() == ingredientDto.Name.ToLower());

                if (exists)
                {
                    return BadRequest($"Ingredient with name '{ingredientDto.Name}' already exists");
                }

                ingredient.Name = ingredientDto.Name;
                await _context.SaveChangesAsync();

                return Ok(new IngredientDto { Name = ingredient.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ingredient {IngredientId}", id);
                return StatusCode(500, "An error occurred while updating the ingredient");
            }
        }

        // Deletes an ingredient if it's not used in any recipes
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ingredient = await _context.Ingredients
                    .Include(i => i.RecipeIngredients)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (ingredient == null)
                {
                    return NotFound($"Ingredient with ID {id} not found");
                }

                // Check if ingredient is used in any recipes
                if (ingredient.RecipeIngredients.Any())
                {
                    return BadRequest($"Cannot delete ingredient '{ingredient.Name}' as it is used in one or more recipes");
                }

                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ingredient {IngredientId}", id);
                return StatusCode(500, "An error occurred while deleting the ingredient");
            }
        }
    }
}