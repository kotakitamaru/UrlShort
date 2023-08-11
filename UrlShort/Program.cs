using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "Policy",
        policy  =>
        {
            policy.WithOrigins("https://surl-e5af9.web.app", "https://surl-e5af9.firebaseapp.com")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config.GetSection("JwtSettings:Key").Value!)),
        ValidateLifetime = false,
        ValidateIssuerSigningKey = false,
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => 
        p.RequireClaim("isAdmin", "True"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Policy");
app.UseAuthorization();

app.MapControllers();

app.Run();
