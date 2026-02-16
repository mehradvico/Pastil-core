using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Helpers
{
    public static class HttpRequestParamReaderHelper
    {
        public static string Get(HttpRequest request, string key)
        {
            if (request.HasFormContentType && request.Form.ContainsKey(key))
                return request.Form[key];

            if (request.Query.ContainsKey(key))
                return request.Query[key];

            return null;
        }
    }
}
