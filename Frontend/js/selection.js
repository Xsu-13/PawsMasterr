document.addEventListener("DOMContentLoaded", async () => {
    var selectionId = getQueryParams().type

    let collection = await fetchGetRecipesBySelectionId(selectionId);

    let recipesPromises = collection.recipes.map(r => fetchGetRecipeById(r));
    let recipes = await Promise.all(recipesPromises);
    

    function getQueryParams() {
        const params = new URLSearchParams(window.location.search);
        return {
            type: params.get('id')
        };
    }

    var clearRecipes = () => {
        var recipesElements = document.getElementsByClassName("recepts");

        if (recipesElements && recipesElements.length > 0) {
            // Преобразуем HTMLCollection в массив и проходим по каждому элементу
            Array.from(recipesElements).forEach(recipe => {
                recipe.parentNode.removeChild(recipe); // Удаляем элемент из родителя
            });
        }
    }
    
    var showRecipes = async (recipesParam, isFavorite = false) => {

        clearRecipes();
        fav_recipes = await fetchGetFavoriteRecipes('66e758ae205c94eb5142bb98');
        added_recipes = await fetchGetAddedRecipes('66e758ae205c94eb5142bb98');

        fav_ids = fav_recipes.map(recipe => recipe.id);
        added_ids = added_recipes.map(recipe => recipe.id);
        recipes = recipesParam;

        //ПОТОМ ИЗМЕНИТЬ!
        if (recipes.length > 20)
            recipes = recipes.slice(0, 20);

        recipes.forEach(recipe => {
            // Создаем секцию рецептов
            const section = document.createElement('section');
            section.className = 'recepts';
            section.id = recipe.id;

            if (added_ids.includes(recipe.id))
            {
                section.className = 'recepts added';
            }

            // Создаем заголовок рецепта
            const header = document.createElement('div');
            header.className = 'recepts__header';

            const recipeTitle = document.createElement('p');
            recipeTitle.className = 'recepts__title';
            recipeTitle.textContent = recipe.title;

            // Создаем контейнер для иконок
            const iconsContainer = document.createElement('div');
            iconsContainer.className = 'recepts__icons';

            // Функция для создания элемента иконки
            function createIcon(svgContent, text) {
                const iconItem = document.createElement('div');
                iconItem.className = 'recepts__icons-item';

                const svg = document.createElement('div');
                svg.innerHTML = svgContent; // Вставляем SVG-код

                const paragraph = document.createElement('p');
                paragraph.textContent = text;

                iconItem.appendChild(svg);
                iconItem.appendChild(paragraph);

                return iconItem;
            }

            // SVG-коды и текст для иконок
            const iconsData = [
                {
                    svg: '<svg width="35" height="32" viewBox="0 0 35 32" fill="none" xmlns="http://www.w3.org/2000/svg"> <path d="M27.3999 9.95819H27.4123M22.3081 23.381C26.6295 18.6724 29.635 14.9546 31.5121 12.4237C32.5398 11.0393 33.0544 10.3455 32.9955 9.4013C32.8761 7.42887 27.9238 2 25.746 2C24.7494 2 23.948 2.83533 22.3437 4.50755L2.85222 24.823C2.30503 25.4019 2 26.1694 2 26.9674C2 27.7653 2.30503 28.5328 2.85222 29.1117C4.14029 30.4541 6.28242 30.2472 7.31163 28.6792L10.8612 23.2737C12.2841 21.1053 13.3056 21.0928 15.0121 22.8708C16.0429 23.9457 17.3449 25.9477 18.9693 25.929C19.9815 25.9166 20.7565 25.0719 22.3081 23.381Z" stroke="black" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                    text: recipe.prep_time
                },
                {
                    svg: '<svg width="44" height="32" viewBox="0 0 44 32" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M32 20L42 16M32 20H2L2.439 23.802C2.63579 25.5083 3.4531 27.0828 4.73537 28.2256C6.01763 29.3685 7.67534 30 9.393 30H24.607C26.3247 30 27.9824 29.3685 29.2646 28.2256C30.5469 27.0828 31.3642 25.5083 31.561 23.802L32 20ZM17 2V12M9 4V10M25 4V10" stroke="black" stroke-width="2.83333" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                    text: recipe.cook_time
                },
                {
                    svg: '<svg width="27" height="29" viewBox="0 0 27 29" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M23.5625 27C23.9437 27 24.3094 26.8495 24.579 26.5816C24.8485 26.3137 25 25.9503 25 25.5714V23.7914C25.0057 19.7829 19.2874 16.6429 13.5 16.6429C7.71262 16.6429 2 19.7829 2 23.7914V25.5714C2 25.9503 2.15145 26.3137 2.42103 26.5816C2.69062 26.8495 3.05625 27 3.4375 27H23.5625ZM18.6807 7.14857C18.6807 7.82469 18.5467 8.49419 18.2864 9.11885C18.026 9.7435 17.6444 10.3111 17.1633 10.7892C16.6823 11.2672 16.1111 11.6465 15.4826 11.9052C14.854 12.164 14.1803 12.2971 13.5 12.2971C12.8197 12.2971 12.146 12.164 11.5174 11.9052C10.8889 11.6465 10.3177 11.2672 9.83665 10.7892C9.35558 10.3111 8.97397 9.7435 8.71361 9.11885C8.45325 8.49419 8.31925 7.82469 8.31925 7.14857C8.31925 5.78309 8.86508 4.47353 9.83665 3.50798C10.8082 2.54244 12.126 2 13.5 2C14.874 2 16.1918 2.54244 17.1633 3.50798C18.1349 4.47353 18.6807 5.78309 18.6807 7.14857Z" stroke="black" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                    text: recipe.servings + ' чел'
                },

            ];


            if (fav_ids.includes(recipe.id)) {
                iconsData.push({
                    svg: '<svg class="recepts__heart fav" width="35" height="35" viewBox="0 0 35 35" fill="none" xmlns="http://www.w3.org/2000/svg"><path fill-rule="evenodd" clip-rule="evenodd" d="M12 6.00019C10.2006 3.90317 7.19377 3.2551 4.93923 5.17534C2.68468 7.09558 2.36727 10.3061 4.13778 12.5772C5.60984 14.4654 10.0648 18.4479 11.5249 19.7369C11.6882 19.8811 11.7699 19.9532 11.8652 19.9815C11.9483 20.0062 12.0393 20.0062 12.1225 19.9815C12.2178 19.9532 12.2994 19.8811 12.4628 19.7369C13.9229 18.4479 18.3778 14.4654 19.8499 12.5772C21.6204 10.3061 21.3417 7.07538 19.0484 5.17534C16.7551 3.2753 13.7994 3.90317 12 6.00019Z" stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                })
            }
            else {
                iconsData.push({
                    svg: '<svg class="recepts__heart" width="35" height="35" viewBox="0 0 35 35" fill="none" xmlns="http://www.w3.org/2000/svg"><path fill-rule="evenodd" clip-rule="evenodd" d="M12 6.00019C10.2006 3.90317 7.19377 3.2551 4.93923 5.17534C2.68468 7.09558 2.36727 10.3061 4.13778 12.5772C5.60984 14.4654 10.0648 18.4479 11.5249 19.7369C11.6882 19.8811 11.7699 19.9532 11.8652 19.9815C11.9483 20.0062 12.0393 20.0062 12.1225 19.9815C12.2178 19.9532 12.2994 19.8811 12.4628 19.7369C13.9229 18.4479 18.3778 14.4654 19.8499 12.5772C21.6204 10.3061 21.3417 7.07538 19.0484 5.17534C16.7551 3.2753 13.7994 3.90317 12 6.00019Z" stroke="#000000" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>',
                })
            }

            // Добавляем иконки в контейнер
            iconsData.forEach(icon => {
                iconsContainer.appendChild(createIcon(icon.svg, icon.text));
            });

            // Добавляем иконки в заголовок
            header.appendChild(recipeTitle);
            header.appendChild(iconsContainer);

            // Добавляем заголовок в секцию
            section.appendChild(header);

            const ingredientsContainer = document.createElement('div');
            ingredientsContainer.className = 'recepts__ingridients';

            // Добавляем изображение
            const image = document.createElement('img');
            image.src = domen + recipe.imageUrl;
            image.alt = '';
            image.className = 'recepts__image';
            ingredientsContainer.appendChild(image);

            // Создаем блок с ингредиентами
            const ingredientsBlock = document.createElement('div');
            ingredientsBlock.className = 'recepts__ingridients-block';

            // Добавляем заголовок
            const title = document.createElement('p');
            title.className = 'recepts__ingridients-title';
            title.textContent = 'Ингридиенты';
            ingredientsBlock.appendChild(title);

            // Создаем список ингредиентов
            const ingredientsList = document.createElement('div');
            ingredientsList.className = 'recepts__ingridients-block-list';

            // Массив с ингредиентами
            const ingredients = recipe.ingredients

            // Добавляем каждый ингредиент в список
            ingredients.forEach(ingredient => {
                const item = document.createElement('div');
                item.className = 'recepts__ingridients-block-list-item';

                const name = document.createElement('p');
                name.className = 'recepts__ingridients-name';
                name.textContent = ingredient.name;

                const line = document.createElement('p');
                line.className = 'recepts__ingridients-line';

                const count = document.createElement('p');
                count.className = 'recepts__ingridients-count';
                count.textContent = ingredient.quantity;

                item.appendChild(name);
                item.appendChild(line);
                item.appendChild(count);
                ingredientsList.appendChild(item);
            });

            // Добавляем список ингредиентов в блок
            ingredientsBlock.appendChild(ingredientsList);

            // Добавляем блок с ингредиентами в контейнер
            ingredientsContainer.appendChild(ingredientsBlock);

            section.appendChild(ingredientsContainer);

            // Создаем элемент для ингредиентов
            const instructionsContainer = document.createElement('div');
            instructionsContainer.className = 'recepts__description-instruction-list';

            // Массив с инструкциями
            const instructions = recipe.steps;

            var stepNum = 1;
            // Добавляем каждую инструкцию в контейнер
            instructions.forEach(instruction => {
                const item = document.createElement('div');
                item.className = 'recepts__description-instruction-list-item';

                const num = document.createElement('div');
                num.className = 'recepts__description-instruction-list-item-num';
                num.textContent = stepNum;

                const text = document.createElement('div');
                text.className = 'recepts__description-instruction-list-item-text';
                text.textContent = instruction;

                item.appendChild(num);
                item.appendChild(text);
                instructionsContainer.appendChild(item);

                stepNum += 1;
            });

            const descriptionContainer = document.createElement('div');
            descriptionContainer.className = 'recepts__description';

            const descriptionTextContainer = document.createElement('div');
            descriptionTextContainer.className = 'recepts__description-text';

            descriptionTextContainer.innerHTML = recipe.description;

            const descriptionInstructionContainer = document.createElement('div');
            descriptionInstructionContainer.className = 'recepts__description-instruction';

            const descriptionPContainer = document.createElement('p');
            descriptionPContainer.className = 'recepts__description-instruction-name';
            descriptionPContainer.innerHTML = 'Инструкция приготовления';

            descriptionContainer.appendChild(descriptionTextContainer);
            descriptionContainer.appendChild(descriptionInstructionContainer);


            // Добавляем контейнер с инструкциями в секцию рецепта
            section.appendChild(descriptionContainer);
            descriptionInstructionContainer.appendChild(descriptionPContainer);
            descriptionInstructionContainer.appendChild(instructionsContainer);

            // Добавляем секцию на страницу (например, в body)
            if (isFavorite == true) {
                let elem = document.getElementById("fav_recepts");
                elem.appendChild(section);
            }
            else {
                let elem = document.getElementById("recepts");
                let podbor = document.getElementById("podbor");
                elem.appendChild(section);

                podbor.after(section);
            }

        });

        let recips = document.getElementsByClassName("recepts__heart");

        [...recips].forEach((elem) => {
            elem.addEventListener('click', async function () {

                let recipt = elem.closest('.recepts');
                if (elem.classList.contains('fav')) {
                    await fetchRemoveFromFavorites(user.id, recipt.id);
                    elem.classList.remove('fav');
                    fav_recipes = fav_recipes.filter(item => item.id !== recipt.id);
                    if(current_page_is_favorite == true)
                    {
                        showRecipes(fav_recipes, current_page_is_favorite);
                    }
                }
                else {
                    await fetchAddToFavorites(user.id, recipt.id);
                    elem.classList.add('fav');

                    let newFav = await fetchGetRecipeById(recipt.id);
                    fav_recipes.push(newFav);
                    // showRecipes(fav_recipes, current_page_is_favorite);
                }

            })
        })
    };


    showRecipes(recipes, false);
})