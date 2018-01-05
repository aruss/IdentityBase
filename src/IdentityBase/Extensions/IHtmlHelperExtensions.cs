// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

    /// <summary>
    /// <see cref="IHtmlHelper"/> extension methods.
    /// </summary>
    public static class IHtmlHelperExtensions
    {
        /// <summary>
        /// Returns <see cref="ModelStateEntry"/> for the specified expression.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the model property.
        /// </typeparam>
        /// <param name="htmlHelper">An <see cref="IHtmlHelper"/> 
        /// for Linq expressions.</param>
        /// <param name="expression">An expression to be evaluated against the 
        /// current model.</param>
        /// <returns>A <see cref="ModelStateEntry"/></returns>
        public static ModelStateEntry ModelStateFor<TModel, TProperty>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression)
        {
            string expressionText =
                ExpressionHelper.GetExpressionText(expression);

            string fullHtmlFieldName = htmlHelper.ViewContext.ViewData
                .TemplateInfo.GetFullHtmlFieldName(expressionText);

            return htmlHelper.ViewData.ModelState[fullHtmlFieldName];
        }
    }
}

