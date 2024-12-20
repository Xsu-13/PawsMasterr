using AutoMapper;
using Backend.Models;

namespace Backend.Configurations
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Recipe, RecipeDto>();
            CreateMap<RecipeDto, Recipe>();
            CreateMap<SelectionDto, Selection>();
            CreateMap<Selection, SelectionDto>();
        }
    }
}
