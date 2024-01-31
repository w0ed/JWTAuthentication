using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.IdentityModel.Tokens;



public class TokenAuthorizeAttribute : AuthorizeAttribute
{
    public override void OnAuthorization(HttpActionContext actionContext)
    {
        // Retrieve the token from the request headers or query string
        string token = actionContext.Request.Headers.Authorization?.Parameter ?? actionContext.Request.GetQueryNameValuePairs().FirstOrDefault(q => q.Key.Equals("token", StringComparison.OrdinalIgnoreCase)).Value;



        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var jwtSettings = ConfigurationManager.AppSettings; // Get settings from your configuration file



                // Configure token validation parameters
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["JwtSecretKey"])),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["JwtIssuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["JwtAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                Thread.CurrentPrincipal = principal;



                return;
            }
            catch (SecurityTokenValidationException)
            {
                // Token validation failed
            }
        }



        // Token is missing or invalid; return unauthorized status
        HandleUnauthorizedRequest(actionContext);
    }
}