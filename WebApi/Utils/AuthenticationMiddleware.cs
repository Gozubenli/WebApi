using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Utils
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                var arr = usernamePassword.Split(":");

                int seperatorIndex = usernamePassword.IndexOf(':');

                var apiName = arr[0]; // usernamePassword.Substring(0, seperatorIndex);
                var apikey = arr[1]; // usernamePassword.Substring(seperatorIndex + 1);

                //if (arr.Length == 3)
                //{
                //    context.Session.SetString("UserName", arr[2]);
                //}
                //else
                //{
                //    context.Session.SetString("UserName", "");
                //}

                if (Singleton.Instance.ApiKey.Keys.Contains(apiName) && Singleton.Instance.ApiKey[apiName] == apikey)
                {
                    await _next.Invoke(context);
                }
                else
                {
                    context.Response.StatusCode = 401; //Unauthorized
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 401; //Unauthorized
                return;
            }
        }        

        //private async Task SignIn(string username, HttpContext context)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, username)
        //    };
        //    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        //    claims.Add(new Claim(ClaimTypes.Role, "Moderator"));

        //    var identity = new ClaimsIdentity(
        //        claims,
        //        CookieAuthenticationDefaults.AuthenticationScheme,
        //        ClaimTypes.Name,
        //        ClaimTypes.Role
        //    );

        //    await context.SignInAsync(
        //        CookieAuthenticationDefaults.AuthenticationScheme,
        //        new ClaimsPrincipal(identity),
        //        new AuthenticationProperties
        //        {
        //            IsPersistent = true,
        //            ExpiresUtc = DateTime.UtcNow.AddMonths(1)
        //        }
        //    );
        //}
    }
}
