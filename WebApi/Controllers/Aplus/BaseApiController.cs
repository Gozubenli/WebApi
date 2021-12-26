using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Controllers.Aplus
{
    public class BaseApiController : ControllerBase
    {
        public string GetUserName()
        {
            return GetUserName(HttpContext.Request.Headers["Authorization"]);
        }

        private string GetUserName(string authHeader)
        {
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                var arr = usernamePassword.Split(":");
                if (arr != null && arr.Length > 2)
                {
                    return arr[2];
                }
            }
            return "";
        }
    }
}
