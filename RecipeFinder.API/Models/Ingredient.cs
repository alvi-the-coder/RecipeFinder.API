namespace RecipeFinder.API.Models
{
    //creating class ingredients with get and set property accessors
    //allows for the variables to get and set values
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}
