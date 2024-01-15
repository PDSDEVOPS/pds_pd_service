using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;

namespace ProductionWebService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //config.EnableCors();
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { id = RouteParameter.Optional }
            );

            //config.AddApiVersioning(options =>
            //{
            //    options.ReportApiVersions = true;
            //    options.AssumeDefaultVersionWhenUnspecified = true;
            //    options.ApiVersionSelector = new DefaultApiVersionSelector(options);
            //});

            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            Console.Out.WriteLine("");

            //config.MessageHandlers.Add(new CompressionHandler());
        }
    }
}
