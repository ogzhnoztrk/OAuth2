using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using OAuth2.Api.Database;
using OAuth2.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.Configure<OAuth2.Api.Database.MongoDbSettings>(o =>
{
    o.ConnectionString = builder.Configuration.GetSection("MongoDbConnectionString:ConnectionString").Value;
    o.DatabaseName = builder.Configuration.GetSection("MongoDbConnectionString:DatabaseName").Value;
});


builder.Services.AddIdentity<User, MongoIdentityRole>()
.AddMongoDbStores<User, MongoIdentityRole, Guid>
(
    builder.Configuration.GetSection("MongoDbConnectionString:ConnectionString").Value,
    builder.Configuration.GetSection("MongoDbConnectionString:DatabaseName").Value
)
.AddDefaultTokenProviders();


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
