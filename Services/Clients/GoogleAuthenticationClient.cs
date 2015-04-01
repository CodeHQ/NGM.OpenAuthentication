using System;
using System.IO;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using DotNetOpenAuth.GoogleOAuth2;
using NGM.OpenAuthentication.Models;

namespace NGM.OpenAuthentication.Services.Clients
{
    public class GoogleAuthenticationClient : IExternalAuthenticationClient
    {
        public string ProviderName
        {
            get { return "Google"; }
        }

        public IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord) {
            var client = new GoogleOAuth2Client(providerConfigurationRecord.ProviderIdKey, providerConfigurationRecord.ProviderSecret);

            return client;
        }
    }
}