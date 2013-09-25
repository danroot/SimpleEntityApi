using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SimpleEntityApi.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            SimpleEntityApiRoutes.RegisterRoutes(config, false);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
