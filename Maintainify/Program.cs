using Maintainify.Core;
using Maintainify.Extensions;
using Maintainify.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMvc(o => o.EnableEndpointRouting = false);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("url")));
// api Services
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true); // validation Error Api
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddSignalR();
builder.Services.AddCors(options => {
    options.AddPolicy("CORSPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed((hosts) => true));
});
// context && json services && IBaseRepository && IUnitOfWork Service
builder.Services.AddContextServices(builder.Configuration);
builder.Services.AddControllersWithViews();

// Services [IAccountService, IPhotoHandling, AddAutoMapper, Hangfire ,
// Session , SignalR ,[ INotificationService, FcmNotificationSetting, FcmSender,ApnSender ]  ]
builder.Services.AddApplicationServices(builder.Configuration);

// Identity services && JWT
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddHttpClient();

// Swagger Service
builder.Services.AddSwaggerDocumentation();
var app = builder.Build();

// Configure the HTTP request pipeline.
//;

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseMiddleware<ExceptionMiddleware>();
}
app.UseSwaggerDocumentation();
app.UseCors("CORSPolicy");
app.UseRouting();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();

app.UseApplicationMiddleware();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Account}/{action=Login}/{id?}");
});

app.Run();
