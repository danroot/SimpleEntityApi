using System.Web.Http;

namespace SimpleEntityApi
{
    public static class SimpleEntityApiRoutes
    {
        public static void RegisterRoutes(HttpConfiguration config, bool useAreas)
        {
            if (useAreas)
            {
                config.Routes.MapHttpRoute(
                    name: "SimpleEntityApi_New",
                    routeTemplate: "api/{area}/{controller}/new",
                    defaults: new { action = "New", id = RouteParameter.Optional }
                    );
                config.Routes.MapHttpRoute(
                  name: "SimpleEntityApi_Meta",
                  routeTemplate: "api/{area}/{controller}/meta",
                  defaults: new { action = "Metadata", id = RouteParameter.Optional }
                  );
                config.Routes.MapHttpRoute(
                  name: "SimpleEntityApi_multiples",
                  routeTemplate: "api/{area}/{controller}/many",
                  defaults: new { action = "MultipleEndpoint", id = RouteParameter.Optional }
                  );
                config.Routes.MapHttpRoute(
                    name: "SimpleEntityApi",
                    routeTemplate: "api/{area}/{controller}/{id}",
                    defaults: new { action = "Endpoint", id = RouteParameter.Optional }
                    );
            }
            else
            {
                config.Routes.MapHttpRoute(
                    name: "SimpleEntityApi_New",
                    routeTemplate: "api/{controller}/new",
                    defaults: new { action = "New", id = RouteParameter.Optional }
                    );
                config.Routes.MapHttpRoute(
                   name: "SimpleEntityApi_Meta",
                   routeTemplate: "api/{controller}/meta",
                   defaults: new { action = "Metadata", id = RouteParameter.Optional }
                   );
                config.Routes.MapHttpRoute(
                  name: "SimpleEntityApi_Help",
                  routeTemplate: "api/{controller}/help",
                  defaults: new { action = "Help", id = RouteParameter.Optional }
                  );
                config.Routes.MapHttpRoute(
                   name: "SimpleEntityApi_Multiples",
                   routeTemplate: "api/{controller}/many",
                   defaults: new { action = "MultipleEndpoint", id = RouteParameter.Optional }
                   );
                config.Routes.MapHttpRoute(
                 name: "SimpleEntityApi_Single",
                 routeTemplate: "api/{controller}/{id}",
                 defaults: new { action = "Single" }
                 );
                config.Routes.MapHttpRoute(
                    name: "SimpleEntityApi",
                    routeTemplate: "api/{controller}",
                    defaults: new { action = "Endpoint" }
                    );
            }
        }

    }
}