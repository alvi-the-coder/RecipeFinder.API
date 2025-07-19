const API_BASE = 'https://localhost:7233';

// Function to find recipes based on ingredients
async function findRecipes() {
    const input = document.getElementById("ingredients").value;
    if (!input.trim()) {
        alert("Please enter at least one ingredient");
        return;
    }

    const ingredients = input
        .split(",")
        .map(i => i.trim())
        .filter(i => i.length > 0)
        .join("&ingredients=");

    try {
        const res = await fetch(`${API_BASE}/api/recipes/available?ingredients=${ingredients}`);
        if (!res.ok) throw new Error('Failed to fetch recipes');
        
        const recipes = await res.json();
        displayRecipes(recipes);
    } catch (e) {
        showError("Could not fetch recipes: " + e.message);
    }
}

// Function to load all recipes
async function loadAllRecipes() {
    try {
        const res = await fetch(`${API_BASE}/api/recipes`);
        if (!res.ok) throw new Error('Failed to fetch recipes');
        
        const data = await res.json();
        displayRecipes(data);
    } catch (err) {
        showError('Error loading recipes: ' + err.message);
    }
}

// Function to display recipes in the UI
function displayRecipes(recipes) {
    const list = document.getElementById("recipeList");
    const displayBox = document.getElementById("displayBox");
    
    // Clear previous results
    list.innerHTML = "";
    displayBox.innerHTML = "";

    if (recipes.length === 0) {
        displayBox.innerHTML = `
            <div class="no-recipes">
                <p>No recipes found for these ingredients.</p>
                <p class="hint">Try different ingredients or fewer ingredients to find more recipes.</p>
            </div>`;
        return;
    }

    // Create recipe cards
    recipes.forEach(recipe => {
        const recipeCard = document.createElement("div");
        recipeCard.className = "recipe-card";

        // Recipe header with title and servings
        const header = document.createElement("div");
        header.className = "recipe-header";
        header.innerHTML = `
            <h3>${recipe.title}</h3>
            <span class="servings">Serves ${recipe.servings}</span>
        `;

        // Recipe ingredients list
        const ingredients = document.createElement("div");
        ingredients.className = "recipe-ingredients";
        ingredients.innerHTML = `
            <h4>Ingredients:</h4>
            <ul>
                ${recipe.ingredients.map(ing => `<li>${ing.name}</li>`).join('')}
            </ul>
        `;

        recipeCard.appendChild(header);
        recipeCard.appendChild(ingredients);
        displayBox.appendChild(recipeCard);
    });

    // Update count
    const count = document.createElement("div");
    count.className = "recipe-count";
    count.textContent = `Found ${recipes.length} recipe${recipes.length !== 1 ? 's' : ''}`;
    displayBox.insertBefore(count, displayBox.firstChild);
}

// Function to show errors
function showError(message) {
    const displayBox = document.getElementById("displayBox");
    displayBox.innerHTML = `
        <div class="error-message">
            <p>${message}</p>
            <p class="hint">Please try again or check your input.</p>
        </div>`;
}

// Event Listeners
document.addEventListener('DOMContentLoaded', () => {
    // Wire up the find recipes button
    const findRecipesBtn = document.getElementById('findRecipesBtn');
    if (findRecipesBtn) {
        findRecipesBtn.addEventListener('click', findRecipes);
    }

    // Wire up the load all button
    const loadAllBtn = document.getElementById('loadAllBtn');
    if (loadAllBtn) {
        loadAllBtn.addEventListener('click', loadAllRecipes);
    }

    // Wire up enter key on ingredients input
    const ingredientsInput = document.getElementById('ingredients');
    if (ingredientsInput) {
        ingredientsInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                findRecipes();
            }
        });
    }
});