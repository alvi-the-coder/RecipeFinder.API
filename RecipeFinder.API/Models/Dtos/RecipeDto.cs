using System.ComponentModel.DataAnnotations;

namespace RecipeFinder.API.Models.Dtos
{
    // Data transfer object for creating or updating a recipe
    public class RecipeDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Range(1, 50)]
        public int Servings { get; set; }
        
        [Required]
        public List<int> IngredientIds { get; set; } = new List<int>();
    }

    // Data transfer object for recipe details including ingredients
    public class RecipeDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Servings { get; set; }
        public List<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
    }

    // Data transfer object for basic ingredient information
    public class IngredientDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}