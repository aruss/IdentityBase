namespace IdentityBase
{
    using System;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.Controllers;

    public class FromAssemblyFeatureProvider : ControllerFeatureProvider
    {
        public static FromAssemblyFeatureProvider WithAssemblyOf<TFoo>()
        {
            return new FromAssemblyFeatureProvider(typeof(TFoo)
              .GetTypeInfo().Assembly);
        }

        private Assembly _assembly;

        public FromAssemblyFeatureProvider(Assembly assembly)
        {
            this._assembly = assembly ?? throw new ArgumentNullException();
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return typeInfo.Assembly == this._assembly &&
                base.IsController(typeInfo);
        }
    }
}
