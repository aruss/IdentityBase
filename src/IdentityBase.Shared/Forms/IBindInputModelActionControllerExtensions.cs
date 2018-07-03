// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Extensions;
    using System.Reflection;
    using ServiceBase.Plugins;
    using System.Linq;

    public static class IBindInputModelActionControllerExtensions
    {
        public static async Task<CreateViewModelResult> 
            CreateViewModel<TCreateViewModelAction>(
            this ControllerBase controller)
            where TCreateViewModelAction : ICreateViewModelAction
        {
            CreateViewModelContext context = new CreateViewModelContext(
                controller);

            IEnumerable<TCreateViewModelAction> actions = controller
               .HttpContext
               .RequestServices
               .GetServices<TCreateViewModelAction>();


            actions.ElementAt(0).GetType()
                .GetCustomAttributes<DependsOnPluginAttribute>(true)
                .Select(s => s.GetType())
                .ExpandInterfaces()




            actions.TopologicalSort(x => x.GetType()
                 .GetCustomAttributes<DependsOnPluginAttribute>(true)
                .Select(s => s.GetType())
                .ExpandInterfaces());
               

            // TODO: filter by step and sort topologically

            foreach (var formComponent in actions)
            {
                await formComponent.Execute(context);

                if (context.Cancel)
                {
                    break;
                }
            }

            // TODO: soft FormElements topologically
            return new CreateViewModelResult(
                context.Items,
                context.FormElements
            );
        }

        public async static Task<BindInputModelResult>
            BindInputModel<TBindInputModelAction>(
            this ControllerBase controller)
            where TBindInputModelAction : IBindInputModelAction
        {
            BindInputModelContext context =
                new BindInputModelContext(controller);

            IEnumerable<TBindInputModelAction> actions = controller
                .HttpContext
                .RequestServices
                .GetServices<TBindInputModelAction>();

            // TODO: filter by step and sort topologically

            foreach (var formComponent in actions)
            {
                await formComponent.Execute(context);

                if (context.Cancel)
                {
                    break;
                }
            }

            return new BindInputModelResult(context.Items);
        }

        public static async Task HandleInputModel<TIHandleInputModelAction>(
            this ControllerBase controller)
            where TIHandleInputModelAction : IHandleInputModelAction
        {
            HandleInputModelContext context = new HandleInputModelContext(
                controller);

            IEnumerable<TIHandleInputModelAction> actions = controller
                .HttpContext
                .RequestServices
                .GetServices<TIHandleInputModelAction>();

            // TODO: filter by step and sort topologically
            foreach (var formComponent in actions)
            {
                await formComponent.Execute(context);

                if (context.Cancel)
                {
                    break;
                }
            }
        }
    }
}
