namespace IdentityBase.Configuration
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using IdentityBase.Crypto;
    using IdentityBase.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class ExampleDataWriter
    {
        public static void Write(IConfiguration config)
        {
            var crypto = new Crypto.DefaultCrypto();
            var options = config.GetSection("App")
                .Get<Configuration.ApplicationOptions>();
            var writer = new Configuration.ExampleDataWriter(crypto, options);
            writer.WriteConfigFiles("./AppData");
        }

        private readonly ICrypto _crypto;
        private readonly ApplicationOptions _applicationOptions;

        public ExampleDataWriter(
            ICrypto crypto,
            ApplicationOptions applicationOptions)
        {
            this._crypto = crypto;
            this._applicationOptions = applicationOptions;
        }

        public void WriteConfigFiles(string path)
        {
            var data = new ExampleData();

            File.WriteAllText(
                Path.Combine(path, "data_clients.json"),
                JsonConvert.SerializeObject(data.GetClients(),
                    Formatting.Indented));

            File.WriteAllText(
                Path.Combine(path, "data_resources_api.json"),
                JsonConvert.SerializeObject(data.GetApiResources(),
                    Formatting.Indented));

            File.WriteAllText(
                Path.Combine(path, "data_resources_identity.json"),
                JsonConvert.SerializeObject(data.GetIdentityResources(),
                    Formatting.Indented));


            var users = data.GetUserAccounts(
                       this._crypto,
                       this._applicationOptions);

            foreach (var user in users)
            {
                user.Accounts = user.Accounts ?? new ExternalAccount[0];

                foreach (var account in user.Accounts)
                {
                    account.UserAccountId = user.Id;
                }

                user.Claims = user.Claims ?? new UserAccountClaim[0];

                foreach (var claim in user.Claims)
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