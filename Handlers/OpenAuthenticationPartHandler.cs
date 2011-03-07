using System.Linq;
using JetBrains.Annotations;
using NGM.OpenAuthentication.Core;
using NGM.OpenAuthentication.Core.OAuth;
using NGM.OpenAuthentication.Models;
using NGM.OpenAuthentication.Services;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Security;
using Orchard.UI.Notify;

namespace NGM.OpenAuthentication.Handlers {
    [UsedImplicitly]
    public class OpenAuthenticationPartHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IOpenAuthenticationService _openAuthenticationService;

        public OpenAuthenticationPartHandler(IRepository<OpenAuthenticationPartRecord> openAuthenticationPartRepository,
            IOrchardServices orchardServices,
            IOpenAuthenticationService openAuthenticationService) {
            _orchardServices = orchardServices;
            _openAuthenticationService = openAuthenticationService;
            Filters.Add(StorageFilter.For(openAuthenticationPartRepository));
            
            OnCreated<IUser>((context, user) => {
                                 if (HasQueryParamsLocator()) {
                                     TryAssociateAccount(user, GetQueryStringParameters());
                                 }

                                 if (HasOpenAutheticationSessionLocator()) {
                                     TryAssociateAccount(user, GetSessionParameters());
                                     // Clean up
                                     _orchardServices.WorkContext.HttpContext.Session.Remove("parameters");
                                 }
                                 // This may be unnessesary as we have the it duplicated in the OnLoaded event.
                                 // TODO: Check event stack
                                 if (HasLiveIdCookie())
                                    TryAssociateLiveIdParameters(user);
                             });
            
            OnLoaded<IUser>((context, user) => {
                                if (_orchardServices.WorkContext.CurrentUser != null) return;

                                if (HasLiveIdCookie())
                                    TryAssociateLiveIdParameters(user);
                            });

            OnRemoved<IUser>((context, user) => _openAuthenticationService.GetExternalIdentifiersFor(user)
                                                    .List()
                                                    .ToList()
                                                    .ForEach(o => _openAuthenticationService.RemoveAssociation(new HashedOpenAuthenticationParameters(o.Record.HashedProvider, o.Record.ExternalIdentifier))));
        }

        private OpenAuthenticationParameters GetQueryStringParameters() {
            var externalIdentifier = _orchardServices.WorkContext.HttpContext.Request.Params["externalidentifier"];
            var externalDisplayIdentifier = _orchardServices.WorkContext.HttpContext.Request.Params["externaldisplayidentifier"];
            var oAuthToken = _orchardServices.WorkContext.HttpContext.Request.Params["oauthtoken"];
            var oAuthAccessToken = _orchardServices.WorkContext.HttpContext.Request.Params["oauthaccesstoken"];
            var provider = int.Parse(_orchardServices.WorkContext.HttpContext.Request.Params["provider"]);

            return new HashedOpenAuthenticationParameters(provider) {
                ExternalIdentifier = externalIdentifier,
                ExternalDisplayIdentifier = externalDisplayIdentifier,
                OAuthToken = oAuthToken,
                OAuthAccessToken = oAuthAccessToken
            };
        }

        private bool HasQueryParamsLocator() {
            return !string.IsNullOrEmpty(_orchardServices.WorkContext.HttpContext.Request.Params["externalidentifier"] as string);
        }

        private bool HasOpenAutheticationSessionLocator() {
            if (_orchardServices.WorkContext.HttpContext.Session != null) {
                if (_orchardServices.WorkContext.HttpContext.Session["parameters"] != null) {
                    var parameters = GetSessionParameters();
                    return parameters != null;
                }
            }

            return false;
        }

        private OpenAuthenticationParameters GetSessionParameters() {
            return _orchardServices.WorkContext.HttpContext.Session["parameters"] as OpenAuthenticationParameters;           
        }

        private bool HasLiveIdCookie() {
            return _orchardServices.WorkContext.HttpContext.Request.Cookies[LiveIdProviderAuthorizer.LoginCookie] != null;
        }

        private void TryAssociateLiveIdParameters(IUser user) {
            var cookie = _orchardServices.WorkContext.HttpContext.Request.Cookies[LiveIdProviderAuthorizer.LoginCookie];
            if (cookie == null) return;

            var parameters = new HashedOpenAuthenticationParameters(OAuthProvider.LiveId.GetHashCode()) {
                ExternalIdentifier = cookie.Value,
                ExternalDisplayIdentifier = cookie.Values["UserId"],
                OAuthToken = cookie.Value
            };

            TryAssociateAccount(user, parameters);
        }

        private void TryAssociateAccount(IUser user, OpenAuthenticationParameters parameters) {
            if (parameters != null && !_openAuthenticationService.AccountExists(parameters)) {
                _openAuthenticationService.AssociateExternalAccountWithUser(user, parameters);
            }
        }
    }
}