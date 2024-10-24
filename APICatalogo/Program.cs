using System.Text;
using System.Text.Json.Serialization;
using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Logging;
using APICatalogo.Models;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using APICatalogo.RateLimitOptions;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add(typeof(ApiExceptionFilter));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddNewtonsoftJson();

builder.Services.AddCors(options =>
{
    // essa é uma política nomeada. 
    options.AddPolicy(
        name: "OriginsWithAccess",
        policy =>
        {
            policy.WithOrigins("http://localhost:")
            .WithMethods("GET", "POST")
            .AllowAnyHeader();
        }
    );

    // essa é uma política padrão, que não precisa ser nomeada
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("https://apirequest.io")
            .WithMethods("GET", "POST")
            .AllowAnyHeader();
        }
    );
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "apicatalogo", Version = "v1" });

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Bearer JWT",
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },

                new string[] { }
            }
        }
    );
});

string? SqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(SqlConnection));
builder.Services.AddScoped<ApiLoggingFilter>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutosRepository, ProdutosRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingProfile));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Adim"))
    .AddPolicy("SuperAdminOnly", policy => policy.RequireRole("Adim").RequireClaim("id", "Alex"))
    .AddPolicy("User", policy => policy.RequireRole("User"))
    .AddPolicy(
        "ExcluseviOnly",
        policy => policy.RequireAssertion(
            context => context.User.HasClaim(
                claim => claim.Type == "id" &&
                claim.Value == "Alex" ||
                context.User.IsInRole("SuperAdmin")
            )
        )
    );

builder.Services.AddScoped<ITokenService, TokenService>();

// Configurando o Identity para gerar as tabelas de logon dos usu�rios
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Rate Limit -> controla à quantidade de requisições que um usuário pode fazer um um determinado período

var rateLimit = new MyRateLimitOptions();
builder.Configuration.GetSection(MyRateLimitOptions.RateLimitParameters).Bind(rateLimit);

builder.Services.AddRateLimiter(rateLimitOption =>
{
    rateLimitOption.AddFixedWindowLimiter("fixedWindow", options =>
    {
        options.PermitLimit = rateLimit.PermitLimit;
        options.Window = TimeSpan.FromSeconds(rateLimit.Window);
        options.QueueLimit = rateLimit.QueueLimit;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    rateLimitOption.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter
        .Create<HttpContext, string>(
            httpcontext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpcontext.User.Identity?.Name ??
                    httpcontext.Request.Headers.Host.ToString(),

                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 2,
                    QueueLimit = 0,
                    Window = TimeSpan.FromSeconds(10)
                }
            )
        );
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader(),
        new UrlSegmentApiVersionReader()
    );

}).AddApiExplorer(op =>
{
    op.GroupNameFormat = "'v'VVV";
    op.SubstituteApiVersionInUrl = true;
});

var secreteKey =
    builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid Secret Key!");

//builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services
    .AddAuthentication(op =>
    {
        op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(op =>
    {
        // salvar o token caso a operação seja bem sucedida
        op.SaveToken = true;

        // permitir que os dados possam ser trafegados somente em rotas https.
        // em produ��o normalmente esse paremetro deve ficar como true
        op.RequireHttpsMetadata = false;
        op.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidAudience = builder.Configuration["JWT:ValidAudiance"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes((secreteKey)))
        };
    });

// aqui nos adicionamos aos serviços da aplicação a classe CustomLoggerProvider
builder.Logging
    .AddProvider(
        new CustomLoggerProvider(
            new CustomLoggerProviderConfiguration { LogLevel = LogLevel.Information }
        )
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();
app.UseCors();

app.UseAuthorization();
app.MapControllers();
app.Run();
