﻿using System.Collections.Generic;
using System.Web.Mvc;
using NGM.OpenAuthentication.Core.Results;

namespace NGM.OpenAuthentication.Core {
    public class AuthenticationState {
        public AuthenticationState(string returnUrl, Statuses statuses) {
            Status = statuses;

            if (Status == Statuses.Authenticated)
                Result = new RedirectResult(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/");
        }

        public AuthenticationState(string returnUrl, AuthenticationResult authenticationResult) : this (returnUrl, authenticationResult.Status) {
            Error = authenticationResult.Error;
        }

        public Statuses Status { get; private set; }

        public KeyValuePair<string, string> Error { get; set; }

        public ActionResult Result { get; set; }
    }
}