using ExchangeDemoWeb.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
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
            var myMessages = new List<MyMessage>();

            string clientID = ConfigurationManager.AppSettings["ida:ClientID"];
            string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
            string clientSecret = ConfigurationManager.AppSettings["ida:AppKey"];            
            
            string webAPIResourceID = ConfigurationManager.AppSettings["webAPIResourceID"];
            string webAPIEndpoint = ConfigurationManager.AppSettings["webAPIEndpoint"];

            string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            
            var signInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            
            try
            {
                var clientCredential = new ClientCredential(clientID, clientSecret);
                AuthenticationContext authContext = new AuthenticationContext(authority, new ADALTokenCache(signInUserId));
                var authResult = await authContext.AcquireTokenAsync(
                    webAPIResourceID,
                    clientCredential,
                    new UserAssertion(userObjectId, UserIdentifierType.UniqueId.ToString()));
                
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, webAPIEndpoint);
                request.Headers.TryAddWithoutValidation("Authorization", authResult.CreateAuthorizationHeader());
                var response = await client.SendAsync(request);

                var responseString = await response.Content.ReadAsStringAsync();

                var responseMessages = JsonConvert.DeserializeObject<IEnumerable<MyMessage>>(responseString);
                myMessages = new List<MyMessage>(responseMessages);
            }
            catch(Exception oops)
            {
                throw oops;
            }

            return View(myMessages);
        }

    }
}