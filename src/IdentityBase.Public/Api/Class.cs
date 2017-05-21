using Microsoft.AspNetCore.Mvc;

namespace IdentityBase.Public.Api
{
    [Area("publicapi")]
    public class FooController : Controller
    {

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
