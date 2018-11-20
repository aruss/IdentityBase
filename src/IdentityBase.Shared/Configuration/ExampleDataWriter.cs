// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using IdentityBase.Crypto;
    using IdentityBase.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// var configBuilder = new ConfigurationBuilder()
    ///     .SetBasePath(Directory.GetCurrentDirectory())
    ///     .AddJsonFile("./AppData/config.json", false, false)
    ///     .Build();
    /// 
    /// ExampleDataWriter.Write(configBuilder); 
    /// </summary>
    public class ExampleDataWriter
    {
        public static void Write()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./config/config.json", false, false)
                .Build();

            ExampleDataWriter.Write(configBuilder);
        }

        public static void Write(IConfiguration config)
        {
            ICryptoService crypto = new DefaultCryptoService();

            ApplicationOptions options = config.GetSection("App")
                .Get<ApplicationOptions>();

            ExampleDataWriter writer = new ExampleDataWriter(crypto, options);

            string configPath = config
                .GetSection("EntityFramework")["SeedExampleDataPath"];

            writer.WriteConfigFiles(configPath);
        }

        private readonly ICryptoService _cryptoService;
        private readonly ApplicationOptions _applicationOptions;

        public ExampleDataWriter(
            ICryptoService cryptoService,
            ApplicationOptions applicationOptions)
        {
            this._cryptoService = cryptoService;
            this._applicationOptions = applicationOptions;
        }

        public void WriteConfigFiles(string path)
        {
            ExampleData data = new ExampleData();

            File.WriteAllText(
                Path.Combine(path, "data_clients.json"),
                JsonConvert.SerializeObject(data.GetClients(),
                Formatting.Indented)
            );

            File.WriteAllText(
                Path.Combine(path, "data_resources_api.json"),
                JsonConvert.SerializeObject(data.GetApiResources(),
                Formatting.Indented)
            );

            File.WriteAllText(
                Path.Combine(path, "data_resources_identity.json"),
                JsonConvert.SerializeObject(data.GetIdentityResources(),
                Formatting.Indented)
            );

            IEnumerable<UserAccount> users = data.GetUserAccounts(
                       this._cryptoService,
                       this._applicationOptions);

            foreach (UserAccount user in users)
            {
                user.Accounts = user.Accounts ?? new ExternalAccount[0];

                foreach (ExternalAccount account in user.Accounts)
                {
                    account.UserAccountId = user.Id;
                }

                user.Claims = user.Claims ?? new UserAccountClaim[0];

                foreach (UserAccountClaim claim in user.Claims)
                {
                    claim.UserAccountId = user.Id;
                    claim.Id = Guid.NewGuid();
                }
            }

            File.WriteAllText(
                Path.Combine(path, "data_users.json"),
                JsonConvert.SerializeObject(users, Formatting.Indented)
            );
        }
    }
}