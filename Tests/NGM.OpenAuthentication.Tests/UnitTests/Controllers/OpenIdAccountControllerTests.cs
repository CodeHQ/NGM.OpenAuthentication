﻿//using System;
//using System.Security.Principal;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Routing;
//using DotNetOpenAuth.OpenId;
//using DotNetOpenAuth.OpenId.RelyingParty;
//using Moq;
//using NGM.OpenAuthentication.Core;
//using NGM.OpenAuthentication.Controllers;
//using NGM.OpenAuthentication.Core.OpenId;
//using NGM.OpenAuthentication.Services;
//using NUnit.Framework;
//using Orchard;
//using Orchard.Localization;
//using Orchard.Security;

//namespace NGM.OpenAuthentication.Tests.UnitTests.Controllers {
//    [TestFixture]
//    public class OpenIdAccountControllerTests {
//        private const string OpenAuthUrlForGoogle = "https://www.google.com/accounts/o8/id";

//        [Test]
//        public void should_return_logon_view_when_no_response_returned_from_relyparty() {
//            var mockRelyingService = new Mock<IOpenIdRelyingPartyService>();
//            mockRelyingService.Setup(ctx => ctx.HasResponse).Returns(false);

//            var accountController = new AccountController(mockRelyingService.Object, null, null);
//            accountController.ControllerContext = MockControllerContext(accountController);
//            var redirectResult = (RedirectToRouteResult)accountController.LogOn(string.Empty);
//            Assert.That(redirectResult.RouteValues["Action"], Is.EqualTo("LogOn"));
            
//            mockRelyingService.VerifyAll();
//        }

//        [Test]
//        public void should_return_error_message_when_authentication_was_canceled() {
//            var mockRelyingService = new Mock<IOpenIdRelyingPartyService>();
//            mockRelyingService.Setup(ctx => ctx.HasResponse).Returns(true);

//            var mockAuthenticationResponse = new Mock<IAuthenticationResponse>();
//            mockAuthenticationResponse.Setup(ctx => ctx.Status).Returns(AuthenticationStatus.Canceled);

//            mockRelyingService.Setup(ctx => ctx.Response).Returns(mockAuthenticationResponse.Object);

//            var accountController = new AccountController(mockRelyingService.Object, null, null);
//            var redirectResult = (RedirectToRouteResult)accountController.LogOn(string.Empty);

//            Assert.That(accountController.TempData.ContainsKey("error-InvalidProvider"), Is.True);

//            mockRelyingService.VerifyAll();
//            mockAuthenticationResponse.VerifyAll();
//        }

//        [Test]
//        public void should_return_error_message_when_authentication_failed() {
//            var mockRelyingService = new Mock<IOpenIdRelyingPartyService>();
//            mockRelyingService.Setup(ctx => ctx.HasResponse).Returns(true);

//            var mockAuthenticationResponse = new Mock<IAuthenticationResponse>();
//            mockAuthenticationResponse.Setup(ctx => ctx.Status).Returns(AuthenticationStatus.Failed);
//            var exception = new Exception("Error Message");
//            mockAuthenticationResponse.Setup(ctx => ctx.Exception).Returns(exception);

//            mockRelyingService.Setup(ctx => ctx.Response).Returns(mockAuthenticationResponse.Object);

//            var accountController = new AccountController(mockRelyingService.Object, null, null);
//            var redirectResult = (RedirectToRouteResult)accountController.LogOn(string.Empty);

//            Assert.That(accountController.TempData.ContainsKey("error-UnknownError"), Is.True);

//            object value;
//            accountController.TempData.TryGetValue("error-UnknownError", out value);
//            Assert.That((value as LocalizedString).Text, Is.EqualTo(exception.Message));

//            mockRelyingService.VerifyAll();
//            mockAuthenticationResponse.VerifyAll();
//        }

//        [Test]
//        public void should_authentiate_identifier_to_logged_in_account() {

