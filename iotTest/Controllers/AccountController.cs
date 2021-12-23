using Google.Apis.Auth.OAuth2;
using Google.Apis.CloudIot.v1;
using Google.Apis.CloudIot.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iotTest.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        public static CloudIotService CreateAuthorizedClient()
        {
            GoogleCredential credential =
                GoogleCredential.GetApplicationDefaultAsync().Result;
            // c
            if (credential.IsCreateScopedRequired)
            {
                credential = credential.CreateScoped(new[]
                {
                    CloudIotService.Scope.CloudPlatform // Used for IoT + PubSub + IAM
                    //CloudIotService.Scope.Cloudiot // Can be used if not accessing Pub/Sub
                });
            }
            return new CloudIotService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                GZipEnabled = false
            });
        }

        [Route("google-login")]
        public async Task<IActionResult> GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            var x = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme); ;
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [Route("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var x = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme); ;

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = result.Ticket.Properties.Items.Select(claim => new
                {
                    claim.Key,
                    claim.Value,
                });

            return Json(claims);
        }

        [Route("create-device")]
        public async Task<IActionResult> GoogleDevice(string projectId, string cloudRegion, string registryId, string pubsubTopic)
        {
            var cloudIot = CreateAuthorizedClient();
            // The resource name of the location associated with the key rings.
            var parent = $"projects/{projectId}/locations/{cloudRegion}";

            Console.WriteLine(parent);

            try
            {
                Console.WriteLine($"Creating {registryId}");


                DeviceRegistry body = new DeviceRegistry()
                {
                    Id = registryId,
                };
                body.EventNotificationConfigs = new List<EventNotificationConfig>();
                var toAdd = new EventNotificationConfig()
                {
                    PubsubTopicName = pubsubTopic.StartsWith("projects/") ?
                        pubsubTopic : $"projects/{projectId}/topics/{pubsubTopic}",
                };
                body.EventNotificationConfigs.Add(toAdd);
                var registry = cloudIot.Projects.Locations.Registries.Create(body, parent).Execute();
                Console.WriteLine("Registry: ");
                Console.WriteLine($"{registry.Id}");
                Console.WriteLine($"\tName: {registry.Name}");
                Console.WriteLine($"\tHTTP Enabled: {registry.HttpConfig.HttpEnabledState}");
                Console.WriteLine($"\tMQTT Enabled: {registry.MqttConfig.MqttEnabledState}");
            }
            catch (Google.GoogleApiException e)
            {
                Console.WriteLine(e.Message);
                if (e.Error != null) return Json(e.Error.Code);
                return Json(-1);
            }
            return Json(0);
        }
    }
}