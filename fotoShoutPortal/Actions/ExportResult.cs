using FotoShoutUtils.Utils.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutUtils.Actions {
    public class ExportResult<Model>: ActionResult {
        private string _Filename { get; set; }
        private string _ViewPath { get; set; }
        private Model _Model { get; set; }

        public ExportResult(string viewPath, string filename, Model model) {
            _ViewPath = viewPath;
            _Filename = filename;
            _Model = model;
        }

        public override void ExecuteResult(ControllerContext context) {
            string content = this.RenderViewToString(context);
            this.WriteFile(content);
        }
        
        protected string RenderViewToString(ControllerContext context) {
            using (var writer = new StringWriter()) {
                var view = ViewEngines.Engines.FindPartialView(context, _ViewPath).View;
                var vdd = new ViewDataDictionary<Model>(_Model);
                var viewCxt = new ViewContext(context, view, vdd, new TempDataDictionary(), writer);
                viewCxt.View.Render(viewCxt, writer);
                return writer.ToString();
            }
        }

        private void WriteFile(string content) {
            HttpContext context = HttpContext.Current;
            context.Response.Clear();
            context.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + _Filename + "\"");
            context.Response.Charset = "";
            context.Response.ContentType = FileUtils.GetContentType(_Filename);
            context.Response.Write(content);
            context.Response.End();
        }
    }
}