//            var mockRelyingService = new Mock<IOpenIdRelyingPartyService>();
//            mockRelyingService.Setup(ctx => ctx.HasResponse).Returns(true);

//            var mockAuthenticationResponse = new Mock<IAuthenticationResponse>();
//            mockAuthenticationResponse.Setup(ctx => ctx.Status).Returns(AuthenticationStatus.Authenticated);
//            Identifier identifier = Identifier.Parse("http://foo.google.com");
//            Identifier friendlyIdentifier = Identifier.Parse("http://foo.google.com/Blah");

//            mockAuthenticationResponse.Setup(ctx => ctx.ClaimedIdentifier).Returns(identifier);
//            mockAuthenticationResponse.Setup(ctx => ctx.FriendlyIdentifierForDisplay).Returns(friendlyIdentifier);

//            mockRelyingService.Setup(ctx => ctx.Response).Returns(mockAuthenticationResponse.Object);

//            var mockOpenAuthorizer = new Mock<IOpenAuthorizer>();
//            mockOpenAuthorizer.Setup(o => o.Authorize(It.IsAny<OpenAuthenticationParameters>())).Returns(OpenAuthenticationStatus.Authenticated);

//            var accountController = new AccountController(mockRelyingService.Object, null, mockOpenAuthorizer.Object);
//            var actionResult = accountController.LogOn(string.Empty);

//            mockAuthenticationResponse.VerifyAll();
//            mockRelyingService.VerifyAll();
//        }

//        [Test]
//        public void should_redirect_to_register_route_if_user_does_not_exist() {
//            var mockRelyingService = new Mock<IOpenIdRelyingPartyService>();
//            mockRelyingService.Setup(ctx => ctx.HasResponse).Returns(true);

//            var mockOrchardServices = new Mock<IOrchardServices>();
//            var mockWorkContext = new Mock<WorkContext>();
//            mockOrchardServices.Setup(o => o.WorkContext).Returns(mockWorkContext.Object);

//            var mockAuthenticationResponse = new Mock<IAuthenticationResponse>();
//            mockAuthenticationResponse.Setup(ctx => ctx.Status).Returns(AuthenticationStatus.Authenticated);
//            Identifier identifier = Identifier.Parse("http://foo.google.com");
//            Identifier friendlyIdentifier = Identifier.Parse("http://foo.google.com/Blah");

//            mockAuthenticationResponse.Setup(ctx => ctx.ClaimedIdentifier).Returns(identifier);
//            mockAuthenticationResponse.Setup(ctx => ctx.FriendlyIdentifierForDisplay).Returns(friendlyIdentifier);

//            mockRelyingService.Setup(ctx => ctx.Response).Returns(mockAuthenticationResponse.Object);

//            var mockAuthenticationService = new Mock<IAuthenticationService>();

//            var mockOpenAuthenticationService = new Mock<IOpenAuthenticationService>();

//            var mockOpenAuthorizer = new Mock<IOpenAuthorizer>();
//            mockOpenAuthorizer.Setup(o => o.Authorize(It.IsAny<OpenAuthenticationParameters>())).Returns(OpenAuthenticationStatus.RequiresRegistration);

//            var accountController = new AccountController(mockRelyingService.Object, mockOpenAuthenticationService.Object, mockOpenAuthorizer.Object);
//            var redirectToRouteResult = (RedirectToRouteResult)accountController.LogOn(string.Empty);

//            Assert.That(redirectToRouteResult.RouteValues["area"], Is.EqualTo("Orchard.Users"));
//            Assert.That(redirectToRouteResult.RouteValues["action"], Is.EqualTo("Register"));
//            Assert.That(redirectToRouteResult.RouteValues["controller"], Is.EqualTo("Account"));

//            Assert.That(accountController.TempData.ContainsKey("RegisterModel"), Is.True);

//            mockAuthenticationService.Verify(ctx => ctx.SignIn(It.IsAny<IUser>(), It.IsAny<bool>()), Times.Never());
//            mockOpenAuthenticationService.Verify(ctx => ctx.AssociateExternalAccountWithUser(It.IsAny<IUser>(), It.IsAny<OpenAuthenticationParameters>()), Times.Never());

