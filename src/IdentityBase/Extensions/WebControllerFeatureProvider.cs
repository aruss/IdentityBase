// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Collections.Generic;
    using System.Reflection;
    using IdentityBase.Actions;
    using IdentityBase.Actions.Recover;
    using IdentityBase.Actions.Register;
    using IdentityBase.Configuration;

    /// <summary>
    /// Discovers WebControllers from a list of <see cref="ApplicationPart"/>
    /// instances and adds feature controllers according to application
    /// configuration.
    /// </summary>
    public class WebControllerFeatureProvider :
        TypedControllerFeatureProvider<WebController>
    {
        private List<TypeInfo> blackList;

        public WebControllerFeatureProvider(ApplicationOptions options)
        {
            this.blackList = new List<TypeInfo>();
            
            this.AddIf<RecoverController>(!options.EnableAccountRecovery);
            this.AddIf<RegisterController>(!options.EnableAccountRegistration);
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return base.IsController(typeInfo) &&
                !this.blackList.Contains(typeInfo);
        }

        private void AddIf<TController>(bool assertion)
            where TController : WebController
        {
            if (assertion)
            {
                this.blackList.Add(typeof(TController).GetTypeInfo());
            }
        }
    }
}
