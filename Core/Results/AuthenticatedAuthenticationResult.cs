namespace NGM.OpenAuthentication.Core.Results {
    public class AuthenticatedAuthenticationResult : AuthenticationResult {
        public AuthenticatedAuthenticationResult() : base (Statuses.Authenticated) {}
    }
}