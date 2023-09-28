using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication7.DAL;
using WebApplication7.DAL.Repositories;
using WebApplication7.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// create connection
var folderPath = AppDomain.CurrentDomain.BaseDirectory + "/BlogDatabase.db";

if(!File.Exists(folderPath))
{
    File.Create(folderPath);
}

var connectionStr = "Data Source=" + folderPath;

// add mapper
var config = new MapperConfiguration(c => c.AddProfile(new MappingProfile()));
IMapper mapper = config.CreateMapper();

builder.Services.AddDbContext<BlogContext>(options => options.UseSqlite(connectionStr))
    .AddScoped<IAuthorRepository, AuthorRepository>()
    .AddScoped<IRoleRepository, RoleRepository>()
    .AddScoped<IArticleRepository, ArticleRepository>()
    .AddScoped<ICommentRepository, CommentRepository>()
    .AddScoped<ITagRepository, TagRepository>()
    .AddSingleton(mapper);

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

app.UseAuthorization();

app.MapControllers();

app.Run();