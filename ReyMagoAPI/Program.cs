using Microsoft.EntityFrameworkCore;
using FamilyApp.Data;
using NuGet.Protocol.Core.Types;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//COnexion a la BAse de Datos
builder.Services.AddDbContext<dbContext>
    (options => { options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")); });

//agregamos los repositorios
//builder.Services.AddTransient<IFichaEgresoRepository, FichaEgresoRepositorio>();
//builder.Services.AddTransient<IEgresoRepository, EgresoRepositorio>();
//builder.Services.AddTransient<IRepository, Repository>();
builder.Services.AddScoped<IRepository, Repository<dbContext>>();

////MAPPER
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


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
