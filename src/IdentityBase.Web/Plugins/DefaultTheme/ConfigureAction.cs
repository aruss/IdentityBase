namespace DefaultTheme
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using ServiceBase.Mvc.Plugins;

    public class ConfigureAction : IConfigureAction
    {
        public void Execute(IApplicationBuilder applicationBuilder)
        {
            Console.WriteLine("DefaultTheme execute ConfigureAction");
        }
    }
}
