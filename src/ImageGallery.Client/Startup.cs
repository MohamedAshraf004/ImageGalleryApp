using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            // create an HttpClient used for accessing the API
            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44366/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44318/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
             {
                 options.AccessDeniedPath = "/Authorization/AccessDenied";
             })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
             {
                 options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                 options.Authority = "https://localhost:44318";
                 options.ResponseType = "code";
                 options.ClientId = "imagegallaryclient";
                 //options.UsePkce = false;

                 //theses are added by default

                 //options.CallbackPath=new PathString() if i want to change call back path
                 //options.SignedOutCallbackPath=new Microsoft.AspNetCore.Http.PathString() def /signout-callback-oidc
                 //options.Scope.Add("profile");
                 //options.Scope.Add("openid");

                 options.Scope.Add("address");
                 options.Scope.Add("roles");
                 //options.ClaimActions.Remove("nbf");
                 //Delete these nclaims from identity token for making it short and then adding these claims to 
                 // access token ==>userinfo
                 options.ClaimActions.DeleteClaim("auth_time");
                 options.ClaimActions.DeleteClaim("s_hash");
                 options.ClaimActions.DeleteClaim("idp");
                 options.ClaimActions.DeleteClaim("sid");
                 options.ClaimActions.MapUniqueJsonKey("role", "role");
                 /*options.ClaimActions.DeleteClaim("address"); address don't added by default in identity claims so no need to delete it */

                 options.SaveTokens = true;
                 options.ClientSecret = "secret";
                 options.GetClaimsFromUserInfoEndpoint = true;
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     NameClaimType = JwtClaimTypes.GivenName,
                     RoleClaimType = JwtClaimTypes.Role
                 };
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
                // The default HSTS value is 30 days. You may want to change this for
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
