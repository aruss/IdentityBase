namespace AspNetCoreWeb.Actions.Invite
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class InviteController : Controller
    {
        [HttpGet("/invite")]
        [Authorize]
        public IActionResult Invite()
        {
            return this.View("Invite");
        }

        [HttpPost("/invite")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Invited(InviteInputModel inputModel)
        {
            string accessToken = await HttpContext
                .GetTokenAsync("access_token");

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var model = new
            {
                Email = inputModel.Email,
                ClientId = "mvc.hybrid"
            };

            HttpResponseMessage response = await client.PutJsonAsync(
                "http://localhost:5000/api/invitations",
                model);

            response.EnsureSuccessStatusCode(); 

            var json = response.Content.ReadAsStringAsync().Result;

            InviteViewModel viewModel = new InviteViewModel(inputModel);

            return this.View("Invited", viewModel);
        }
    }
}
