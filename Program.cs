using Business_Logic_Layer.Services;
using Data_Access_Layer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using StudentRegistration;
using StudentRegistration.Data;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<StudentDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));
builder.Services.AddTransient(typeof(IRepository<>),typeof(Repository<>));
builder.Services.AddScoped<IStudentService,StudentService>();
builder.Services.AddAutoMapper(typeof(mapConfig));


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
