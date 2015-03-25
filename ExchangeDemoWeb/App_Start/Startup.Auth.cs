using ExchangeDemoWeb.Models;
using ExchangeDemoWeb.Utils;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using System.Web;

namespace ExchangeDemoWeb
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                //Implement our own cookie manager to work around the infinite
                //redirect loop issue
                CookieManager = new SystemWebCookieManager()
            });

            string clientID = ConfigurationManager.AppSettings["ida:ClientID"];
            string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
            string clientSecret = ConfigurationManager.AppSettings["ida:AppKey"];

            string webAPIResourceID = ConfigurationManager.AppSettings["webAPIResourceID"];

            string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientID,
                    Authority = authority,

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        // when an auth code is received...
                        AuthorizationCodeReceived = (context) =>
                        {
                            // get the OpenID Connect code passed from Azure AD on successful auth
                            string code = context.Code;

                            // create the app credentials & get reference to the user
                            ClientCredential creds = new ClientCredential(clientID, clientSecret);                            
                            string signInUserId = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                            // use the OpenID Connect code to obtain access token & refresh token...
                            //  save those in a persistent store...
                            AuthenticationContext authContext = new AuthenticationContext(authority, new ADALTokenCache(signInUserId));

                            // obtain access token for the Web API
                            Uri redirectUri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));                            
                            AuthenticationResult authResult = authContext.AcquireTokenByAuthorizationCode(code, redirectUri, creds, webAPIResourceID);
                        
                            // successful auth                            
                            return Task.FromResult(0);
                        },
                        AuthenticationFailed = (context) =>
                        {
                            context.HandleResponse();
                            return Task.FromResult(0);
                        }
                    }

                });
        }
    }
}