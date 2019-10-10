using LoginDecryption;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using TFSApi;

namespace TFSApi.Token_Based_Authentication
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var decryptedUserName = Logindecryption.DecryptStringAES(context.UserName);
            var decryptedPassword = Logindecryption.DecryptStringAES(context.Password);
            bool user = BasicAuthentication.IsAuthorizedUser(decryptedUserName, decryptedPassword);
            if (user)
            {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim("UserName", decryptedUserName));
                identity.AddClaim(new Claim("LoggedOn",DateTime.Now.ToString()));
                context.Validated(identity);
            }
            else
            {
                context.SetError("invalid_grant", "Provided username and password is incorrect");
                return;
            }
        }
    }
}