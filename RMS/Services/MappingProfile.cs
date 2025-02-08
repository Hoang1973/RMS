using AutoMapper;
using RMS.Data.Entities;
using RMS.Models;

namespace RMS.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Ingredient, IngredientViewModel>()
                .ReverseMap() // Enables bi-directional mapping
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
