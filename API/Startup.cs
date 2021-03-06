using System.Text;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.

        // This is our DI container. If we want to make a class/service avaiable to other areas of our application, we
        // can add them inside this container and .NET Core will take care of the creation and injection of these
        // classes.
        public void ConfigureServices(IServiceCollection services)
        {
            // AddSingleton => Created and doesn't stop until app stops
            // AddScoped => Scoped to lifetime of HTTP request. For the most part when dealin with APIs, this is the
            //              scope you'll use.
            // AddTransient => Created and destroyed as soon as method is finished

            // The reason to create the interface is two-fold:
            //  - makes it very easy to mock the service for testing purpsoes
            //  - best practice
            services.AddScoped<ITokenService, TokenService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenKey"])),
                        ValidateIssuer = false,   /* issuer is our API server */
                        ValidateAudience = false  /* audience is our Angular frontend */
                    };
                });

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(_config.GetConnectionString("DefaultConnection"));
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        // As we make our GET request to our controller endpoint, the request goes through a series of middleware on
        // the way in and on the way out.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // This configures a developer exception page if using a dev env.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            // If using HTTP, redirect to HTTPS endpoint
            app.UseHttpsRedirection();

            // Sets up routing to controller endpoints
            app.UseRouting();

            // UseCors must be between UseRouting and UseEndpoints. However, when using Authorization, it must also
            // be before UseAuthorization().
            app.UseCors(policy => policy.AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .WithOrigins("https://localhost:4200")
            );

            app.UseAuthentication();
            // Not doing much at the moment since we have not configured authorization.
            app.UseAuthorization();

            // Middleware to actually use the endpoints and to map to the controllers.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
