namespace Backend.Models
{
    public class RecipeDtoComparer : IEqualityComparer<RecipeDto>
    {
        public bool Equals(RecipeDto x, RecipeDto y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(RecipeDto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
