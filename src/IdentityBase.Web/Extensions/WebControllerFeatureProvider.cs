// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Collections.Generic;
    using System.Reflection;
    using IdentityBase.Actions.Recover;
    using IdentityBase.Actions.Register;
    using IdentityBase.Configuration;
    using IdentityBase.Mvc;

    /// <summary>
    /// Discovers WebControllers from a list of <see cref="ApplicationPart"/>
    /// instances and adds feature controllers according to application
    /// configuration.
    /// </summary>
    public class WebControllerFeatureProvider :
        TypedControllerFeatureProvider<WebController>
    {
        private readonly List<TypeInfo> _blackList;

        public WebControllerFeatureProvider(ApplicationOptions options)
        {
            this._blackList = new List<TypeInfo>();
            
            this.AddIf<RecoverController>(!options.EnableAccountRecovery);
            this.AddIf<RegisterController>(!options.EnableAccountRegistration);
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return base.IsController(typeInfo) &&
                !this._blackList.Contains(typeInfo);
        }

        private void AddIf<TController>(bool assertion)
            where TController : WebController
        {
            if (assertion)
            {
                this._blackList.Add(typeof(TController).GetTypeInfo());
            }
        }
    }
}
