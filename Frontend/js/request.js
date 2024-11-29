axios.defaults.withCredentials = true;

let domen = "https://localhost:50904";

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
        // Передаем ингредиенты как массив
        ingredients.forEach(ingredient => params.append('ingredients', ingredient));
    }
    if (ingredientsCount) {
        params.append('ingredientsCount', ingredientsCount);
    }
    if (servingsCount) {
        params.append('servingsCount', servingsCount);
    }

    try {
        const response = await fetch(domen + `/api/Recipes/search?${params.toString()}`, {
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

const fetchGetFavoriteRecipes = async (userId) => {
    try {
        let response = await axios.get(domen + `/api/User/favoriteRecipes?userId=${userId}`)
        return response.data;
    }
    catch (e) {
        console.log(e);
    }
}

const fetchGetUser = async (userId) => {
    try {
        let response = await axios.get(domen + `/api/User?userId=${userId}`)
        return response.data;
    }
    catch (e) {
        console.log(e);
    }
}

const fetchAddToFavorites = async (userId, recipeId) => {
    try {
        let response = await axios.post(domen + `/api/User/${recipeId}?userId=${userId}`)
        return response.data;
    }
    catch (e) {
        console.log(e);
    }
}

const fetchRemoveFromFavorites = async (userId, recipeId) => {
    try {
        await axios.delete(domen + `/api/User/${recipeId}?userId=${userId}`)
    }
    catch (e) {
        console.log(e);
    }
}

const fetchPostRecipe = async (recipe) => {
    try {
        let response = await axios.post(domen + `/api/Recipes`, recipe)
        return response;
    }
    catch (e) {
        console.log(e);
    }
}

const fetchPutRecipe = async (recipeId, recipe) => {
    try {
        await axios.put(domen + `/api/Recipes/${recipeId}`, recipe)
    }
    catch (e) {
        console.log(e);
    }
}

const fetchGetRecipeById = async (recipeId) => {
    try {
        let response = await axios.get(domen + `/api/Recipes/${recipeId}`)
        return response.data;
    }
    catch (e) {
        console.log(e);
    }
}

const fetchUploadRecipeImage = async (recipeId, imageFile) => {
    const formData = new FormData();
    formData.append('image', imageFile);

    try {
        const response = await axios.post(domen + `/api/Recipes/${recipeId}/upload-image`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
        console.log('Image uploaded successfully:', response.data);
    } catch (error) {
        console.error('Error uploading image:', error.response ? error.response.data : error.message);
    }
}

const fetchUploadUserImage = async (userId, imageFile) => {
    const formData = new FormData();
    formData.append('image', imageFile);

    try {
        const response = await axios.post(domen + `/api/User/${userId}/upload-image`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
        console.log('Image uploaded successfully:', response.data);
    } catch (error) {
        console.error('Error uploading image:', error.response ? error.response.data : error.message);
    }
}

const fetcLogin = async (login) => {
    try {
        await axios.post(domen + `/api/User/login`, login)
    }
    catch (e) {
        console.log(e);
    }
}

const fetchSignUp = async (signup) => {
    try {
        await axios.post(domen + `/api/User/signup`, signup)
    }
    catch (e) {
        console.log(e);
    }
}

const fetchLogout = async () => {
    try {
        await axios.post(domen + `/api/User/logout`)
    }
    catch (e) {
        console.log(e);
    }
}