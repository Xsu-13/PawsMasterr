document.addEventListener("DOMContentLoaded", async () => {

    const addRecipeButton = document.getElementById('add-recipt-button');

    addRecipeButton.addEventListener('click', async function () {
        
        const recipe_name = document.getElementById('recipt_input_name').value;
        const recipe_prep_time = document.getElementById('recipt_input_preparation_time').value;
        const recipe_cooking_time = document.getElementById('recipt_input_cooking_time').value;
        const recipe_serving = parseInt(document.getElementById('recipt_input_serving').value, 10);

        const recipe_file = document.getElementById('file-input').files[0];
        const recipe_description = document.getElementById('recipt_input_description').value;
        const recipe_steps = document.getElementById('recipt_input_steps').value.split('\n').filter(step => step.trim() !== '');

        const recipe_ingredients_elements = document.getElementsByClassName('add-form__block-product-input');
        const recipe_ingredients_count_elements = document.getElementsByClassName('add-form__block-count-input');

        let recipe_ingredients = [];
        for (let i = 0; i < recipe_ingredients_elements.length; i++) {
            const ingredient_name = recipe_ingredients_elements[i].value.trim();
            const ingredient_quantity = recipe_ingredients_count_elements[i].value.trim();
            if (ingredient_name && ingredient_quantity) {
                recipe_ingredients.push({ name: ingredient_name, quantity: ingredient_quantity });
            }
        }

        const recipe_data = {
            title: recipe_name,
            ingredients: recipe_ingredients,
            steps: recipe_steps,
            servings: recipe_serving,
            prep_time: recipe_prep_time,
            cook_time: recipe_cooking_time,
            description: recipe_description
        };

        const response = await fetchPostRecipe(recipe_data);

        await fetchUploadRecipeImage(response.data.id, recipe_file);
    });
});