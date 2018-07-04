// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{    /// <summary>
     /// Represents one form element, like text box or recaptcha widget
     /// </summary>
    public class FormElement
    {
        public string Name { get; set; }

        public string Before { get; set; }

        /// <summary>
        /// The name or path of the view that is rendered to the response.
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// The model that is rendered by the view. 
        /// </summary>
        public object Model { get; set; }
    }
}
