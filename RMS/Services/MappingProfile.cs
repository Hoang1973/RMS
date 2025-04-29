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
            CreateMap<Dish, DishViewModel>()
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<DishIngredient, DishViewModel>()
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Table, TableViewModel>()
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Order, OrderViewModel>()
                .ForMember(dest => dest.TableNumber, opt => opt.MapFrom(src => src.Table.TableNumber)) // Chỉ map khi đọc dữ liệu
                .ReverseMap()
                .ForMember(dest => dest.Table, opt => opt.Ignore()) // Không map Table ngược lại
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User, UserViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Không map Password ngược lại
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Discount, DiscountViewModel>()
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Bill, BillViewModel>()
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Payment, PaymentViewModel>()
                .ReverseMap()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
                
        }
    }
}
