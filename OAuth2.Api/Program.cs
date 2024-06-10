using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OAuth2.Api.Database;
using OAuth2.Api.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




//jwt ile ilgili ayarlar
builder.Services.Configure<JwtModel>(o =>
{
    o.Issuer = builder.Configuration["Jwt:Issuer"];
    o.Audience = builder.Configuration["Jwt:Audience"];
    o.Key = builder.Configuration["Jwt:Key"];
});

//JwtBearer token ile ilgili ayarlar
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,


    };
});





//mongodb ile ilgili ayarlar
builder.Services.Configure<OAuth2.Api.Database.MongoDbSettings>(o =>
{
    o.ConnectionString = builder.Configuration.GetSection("MongoDbConnectionString:ConnectionString").Value;
    o.DatabaseName = builder.Configuration.GetSection("MongoDbConnectionString:DatabaseName").Value;
});

//microsoft identity mongodb versiyonu ile ilgili ayarlar
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
app.UseAuthorization();

app.MapControllers();

app.Run();
