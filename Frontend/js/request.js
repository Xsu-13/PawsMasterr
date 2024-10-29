axios.defaults.withCredentials = true;

let domen = "https://localhost:52351";

const fetchGetRecipes = async () => {
    try {
        let data = await axios.get(domen + "/api/Recipes")
        console.log(data)
        return data
    }
    catch (e) {
        console.log(e);
    }
}