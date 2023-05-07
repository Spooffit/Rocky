using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Initializer;
using Rocky_DataAccess.Repository;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utility;
using Rocky_Utility.BrainTree;
using System.Configuration;

namespace Rocky
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["DefaultConnection"]);
            });

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["Syncfusion_License"]);

            builder.Services.Configure<BrainTreeSettings>(options =>
            {
                options.Environment = "sandbox";
                options.MerchantId = builder.Configuration["BrainTree_MerchantId"] ?? "";
                options.PublicKey = builder.Configuration["BrainTree_PublicKey"] ?? "";
                options.PrivateKey = builder.Configuration["BrainTree_PrivateKey"] ?? "";
            });

            builder.Services.AddSingleton<IBrainTreeGate, BrainTreeGate>();

            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IApplicationTypeRepository, ApplicationTypeRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            builder.Services.AddScoped<IInquiryHeaderRepository, InquiryHeaderRepository>();
            builder.Services.AddScoped<IInquiryDetailRepository, InquiryDetailRepository>();

            builder.Services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
            builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

            builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

            builder.Services.AddScoped<IDbInitializer, DbInitializer>();

            var googleClient = new GoogleClient
            {
                ClientId = builder.Configuration["GoogleClient_Id"] ?? "",
                ClientSecret = builder.Configuration["GoogleClient_Secret"] ?? "",

            };

            builder.Services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = googleClient.ClientId;
                options.ClientSecret = googleClient.ClientSecret;
            });

            builder.Services.AddControllersWithViews();


            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".Rocky.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                dbInitializer.Initialize();
            }

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}