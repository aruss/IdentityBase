using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityBase.Public.Api
{
    [Area("Api")]
    public class StatusController : Controller
    {
        public StatusController()
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
