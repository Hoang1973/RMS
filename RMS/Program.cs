using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Services;
using System.Text;

namespace RMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            // Get environment variable (set manually for home/office)
            string location = Environment.GetEnvironmentVariable("MY_LOCATION"); // Default is Home

            // Choose the correct connection string
            string connectionString = location == "Office"
                ? builder.Configuration.GetConnectionString("OfficeDB")
                : builder.Configuration.GetConnectionString("DefaultConnection");

            //$env:MY_LOCATION = "Office"



            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<RMSDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("OfficeDB")));

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // Add service interfaces
            builder.Services.AddScoped<IIngredientService, IngredientService>();
            builder.Services.AddScoped<IDishService, DishService>();
            builder.Services.AddScoped<ITableService, TableService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Add these services in Program.cs
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            builder.Services.AddScoped<IAuthService, AuthService>();


            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
