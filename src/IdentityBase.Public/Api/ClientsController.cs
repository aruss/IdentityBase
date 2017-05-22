using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityBase.Public.Api
{
    [Area("Api")]
    public class ClientsController : Controller
    {
        public ClientsController()
        {

        }
        
        public object Get()
        {
            return new
            {
                Id = "foo",
                Name = "Bar"
            }; 
        }
    }
}
