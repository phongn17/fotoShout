using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutUtils {
    static class Test {
        static void Main() {
            GetQualifiedAssemblyNameOfClass();
        }

        private static void GetQualifiedAssemblyNameOfClass() {
            Type type = typeof(FotoShoutUtils.Sync.Db.SqlClientSync);
            Console.WriteLine("Qualified assembly name: {0}", type.AssemblyQualifiedName.ToString());
            Console.ReadLine();
        }

        private static void TestRazorTemplate() {
            //var config = new TemplateServiceConfiguration {
            //    Language = RazorEngine.Language.CSharp
            //};
            //var service = new TemplateService(config);
            //Razor.SetTemplateService(service);

            string template = @"<html>
                                    <head><title>@Model.Name</title></head>
                                    <body>
                                        Dear @Model.Name:<br/>
                                        <p>
                                        Thank you for joining the event. Please follow the <a href='@Model.Name'>link</a> to view your photo.
                                        </p>
                                    </body>
                                </html>";
            var result = Razor.Parse(template, new { Name = "Phong Nguyen" });
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
