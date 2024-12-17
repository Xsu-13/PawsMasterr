document.addEventListener("DOMContentLoaded", async () => {

    const addRecipeButton = document.getElementById('add-recipt-button');
    const addRecipeForm = document.getElementById('add-form')

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

        addRecipeForm.style.display = 'none';
    });
});

function GoToProfile() {
    document.querySelector('.main').style.display = 'none';
    document.querySelector('.recepti').style.display = 'none';
    document.querySelector('.podborki').style.display = 'none';
    document.querySelector('.profil').style.display = 'none';

    showRecipes(fav_recipes);
    document.querySelector('.profil').style.display = 'block';
}


const clickSound = document.getElementById('clickSound');
const moveSound = document.getElementById('moveSound');
const button = document.querySelector('.header__svg1');
let soundsEnabled = true; // Переменная для отслеживания состояния звуков

// Воспроизведение звука при клике
document.addEventListener('click', () => {
    if (soundsEnabled) {
        clickSound.currentTime = 0; // Сброс времени воспроизведения
        clickSound.play();
    }
});

// Воспроизведение звука при движении мыши
document.addEventListener('mousemove', () => {
    if (soundsEnabled) {
        moveSound.play();
    }
});

// Обработка клика по кнопке
button.addEventListener('click', () => {
    soundsEnabled = !soundsEnabled; // Переключение состояния звуков

    if (soundsEnabled) {
        // Если звуки включены, убираем стили
        button.classList.remove('active');
    } else {
        // Если звуки выключены, применяем стили
        button.classList.add('active');
    }
});