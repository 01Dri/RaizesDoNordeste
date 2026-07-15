using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace RaizesDoNordeste.API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class UninterPaymentAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private const string HeaderName = "X-UninterPayment-Key";
        private const string ExpectedToken = "UninterSecretWebhookToken123!";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var headerValue) || 
                headerValue != ExpectedToken)
            {
                context.Result = new UnauthorizedObjectResult("Acesso não autorizado.");
            }
        }
    }
}
