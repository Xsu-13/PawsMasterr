using AutoMapper;
using Backend.Models;

namespace Backend.Configurations
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Recipe, RecipeDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(d => d.Servings, opt => opt.MapFrom(s => s.Servings))
                .ForMember(d => d.Prep_time, opt => opt.MapFrom(s => s.PrepTime))
                .ForMember(d => d.Cook_time, opt => opt.MapFrom(s => s.CookTime))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl))
                .ForMember(d => d.Ingredients, opt => opt.MapFrom(s => s.Ingredients.Select(i => new IngredientDto { Name = i.Name, Quantity = i.Quantity }).ToList()))
                .ForMember(d => d.Steps, opt => opt.MapFrom(s => s.Steps));

            CreateMap<RecipeDto, Recipe>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id ?? ""))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
                .ForMember(d => d.Servings, opt => opt.MapFrom(s => s.Servings))
                .ForMember(d => d.PrepTime, opt => opt.MapFrom(s => s.Prep_time))
                .ForMember(d => d.CookTime, opt => opt.MapFrom(s => s.Cook_time))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl))
                .ForMember(d => d.Ingredients, opt => opt.MapFrom(s => s.Ingredients.Select(i => new Ingredient { Name = i.Name, Quantity = i.Quantity }).ToList()))
                .ForMember(d => d.Steps, opt => opt.MapFrom(s => s.Steps));

            CreateMap<Selection, SelectionDto>()
                .ForMember(d => d.id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.recipes, opt => opt.MapFrom(s => s.Recipes));

            CreateMap<SelectionDto, Selection>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.id))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.title))
                .ForMember(d => d.Recipes, opt => opt.MapFrom(s => s.recipes));
        }
    }
}
