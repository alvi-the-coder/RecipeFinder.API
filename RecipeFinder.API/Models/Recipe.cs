namespace RecipeFinder.API.Models
{
    //creating class recipes with get and set property accessors
    //allows for the variables to get and set values
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Servings { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}
