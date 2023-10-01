using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication7.DAL;
using WebApplication7.DAL.Repositories;
using WebApplication7.Infrastructure;
using ConnectingLib;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// add mapper
var config = new MapperConfiguration(c => c.AddProfile(new MappingProfile()));
IMapper mapper = config.CreateMapper();

builder.Services.AddDbContext<BlogContext>(options => options.UseSqlite(SQLiteBaseBuilder.GetConnectionString(AppDomain.CurrentDomain)))
    .AddScoped<IAuthorRepository, AuthorRepository>()
    .AddScoped<IRoleRepository, RoleRepository>()
    .AddScoped<IArticleRepository, ArticleRepository>()
    .AddScoped<ICommentRepository, CommentRepository>()
    .AddScoped<ITagRepository, TagRepository>()
    .AddSingleton(mapper);

builder.Services.AddAuthentication(options => options.DefaultScheme = "Cookies").AddCookie("Cookies", options => 
{
    options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
    {
        OnRedirectToLogin = redirectContext =>
        {
            redirectContext.HttpContext.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();