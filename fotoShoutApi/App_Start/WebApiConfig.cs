using FotoShoutApi.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace FotoShoutApi {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            //config.Routes.MapHttpRoute(
            //    name: "PhotoGuestsApi",
            //    routeTemplate: "fs1/PhotoGuests/{id}/{eventOptionId}",
            //    defaults: new { controller = "PhotoGuests" });

            //config.Routes.MapHttpRoute(
            //    name: "GuestsApi",
            //    routeTemplate: "fs1/Guests/{action}/{email}",
            //    defaults: new { controller = "Guests", action = RouteParameter.Optional }
            //);

            config.Routes.MapHttpRoute(
                name: "ActionApi1",
                routeTemplate: "fs1/{controller}/{action}/{id}/{status}"
            );

            config.Routes.MapHttpRoute(
                name: "ActionApi2",
                routeTemplate: "fs1/{controller}/{action}/{id}/{filename}",
                defaults: new { filename = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "fs1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configure the media-type formatters
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Register the JpegFormatter
            config.Formatters.Add(new ImageFormatter());
        }
    }
}
