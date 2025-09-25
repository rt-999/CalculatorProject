using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace IO.Swagger.Security
{
    /// <summary>
    /// class to handle bearer authentication.
    /// </summary>
    public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        /// <summary>
        /// scheme name for authentication handler.
        /// </summary>
        public const string SchemeName = "Bearer";

        private readonly byte[] key = Encoding.ASCII.GetBytes("a-string-secret-at-least-256-bits-long");

        public BearerAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        /// <summary>
        /// verify that require authorization header exists.
        /// </summary>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var token = authHeader.Parameter; 

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"Invalid Authorization Header: {ex.Message}");
            }
        }
    }
}
