using ExchangeDemoWeb.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Office365.Discovery;
using Microsoft.Office365.OutlookServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ExchangeDemoWeb.Controllers
{
    [Authorize]
    public class MailController : Controller
    {
        // GET: Mail
        public async Task<ActionResult> Index()
        {
            string clientID = ConfigurationManager.AppSettings["ida:ClientID"];
            string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
            string clientSecret = ConfigurationManager.AppSettings["ida:AppKey"];
            string graphResourceID = ConfigurationManager.AppSettings["ida:GraphResourceID"];


            string discoveryResourceID = "https://api.office.com/discovery/";
            string discoveryServiceEndpointUri = "https://api.office.com/discovery/v1.0/me/";

            string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            List<MyMessage> myMessages = new List<MyMessage>();

            var signInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            //Create an authentication context from cache
            AuthenticationContext authContext = new AuthenticationContext(authority, new ADALTokenCache(signInUserId));

            try
            {
                DiscoveryClient discClient = new DiscoveryClient(new Uri(discoveryServiceEndpointUri),
                    async () =>
                    {
                        //Get an access token to the discovery service
                        var authResult = await authContext.AcquireTokenSilentAsync(discoveryResourceID, new ClientCredential(clientID, clientSecret), new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));

                        return authResult.AccessToken;
                    });

                var dcr = await discClient.DiscoverCapabilityAsync("Mail");

                OutlookServicesClient exClient = new OutlookServicesClient(dcr.ServiceEndpointUri,
                    async () =>
                    {
                        //Get an access token to the Messages
                        var authResult = await authContext.AcquireTokenSilentAsync(dcr.ServiceResourceId, new ClientCredential(clientID, clientSecret), new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));

                        return authResult.AccessToken;
                    });

                var messagesResult = await exClient.Me.Messages.ExecuteAsync();

                do
                {
                    var messages = messagesResult.CurrentPage;
                    foreach (var message in messages)
                    {

                        myMessages.Add(new MyMessage
                        {
                            Subject = message.Subject,
                            From = message.Sender.EmailAddress.Address
                        });
                    }

                    messagesResult = await messagesResult.GetNextPageAsync();

                } while (messagesResult != null);
            }
            catch (AdalException exception)
            {
                //handle token acquisition failure
                if (exception.ErrorCode == AdalError.FailedToAcquireTokenSilently)
                {
                    authContext.TokenCache.Clear();

                    //handle token acquisition failure
                }
            }

            return View(myMessages);
        }

    }
}