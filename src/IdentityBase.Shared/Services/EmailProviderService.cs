// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Models;

    public interface IEmailProviderInfoService
    {
        Task<EmailProviderInfo> GetProviderInfo(string email);
        Task<bool> HasSameProviderInfo(string emaila, string emailb);
    }

    public class DefaultEmailProviderInfoService : IEmailProviderInfoService
    {
        private readonly Dictionary<string, EmailProviderInfo> _providerInfos;

        public DefaultEmailProviderInfoService()
        {
            this._providerInfos = new Dictionary<string, EmailProviderInfo>();

            this.AddProvider(
                "https://mail.google.com/mail/u/0/#inbox",
                "Google",
                "gmail.com", "googlemail.com"
            );

            this.AddProvider(
                "https://outlook.live.com/owa/?path=/mail/inbox",
                "Outlook",
                "hotmail.be", "hotmail.ca", "hotmail.cl", "hotmail.co.id",
                "hotmail.co.il", "hotmail.co.in", "hotmail.co.jp", "hotmail.co.kr",
                "hotmail.co.th", "hotmail.co.uk", "hotmail.co.za", "hotmail.com",
                "hotmail.com.ar", "hotmail.com.au", "hotmail.com.br", "hotmail.com.hk",
                "hotmail.com.tr", "hotmail.com.tw", "hotmail.com.vn", "hotmail.cz",
                "hotmail.de", "hotmail.dk", "hotmail.es", "hotmail.fi", "hotmail.fr",
                "hotmail.gr", "hotmail.hu", "hotmail.it", "hotmail.lt", "hotmail.lv",
                "hotmail.my", "hotmail.nl", "hotmail.no", "hotmail.ph", "hotmail.rs",
                "hotmail.se", "hotmail.sg", "hotmail.sk", "live.at", "live.be",
                "live.ca", "live.cl", "live.cn", "live.co.kr", "live.co.uk", "live.co.za",
                "live.com", "live.com.ar", "live.com.au", "live.com.mx", "live.com.my",
                "live.com.ph", "live.com.pt", "live.com.sg", "live.de", "live.dk",
                "live.fi", "live.fr", "live.hk", "live.ie", "live.in", "live.it", "live.jp",
                "live.nl", "live.no", "live.ru", "live.se", "livemail.tw", "outlook.at",
                "outlook.be", "outlook.cl", "outlook.co.id", "outlook.co.il", "outlook.co.th",
                "outlook.com", "outlook.com.ar", "outlook.com.au", "outlook.com.br",
                "outlook.com.gr", "outlook.com.tr", "outlook.com.vn", "outlook.cz",
                "outlook.de", "outlook.dk", "outlook.es", "outlook.fr", "outlook.hu",
                "outlook.ie", "outlook.in", "outlook.it", "outlook.jp", "outlook.kr",
                "outlook.lv", "outlook.my", "outlook.ph", "outlook.pt", "outlook.sa",
                "outlook.sg", "outlook.sk", "windowslive.com"
            );
        }

        public async Task<EmailProviderInfo> GetProviderInfo(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email)); 
            }

            string host = email
                .Trim()
                .Split('@')
                .LastOrDefault()
                .ToLowerInvariant();

            if (!this._providerInfos.ContainsKey(host))
            {
                return null; 
            }

            EmailProviderInfo result = this._providerInfos[host];

            return result;
        }

        private void AddProvider(
            string url,
            string provider,
            params string[] hosts)
        {
            var info = new EmailProviderInfo
            {
                Provider = provider,
                WebMailBaseUrl = url,
                Hosts = hosts
            };

            foreach (var host in hosts)
            {
                this._providerInfos.Add(host, info);
            }
        }

        public async Task<bool> HasSameProviderInfo(
            string emailA,
            string emailB)
        {
            EmailProviderInfo providerA = await this.GetProviderInfo(emailA);
            EmailProviderInfo providerB = await this.GetProviderInfo(emailA);

            if (providerA == null || providerB == null)
            {
                return false;
            }

            return providerA.Provider.Equals
            (
                 providerA.Provider,
                 StringComparison.InvariantCultureIgnoreCase
            );
        }
    }
}

