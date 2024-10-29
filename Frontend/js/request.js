axios.defaults.withCredentials = true;

let domen = "https://localhost:50365";

const fetchGetRecipes = async () => {
    try {
        let data = await axios.get(domen + "/api/Recipes")
        return data
    }
    catch (e) {
        console.log(e);
    }
}

const fetchGetRecipesWithFilter = async (subtitle, ingredients, ingredientsCount, servingsCount) => {
    // Создаем объект для параметров запроса
    const params = new URLSearchParams();

    // Добавляем параметры только если они определены
    if (subtitle) {
        params.append('subtitle', subtitle);
    }
    if (ingredients && ingredients.length > 0) {
        params.append('ingredients', ingredients.join(','));
    }
    if (ingredientsCount) {
        params.append('ingredientsCount', ingredientsCount);
    }
    if (servingsCount) {
        params.append('servingsCount', servingsCount);
    }

    try {
        const response = await fetch(domen+`/api/Recipes/search?${params.toString()}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        // Проверяем, успешен ли ответ
        if (!response.ok) {
            throw new Error(`Ошибка: ${response.status}`);
        }

        // Получаем данные из ответа
        return await response.json();
    } catch (error) {
        console.error('Ошибка при выполнении запроса:', error);
    }
}