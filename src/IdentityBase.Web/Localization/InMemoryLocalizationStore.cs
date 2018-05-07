// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class InMemoryLocalizationStore : ILocalizationStore
    {
        private static ConcurrentDictionary
            <string, Dictionary<string, string>> _dictionaries =
                new ConcurrentDictionary<string, Dictionary<string, string>>();

        public Task<string> GetAsync(string culture, string name)
        {
            Dictionary<string, string> dict = InMemoryLocalizationStore.
                _dictionaries.GetOrAdd(culture, (c) =>
                {
                    return new Dictionary<string, string>();
                });

            if (dict.ContainsKey(name))
            {
                return Task.FromResult(dict[name]);
            }

            return Task.FromResult<string>(null);
        }
    }
}
