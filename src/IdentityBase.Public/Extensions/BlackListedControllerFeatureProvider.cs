using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Controllers
{
    public class BlackListedControllerFeatureProvider : ControllerFeatureProvider
    {
        IEnumerable<TypeInfo> _blackList;

        public BlackListedControllerFeatureProvider(IEnumerable<TypeInfo> blackList)
        {
            _blackList = blackList;
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return base.IsController(typeInfo) && !_blackList.Contains(typeInfo);
        }
    }
}