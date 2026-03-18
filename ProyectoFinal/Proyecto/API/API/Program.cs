using API;
using API.Middlewares;
using Business;
using Business.Authentication;
using Data;
using Domain.Authentication;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var storagePublicPath = builder.Configuration["Storage:PublicPath"];

if (storagePublicPath != null)
{
    var projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.ToString() ?? "";
    storagePublicPath = Path.Combine(projectDirectory, storagePublicPath);

    if (!Directory.Exists(storagePublicPath))
    {
        Directory.CreateDirectory(storagePublicPath);
    }

    builder.Configuration["Storage:PublicPath"] = storagePublicPath;
}

// Add services to the container.
builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalHostOrigin", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            new Uri(origin).Host == "localhost" ||
            new Uri(origin).Host == "127.0.0.1")
          .AllowAnyHeader()
          .AllowAnyMethod();

        policy.WithOrigins("http://127.0.0.1:5000")
            .AllowAnyHeader()
            .AllowAnyMethod();

        policy.WithOrigins("http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();

        policy.WithOrigins("http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod();

        policy.WithOrigins("http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.CustomSchemaIds(SwaggerHelper.SafeSchemaId);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LaVozEstudiantil - API",
        Version = "v1"
    });

    // Add JWT Bearer Auth
    options.AddSecurityDefinition(AuthScheme.User.ToSchemeName(), new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token de usuario."
    });

    options.OperationFilter<SwaggerAuthorizeCheckOperationFilter>();
});

var app = builder.Build();

app.UseWhen(context =>
{
    var path = context.Request.Path;
    
    return path.StartsWithSegments("/api")
        && !path.StartsWithSegments("/api/auth/generate-token")
        && !path.StartsWithSegments("/api/media");
}, appBuilder =>
{
    appBuilder.UseMiddleware<UserValidationMiddleware>();
});

if (storagePublicPath != null)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(storagePublicPath),
        RequestPath = "/api/media"
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.EnablePersistAuthorization();
        opt.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });

    app.UseCors("AllowLocalHostOrigin");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
