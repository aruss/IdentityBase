// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Extensions
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using ServiceBase.Extensions;

    public static partial class StringExtensions
    {
        private static readonly string[] UglyBase64 = { "+", "/", "=" };

        [DebuggerStepThrough]
        public static string StripUglyBase64(this string s)
        {
            if (String.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            foreach (var ugly in UglyBase64)
            {
                s = s.Replace(ugly, String.Empty);
            }

            return s;
        }

        [DebuggerStepThrough]
        public static string GetFullPath(this string path, string rootPath)
        {
            if (!Path.IsPathRooted(path))
            {
                return Path.GetFullPath(
                    Path.Combine(
                        rootPath.RemoveTrailingSlash(),
                        path.RemoveLeadingSlash())
                    );
            }

            return Path.GetFullPath(path);
        }
    }
}