﻿using Microsoft.Owin;
using Owin;
using System;
using System.Linq;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using VlaboralApi.Infrastructure;
using VlaboralApi.Providers;
using VLaboralApi.Models;

[assembly: OwinStartup(typeof(VLaboralApi.Startup))] // en este atributo indico la clase que se dispara una vez que arranca el servidor
namespace VLaboralApi
{
    public class Startup
    {


        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            ConfigureOAuthTokenGeneration(app);

            ConfigureOAuthTokenConsumption(app);
            app.UseCors(CorsOptions.AllowAll);

            //var hubConfiguration = new HubConfiguration
            //{
            //    EnableDetailedErrors = true,
            //    EnableJavaScriptProxies = false,
            //};

            //app.MapSignalR("/signalr", hubConfiguration);

            // app.MapSignalR();

            ConfigureWebApi(httpConfig);

            app.UseWebApi(httpConfig);

            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);

                map.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
                {
                    Provider = new QueryStringOAuthBearerProvider()
                });

                var hubConfiguration = new HubConfiguration
                {
                    Resolver = GlobalHost.DependencyResolver,
                };
                map.RunSignalR(hubConfiguration);
            });


        }

        public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
        {
            public override Task RequestToken(OAuthRequestTokenContext context)
            {
                var value = context.Request.Query.Get("access_token");

                if (!string.IsNullOrEmpty(value))
                {
                    context.Token = value;
                }

                return Task.FromResult<object>(null);
            }
        }

        private static void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            //app.CreatePerOwinContext(AuthDbContext.Create);
            app.CreatePerOwinContext(VLaboral_Context.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                //For Dev enviroment only (on production should be AllowInsecureHttp = false)
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat(ConfigurationManager.AppSettings["urlApi"]) //fpaz: url del WebApi que se toma desde las configuraciones en el webconfig                 
                
            };

            // OAuth 2.0 Bearer Access Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
        }

        private static void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {

            var issuer = ConfigurationManager.AppSettings["urlApi"]; //fpaz: url del WebApi que se toma desde las configuraciones en el webconfig                             
            var audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            var audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[]
                    {
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
                });
        }

        private static void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}