//            mockAuthenticationResponse.VerifyAll();
//            mockAuthenticationService.VerifyAll();
//            mockRelyingService.VerifyAll();
//            mockOpenAuthenticationService.VerifyAll();
//        }

//        //[Test]
//        //public void should_redirect_to_logon_view_if_no_viewmodel_present_on_register_page() {
//        //    var accountController = new AccountController(null, null, null, null);
//        //    var redirectToRouteResult = (RedirectToRouteResult)accountController.Register(null);

//        //    Assert.That(redirectToRouteResult.RouteValues["area"], Is.EqualTo("Orchard.Users"));
//        //    Assert.That(redirectToRouteResult.RouteValues["action"], Is.EqualTo("LogOn"));
//        //    Assert.That(redirectToRouteResult.RouteValues["controller"], Is.EqualTo("Account"));
//        //}

//        //[Test]
//        //public void should_redirect_to_logon_view_if_viewmodel_has_null_model_present_on_register_page() {
//        //    var accountController = new AccountController(null, null, null, null);

//        //    var viewModel = new RegisterViewModel();
//        //    var redirectToRouteResult = (RedirectToRouteResult)accountController.Register(viewModel);

//        //    Assert.That(redirectToRouteResult.RouteValues["area"], Is.EqualTo("Orchard.Users"));
//        //    Assert.That(redirectToRouteResult.RouteValues["action"], Is.EqualTo("LogOn"));
//        //    Assert.That(redirectToRouteResult.RouteValues["controller"], Is.EqualTo("Account"));
//        //}

//        //[Test]
//        //public void should_use_passedin_model_from_logon_if_avalible() {
//        //    var accountController = new AccountController(null, null, null, null);
//        //    var model = new RegisterModel { ExternalIdentifier = "Test" };
//        //    accountController.TempData.Add("RegisterModel", model);

//        //    var viewResult = (ViewResult)accountController.Register(null);
//        //    Assert.That(viewResult.ViewName, Is.EqualTo("Register"));
//        //    Assert.That(viewResult.ViewData.Model, Is.TypeOf(typeof(RegisterViewModel)));
//        //    var viewModel = viewResult.ViewData.Model as RegisterViewModel;
//        //    Assert.That(viewModel.Model, Is.EqualTo(model));
//        //}

//        //[Test]
//        //public void should_not_recreate_registration_view_model_if_view_model_exists() {
//        //    var accountController = new AccountController(null, null, null, null);
//        //    var viewModel = new RegisterViewModel { Model = new RegisterModel { ExternalIdentifier = "test" } };
//        //    var viewResult = (ViewResult)accountController.Register(viewModel);

//        //    Assert.That(viewResult.ViewData.Model, Is.EqualTo(viewModel));
//        //}

//        //[Test]
//        //public void should_show_all_identifiers_associated_with_loggedon_user() {
//        //    var mockUser = new Mock<IUser>();

//        //    var mockAuthenticationService = new Mock<IAuthenticationService>();
//        //    mockAuthenticationService.Setup(o => o.GetAuthenticatedUser()).Returns(mockUser.Object);

//        //    var mockContentQuery = new Mock<IContentQuery<OpenAuthenticationPart, OpenAuthenticationPartRecord>>();

//        //    var openAuthenticationPartRecord1 = new OpenAuthenticationPartRecord { ExternalIdentifier = "foo" };
//        //    var openAuthenticationPartRecord2 = new OpenAuthenticationPartRecord { ExternalIdentifier = "bar" };
//        //    var openAuthenticationPart1 = new OpenAuthenticationPart { Record = openAuthenticationPartRecord1 };
//        //    var openAuthenticationPart2 = new OpenAuthenticationPart { Record = openAuthenticationPartRecord2 };

//        //    mockContentQuery.Setup(o => o.List()).Returns(new[] { openAuthenticationPart1, openAuthenticationPart2 });

