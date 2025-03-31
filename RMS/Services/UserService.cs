using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace RMS.Services
{
    public interface IUserService : IBaseService<UserViewModel, User> { }

    public class UserService : BaseService<UserViewModel, User>, IUserService
    {
        public UserService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }
    }

}
