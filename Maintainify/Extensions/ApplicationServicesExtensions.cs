using Maintainify.BusinessLayer.Interface;
using Maintainify.BusinessLayer.Services;
using Maintainify.Middleware;

namespace Maintainify.Extensions
{
    public static class ApplicationServicesExtensions
    {
        // interfaces sevices [IAccountService, IPhotoHandling,[ INotificationService, FcmNotificationSetting, FcmSender,ApnSender ], AddAutoMapper, hangfire  ]
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {


            // Session Service
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(12);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Application Service 
            services.AddScoped<IAccountService, AccountService>();
            services.AddTransient<IFileHandling, FileHandling>();
            //services.AddTransient<INotificationService, NotificationService>();
            //services.AddTransient<IPaymentService, PaymentService>();
            //services.Configure<FcmNotificationSetting>(config.GetSection("FcmNotification"));
            //services.Configure<PaymentSettings>(config.GetSection("Payments"));
            //services.Configure<ApplePublish>(config.GetSection("ApplePublish"));
            //services.AddHttpClient<FcmSender>();
            //services.AddHttpClient<ApnSender>();
            //services.AddAutoMapper(typeof(Program).Assembly);

            // Hangfire Service
            /*   services.AddHangfire(c =>
               {
                   c.UseSqlServerStorage(config.GetConnectionString("url"));
               });
               services.AddHangfireServer();

               // SignalR Service
               services.AddSignalR();*/

            return services;
        }

        public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app)
        {
            app.UseSession();
            return app;
        }

    }
}
