using Microsoft.EntityFrameworkCore;
using RMS.Data;
using RMS.Services;

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

            // Add service interfaces
            builder.Services.AddScoped<IIngredientService, IngredientService>();

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
