using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FotoShoutApi.Controllers
{
    public class ImageResult: ActionResult
    {
        public string ImageFilename { get; private set; }

        public ImageResult(string imageFilename)
        {
            ImageFilename = imageFilename;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (string.IsNullOrEmpty(ImageFilename))
                return;
            
            using (var image = Image.FromFile(ImageFilename))
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Png);
                var response = context.RequestContext.HttpContext.Response;
                response.ContentType = "image/png";
                response.BinaryWrite(memoryStream.GetBuffer());
            }
        }
    }
}