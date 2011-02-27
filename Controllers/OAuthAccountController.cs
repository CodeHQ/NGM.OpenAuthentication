﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using NGM.OpenAuthentication.Core;
using NGM.OpenAuthentication.Core.OAuth;
using NGM.OpenAuthentication.Extensions;
using NGM.OpenAuthentication.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace NGM.OpenAuthentication.Controllers {
    [Themed]
    public class OAuthAccountController : Controller {
        private readonly IEnumerable<IOAuthProviderAuthorizer> _oAuthWrappers;
        private readonly IOrchardServices _orchardServices;

        public OAuthAccountController(IEnumerable<IOAuthProviderAuthorizer> oAuthWrappers, IOrchardServices orchardServices) {
            _oAuthWrappers = oAuthWrappers;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult LogOn(string returnUrl, string knownProvider) {
            var viewModel = new CreateViewModel();
            TryUpdateModel(viewModel);

            var wrapper = _oAuthWrappers
                .Where(o => o.Provider.ToString().Equals(GetKnownProvider(viewModel, knownProvider), StringComparison.InvariantCultureIgnoreCase) 
                    && o.IsConsumerConfigured).FirstOrDefault();

            if (wrapper != null) {
                var result = wrapper.Authorize(returnUrl);

                if (result.AuthenticationStatus == OpenAuthenticationStatus.ErrorAuthenticating) {
                    _orchardServices.Notifier.Error(T(result.Error.Value));
                }
                
                if (result.AuthenticationStatus == OpenAuthenticationStatus.RequiresRegistration) {
                    TempData["registermodel"] = result.RegisterModel;
                    return new RedirectResult(Url.Register(returnUrl, result.RegisterModel));
                }
                
                if (result.AuthenticationStatus == OpenAuthenticationStatus.Authenticated) {
                    _orchardServices.Notifier.Information(T("Account succesfully associated to logged in account"));
                    return new RedirectResult(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/");
                }

                if (result.Result != null) return result.Result;
            }

            return HttpContext.Request.IsAuthenticated ? new RedirectResult(returnUrl) : new RedirectResult(Url.LogOn(returnUrl));
        }

        public string GetKnownProvider(CreateViewModel viewModel, string tempKnownProvider) {
            var provider = string.Empty;
            if (string.IsNullOrEmpty(viewModel.KnownProvider)) {
                if (_orchardServices.WorkContext.HttpContext.Session["knownProvider"] != null) {
                    provider = _orchardServices.WorkContext.HttpContext.Session["knownProvider"] as string;
                }
                else if (!string.IsNullOrEmpty(tempKnownProvider)) {
                    provider = tempKnownProvider;
                }
            }
            return provider;
        }
    }
}