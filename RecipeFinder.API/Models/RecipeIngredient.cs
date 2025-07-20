namespace RecipeFinder.API.Models
{
    //creating class recipe ingredients with get and set property accessors
    //allows for the variables to get and set values
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;
    }
}
