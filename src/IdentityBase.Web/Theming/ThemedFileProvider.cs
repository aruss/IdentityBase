// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.IO;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Primitives;
    using ServiceBase.Extensions;
    using System.Linq;
    using Microsoft.Extensions.FileProviders.Physical;
    using System.Collections.Generic;

    // https://github.com/aspnet/FileSystem/blob/e469b8f244e38c346ab6a64f363df4d638cf5cee/src/FS.Physical/PhysicalFileProvider.cs
    // https://raw.githubusercontent.com/aspnet/FileSystem/e469b8f244e38c346ab6a64f363df4d638cf5cee/src/FS.Physical/Internal/PathUtils.cs
    public class ThemedFileProvider : IFileProvider
    {
        private readonly string _themeBasePath;

        public ThemedFileProvider(string themeBasePath)
        {
            this._themeBasePath = themeBasePath;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath) ||
                PathUtils.HasInvalidPathChars(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            subpath = subpath.RemoveLeadingSlash();

            // Absolute paths not permitted.
            if (Path.IsPathRooted(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            string fullPath = this.GetFullPath(subpath);
            if (fullPath == null)
            {
                return new NotFoundFileInfo(subpath);
            }

            return new PhysicalFileInfo(new FileInfo(fullPath));
        }

        private string GetFullPath(string path)
        {
            List<string> chunks = path.Split("/").ToList();
            chunks.Insert(1, "Public");
            path = Path.Combine(chunks.ToArray()); 
            
            if (PathUtils.PathNavigatesAboveRoot(path))
            {
                return null;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(Path.Combine(
                    this._themeBasePath, path));
            }
            catch
            {
                return null;
            }

            if (!this.IsUnderneathRoot(fullPath))
            {
                return null;
            }

            return fullPath;
        }

        private bool IsUnderneathRoot(string fullPath)
        {
            return fullPath.StartsWith(this._themeBasePath,
                StringComparison.OrdinalIgnoreCase);
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }
    }
}

namespace IdentityBase
{
    // Copyright (c) .NET Foundation. All rights reserved.
    // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Primitives;

    internal static class PathUtils
    {
        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars()
            .Where(c => c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar).ToArray();

        private static readonly char[] _invalidFilterChars = _invalidFileNameChars
            .Where(c => c != '*' && c != '|' && c != '?').ToArray();

        private static readonly char[] _pathSeparators = new[]
            {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};

        internal static bool HasInvalidPathChars(string path)
        {
            return path.IndexOfAny(_invalidFileNameChars) != -1;
        }

        internal static bool HasInvalidFilterChars(string path)
        {
            return path.IndexOfAny(_invalidFilterChars) != -1;
        }

        internal static string EnsureTrailingSlash(string path)
        {
            if (!string.IsNullOrEmpty(path) &&
                path[path.Length - 1] != Path.DirectorySeparatorChar)
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        internal static bool PathNavigatesAboveRoot(string path)
        {
            var tokenizer = new StringTokenizer(path, _pathSeparators);
            var depth = 0;

            foreach (var segment in tokenizer)
            {
                if (segment.Equals(".") || segment.Equals(""))
                {
                    continue;
                }
                else if (segment.Equals(".."))
                {
                    depth--;

                    if (depth == -1)
                    {
                        return true;
                    }
                }
                else
                {
                    depth++;
                }
            }

            return false;
        }
    }
}