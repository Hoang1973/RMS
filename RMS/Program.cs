using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

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
            builder.Services.AddControllersWithViews(options =>
            {
                // Yêu cầu xác thực cho tất cả controller/action, trừ các action được đánh dấu [AllowAnonymous]
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

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
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IDiscountService, DiscountService>();
            builder.Services.AddScoped<IBillService, BillService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IKitchenDisplayService, KitchenDisplayService>();

            builder.Services.AddRazorPages();
            builder.Services.AddSignalR(); // Add this line

            // Add these services in Program.cs
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("ChefOnly", policy =>
                    policy.RequireRole("Chef"));

                options.AddPolicy("WaiterOrCashier", policy =>
                    policy.RequireRole("Waiter", "Cashier"));

                options.AddPolicy("StaffOnly", policy =>
                    policy.RequireRole("Admin", "Chef", "Waiter", "Cashier"));
            });

            var app = builder.Build();

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

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapRazorPages();

            app.MapHub<NotificationHub>("/notificationHub");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