//        //    var mockOpenAuthenticationService = new Mock<IOpenAuthenticationService>();
//        //    mockOpenAuthenticationService.Setup(o => o.GetExternalIdentifiersFor(mockUser.Object)).Returns(mockContentQuery.Object);

//        //    var accountController = new AccountController(null, mockAuthenticationService.Object, mockOpenAuthenticationService.Object, null);
//        //    var viewResult = (ViewResult)accountController.VerifiedAccounts();

//        //    var viewModel = viewResult.ViewData.Model as VerifiedAccountsViewModel;
//        //    Assert.That(viewModel.Accounts.Count(), Is.EqualTo(2));
//        //}

//        //[Test]
//        //public void should_remove_identifier_from_loggedon_account_and_return_verifiedaccounts_view_on_success() {
//        //    var mockUser = new Mock<IUser>();

//        //    var mockAuthenticationService = new Mock<IAuthenticationService>();
//        //    mockAuthenticationService.Setup(o => o.GetAuthenticatedUser()).Returns(mockUser.Object);

//        //    var mockOpenAuthenticationService = new Mock<IOpenAuthenticationService>();
//        //    mockOpenAuthenticationService.Setup(o => o.RemoveIdentifier(mockUser.Object)).Returns(new[] { "foo", "bar" });

//        //    var accountController = new AccountController(null, mockAuthenticationService.Object, mockOpenAuthenticationService.Object);
//        //    var viewResult = (ViewResult)accountController.VerifiedAccounts();

//        //    var viewModel = viewResult.ViewData.Model as VerifiedAccountsViewModel;
//        //    Assert.That(viewModel.Accounts.Count(), Is.EqualTo(2));
//        //}



//        /* POST */

//        //[Test]
//        //public void should_remove_all_assosiated_openididentifiers_checked() {
//        //    string testUrl1 = "Foo";
//        //    string testUrl2 = "Bar";
            
//        //    var mockOpenAuthenticationService = new Mock<IOpenAuthenticationService>();
//        //    mockOpenAuthenticationService.Setup(o => o.RemoveAssociation(testUrl1));

//        //    var accountController = new AccountController(null, null, mockOpenAuthenticationService.Object, null);
//        //    accountController.ControllerContext = MockControllerContext(accountController);

//        //    var nameValueCollection = new NameValueCollection();
//        //    nameValueCollection.Add("Accounts[0].Account.ExternalIdentifier", testUrl1);
//        //    nameValueCollection.Add("Accounts[0].IsChecked", true.ToString().ToLowerInvariant());
//        //    nameValueCollection.Add("Accounts[1].Account.ExternalIdentifier", testUrl2);
//        //    nameValueCollection.Add("Accounts[1].IsChecked", false.ToString().ToLowerInvariant());

//        //    var verifiedAccounts = (RedirectToRouteResult)accountController._VerifiedAccounts(new FormCollection(nameValueCollection));

//        //    mockOpenAuthenticationService.Verify(o => o.RemoveAssociation(testUrl1), Times.Once());
//        //    mockOpenAuthenticationService.VerifyAll();
//        //}

//        public ControllerContext MockControllerContext(ControllerBase controllerBase) {
//            var mockHttpContext = new Mock<HttpContextBase>();
//            var mockIPrinciple = new Mock<IPrincipal>();
//            mockIPrinciple.SetupAllProperties();
//            var mockIIdentity = new Mock<IIdentity>();
//            mockIIdentity.SetupAllProperties();
//            mockIPrinciple.Setup(ctx => ctx.Identity).Returns(mockIIdentity.Object);
//            mockHttpContext.Setup(ctx => ctx.User).Returns(mockIPrinciple.Object);
//            return new ControllerContext(
//                mockHttpContext.Object,
//                new RouteData(
//                    new Route("foobar", new MvcRouteHandler()),
//                    new MvcRouteHandler()),
//                controllerBase);
//        }
//    }
//}
