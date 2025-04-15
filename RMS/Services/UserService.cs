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

        public override async Task CreateAsync(UserViewModel model)
        {
            var user = _mapper.Map<User>(model);
            // Hash password before saving
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            await _dbSet.AddAsync(user);
            await CreateRelationshipsAsync(user, model);
            await _context.SaveChangesAsync();
        }

        public override async Task UpdateAsync(UserViewModel model)
        {
            var entityId = model.Id;
            var user = await _dbSet.FindAsync(entityId);
            if (user == null)
                throw new Exception("User not found");

            // Store old password in case new password is not provided
            var oldPassword = user.Password;

            // Map properties except Password
            _mapper.Map(model, user);

            // Only hash new password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            else
            {
                user.Password = oldPassword; // Restore old password if not updated
            }

            await UpdateRelationshipsAsync(user, model);
            await _context.SaveChangesAsync();
        }
    }

}
