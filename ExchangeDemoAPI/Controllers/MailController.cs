using ExchangeDemoAPI.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Office365.Discovery;
using Microsoft.Office365.OutlookServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ExchangeDemoAPI.Controllers
{
    [Authorize]
    public class MailController : ApiController
    {
        public async Task<IHttpActionResult> GetMessages()
        {
            string clientID = ConfigurationManager.AppSettings["ida:ClientID"];
            string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
            string clientSecret = ConfigurationManager.AppSettings["ida:AppKey"];            

            //string graphResourceID = "https://graph.windows.net";
            string discoveryResourceID = "https://api.office.com/discovery/";
            string discoveryServiceEndpointUri = "https://api.office.com/discovery/v1.0/me/";

            string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            List<MyMessage> myMessages = new List<MyMessage>();

            var signInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;            

            //Get the access token from the request and form a new user assertion
            string authHeader = HttpContext.Current.Request.Headers["Authorization"];
            string userAccessToken = authHeader.Substring(authHeader.LastIndexOf(' ')).Trim();
            UserAssertion userAssertion = new UserAssertion(userAccessToken);

            //Create an authentication context from cache
            AuthenticationContext authContext = new AuthenticationContext(
                authority,
                new ADALTokenCache(signInUserId));
            
            try
            {
                DiscoveryClient discClient = new DiscoveryClient(new Uri(discoveryServiceEndpointUri),
                    async () =>
                    {
                        //Get an access token to the discovery service asserting the
                        //credentials of the caller... this is how we achieve "on behalf of"
                        var authResult = await authContext.AcquireTokenAsync(
                            discoveryResourceID, 
                            new ClientCredential(clientID, clientSecret), 
                            userAssertion);

                        return authResult.AccessToken;
                    });

                var dcr = await discClient.DiscoverCapabilityAsync("Mail");

                OutlookServicesClient exClient = new OutlookServicesClient(dcr.ServiceEndpointUri,
                    async () =>
                    {
                        //Get an access token to the Messages asserting the
                        //credentials of the caller... this is how we achieve "on behalf of"
                        var authResult = await authContext.AcquireTokenAsync(
                            dcr.ServiceResourceId, 
                            new ClientCredential(clientID, clientSecret), 
                            userAssertion);

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
                throw exception;
            }

            return Ok(myMessages);
        }
    }
}
