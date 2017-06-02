using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreWeb.UI.InviteUser
{
    public class InviteUserController : Controller
    {
        [HttpGet("/invite-user")]
        [Authorize]
        public IActionResult Invite()
        {
            return View("Invite");
        }

        [HttpPost("/invite-user")]
        [Authorize]
        public async Task<IActionResult> Invited(InviteInputModel inputModel)
        {
            var accessToken = await HttpContext.Authentication.GetTokenAsync("access_token");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var model = new
            {
                Email = inputModel.Email
            };

            var response = await client.PutAsync("http://localhost:5000/api/users/_invite", model);

            var viewModel = new InviteViewModel(inputModel); 

            return View("Invited", viewModel);
        }
    }

    public class InviteInputModel
    {
        [Required]
        [EmailAddress]
        [Range(6, 254)]
        public string Email { get; set; }
    }
    public class InviteViewModel : InviteInputModel

    {
        public InviteViewModel(InviteInputModel inputModel)
        {
            this.Email = inputModel.Email; 
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PutAsync(this HttpClient client, string requestUrl, object model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            return await client.PutAsync(requestUrl, content);
        }
    }
}