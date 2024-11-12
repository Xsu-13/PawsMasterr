document.addEventListener("DOMContentLoaded", () => {

    let selectedIngredients = [];

    // Получаем поле ввода для ингредиентов
    const ingredientsInput = document.querySelector('.ingridients .search__input');

    // Добавляем обработчик события нажатия клавиши
    ingredientsInput.addEventListener('keydown', function (event) {
        // Проверяем, была ли нажата клавиша Enter
        if (event.key === 'Enter') {
            // Получаем значение из поля ввода
            const ingredient = ingredientsInput.value.trim();

            // Проверяем, не пустое ли значение
            if (ingredient) {
                selectedIngredients.push(ingredient);
                console.log('Ингредиенты:', selectedIngredients); // Выводим в консоль
                ingredientsInput.value = ''; // Очищаем поле ввода после вывода
            }
        }
    });

    const filterButton = document.querySelector('.podbor__button');

    // Добавляем обработчик события клика
    filterButton.addEventListener('click', async function () {
        // Получаем значения из полей ввода
        const searchInput = document.querySelector('.search__input').value;
        const ingredientsCountInput = document.querySelector('.filters__block input:nth-child(1)').value;
        const servingsCountInput = document.querySelector('.filters__block input:nth-child(2)').value;

        // Выводим значения в консоль (или выполняем другие действия)
        console.log('Поиск:', searchInput);
        console.log('Ингредиенты:', selectedIngredients);
        console.log('Количество ингредиентов:', ingredientsCountInput);
        console.log('Число персон:', servingsCountInput);

        let recipes = await fetchGetRecipesWithFilter(subtitle = searchInput, ingredients = selectedIngredients, ingredientsCount = ingredientsCountInput, servingsCount = servingsCountInput);
        console.log(recipes);

        showRecipes(recipes);
    });


    var clearRecipes = () => {
        var recipesElements = document.getElementsByClassName("recepts");

        if (recipesElements && recipesElements.length > 0) {
            // Преобразуем HTMLCollection в массив и проходим по каждому элементу
            Array.from(recipesElements).forEach(recipe => {
                recipe.parentNode.removeChild(recipe); // Удаляем элемент из родителя
            });
        }
    }

    var showRecipes = async (recipesParam) => {

        clearRecipes();

        if (recipesParam == null) {
            recipes = (await fetchGetRecipes()).data;
        }
        else {
            recipes = recipesParam;
        }

        //ПОТОМ ИЗМЕНИТЬ!
        if (recipes.length > 20)
            recipes = recipes.slice(0, 20);

        recipes.forEach(recipe => {
            // Создаем секцию рецептов
            const section = document.createElement('section');
            section.className = 'recepts';

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
                {
                    svg: '<svg class="recepts__heart" width="35" height="35" viewBox="0 0 35 35" fill="none" xmlns="http://www.w3.org/2000/svg"><path d="M17.5 6.01136L15.9321 4.39979C12.2476 0.612646 5.49969 1.91996 3.06328 6.67809C1.91742 8.91588 1.65977 12.146 3.75001 16.2697C5.76313 20.2412 9.94804 24.9954 17.5 30.1757C25.052 24.9954 29.2369 20.2412 31.25 16.2697C33.3402 12.146 33.0826 8.91588 31.9367 6.67809C29.5003 1.91996 22.7524 0.612646 19.0679 4.39979L17.5 6.01136ZM17.5 32.8125C-16.0412 10.6496 7.17228 -6.6519 17.1157 2.50049C17.2461 2.62056 17.3743 2.74517 17.5 2.87438C17.6257 2.74518 17.7539 2.62057 17.8843 2.50051C27.8277 -6.65192 51.0412 10.6496 17.5 32.8125Z" fill="#36281C"/></svg>',
                }
            ];

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
            let elem = document.getElementById("recepti");
            elem.appendChild(section);

        });

        //         let elem = document.getElementById("recepti");
        // elem.appendChild(section);
        //  // Создаем элемент section
        //  const newsection = document.createElement('section');
        //  newsection.className = 'new-recept';

        //  // Создаем элемент div для текста
        //  const textDiv = document.createElement('div');
        //  textDiv.className = 'new-recept__text';
        //  textDiv.textContent = 'Не нашли рецепт? Добавьте сами!';

        //  // Создаем элемент div для кнопки
        //  const buttonDiv = document.createElement('div');
        //  buttonDiv.className = 'new-recept__button btn-reset';
        //  buttonDiv.textContent = 'Добавить';

        //  // Добавляем текст и кнопку в секцию
        //  section.appendChild(textDiv);
        //  section.appendChild(buttonDiv);

        //  // Добавляем секцию в body или другой контейнер
        //  elem.appendChild(newsection);

    };


    showRecipes();


    document.querySelector('.main').style.display = 'block';
    document.querySelector('.recepti').style.display = 'none';
    document.querySelector('.podborki').style.display = 'none';
    document.querySelector('.profil').style.display = 'none';

    document.querySelectorAll('.header__link').forEach(link => {
        link.addEventListener('click', function () {
            // Скрываем все блоки
            document.querySelector('.main').style.display = 'none';
            document.querySelector('.recepti').style.display = 'none';
            document.querySelector('.podborki').style.display = 'none';
            document.querySelector('.profil').style.display = 'none';

            // Показываем нужный блок
            if (this.id === 'recepti') {
                document.querySelector('.recepti').style.display = 'block';
            } else if (this.id === 'podborki') {
                document.querySelector('.podborki').style.display = 'block';
            } else if (this.id === 'profil') {
                document.querySelector('.profil').style.display = 'block';
            } else if (this.id === 'main') {
                document.querySelector('.main').style.display = 'block';
            }
        });
    });

    const form = document.querySelector('.add-form');
    document.querySelectorAll('.new').forEach(button => {
        button.addEventListener('click', () => {
            form.classList.add('show'); // Добавляем класс для показа формы
        });
    });
    const closeButton = document.querySelector('.add-form__right svg');
    closeButton.addEventListener('click', () => {
        form.classList.remove('show');
    })

    document.querySelector('.add-form__add-photo').addEventListener('click', (event) => {
        event.preventDefault(); // Отменяем стандартное поведение ссылки
        document.getElementById('file-input').click(); // Открываем диалог выбора файла
    });
});
