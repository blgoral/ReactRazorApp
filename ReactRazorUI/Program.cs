using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["OIDC:Authority"];
        options.ClientId = builder.Configuration["OIDC:ClientId"];
        options.ClientSecret = builder.Configuration["OIDC:ClientSecret"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("OIDC:RequireHttpsMetaData");
        options.SaveTokens = builder.Configuration.GetValue<bool>("OIDC:SaveTokens");
        options.GetClaimsFromUserInfoEndpoint = builder.Configuration.GetValue<bool>("OIDC:GetClaimsFromUserEndpoint");
        options.ResponseType = OpenIdConnectResponseType.Code;
        // Return a 401 instead of redirecting if an API route is not authorized
        options.Events = new OpenIdConnectEvents()
        {
            OnRedirectToIdentityProvider = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    if (context.Response.StatusCode == (int)HttpStatusCode.OK)
                    {
                        context.Response.StatusCode = 401;
                    }
                    context.HandleResponse();
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();