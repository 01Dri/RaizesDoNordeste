using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using RaizesDoNordeste.API.Attributes;

namespace RaizesDoNordeste.Test.Attributes
{
    [TestFixture]
    public class UninterPaymentAuthorizeAttributeTest
    {
        private UninterPaymentAuthorizeAttribute _attribute;

        [SetUp]
        public void Setup()
        {
            _attribute = new UninterPaymentAuthorizeAttribute();
        }

        [Test]
        public void OnAuthorization_ShouldDoNothing_WhenHeaderTokenIsValid()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-UninterPayment-Key"] = "UninterSecretWebhookToken123!";

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var filterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            // Act
            _attribute.OnAuthorization(filterContext);

            // Assert
            Assert.That(filterContext.Result, Is.Null);
        }

        [Test]
        public void OnAuthorization_ShouldSetUnauthorizedResult_WhenHeaderTokenIsMissing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var filterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            // Act
            _attribute.OnAuthorization(filterContext);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(filterContext.Result, Is.Not.Null);
                Assert.That(filterContext.Result, Is.TypeOf<UnauthorizedObjectResult>());
            });
        }

        [Test]
        public void OnAuthorization_ShouldSetUnauthorizedResult_WhenHeaderTokenIsInvalid()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-UninterPayment-Key"] = "invalid-token";

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var filterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            // Act
            _attribute.OnAuthorization(filterContext);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(filterContext.Result, Is.Not.Null);
                Assert.That(filterContext.Result, Is.TypeOf<UnauthorizedObjectResult>());
            });
        }
    }
}
