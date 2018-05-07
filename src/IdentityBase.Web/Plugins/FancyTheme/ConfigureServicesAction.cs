namespace FancyTheme
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Plugins;

    public class ConfigureServicesAction : IConfigureServicesAction
    {
        public void Execute(IServiceCollection services)
        {
            Console.WriteLine("FancyTheme execute ConfigureServicesAction");

            //return Task.CompletedTask;
        }
    }
}
