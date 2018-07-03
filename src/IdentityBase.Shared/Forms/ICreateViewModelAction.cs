// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
    using System.Threading.Tasks;

    public interface ICreateViewModelAction
    {
        int Step { get; }
        Task Execute(CreateViewModelContext context);
    }
}
