using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.FileSystemPicker.Controllers
{
    [PluginController("FileSystemPicker")]
    public class FileSystemThumbnailApiController : UmbracoAuthorizedApiController
    {
        public HttpResponseMessage GetThumbnail(string imagePath, int width)
        {
            if (false == string.IsNullOrWhiteSpace(imagePath) && imagePath.IndexOf("{{") < 0)
            {
                var image =  Image.FromFile(System.Web.Hosting.HostingEnvironment.MapPath(imagePath));

                MemoryStream outStream = new MemoryStream();

                byte[] photoBytes = File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(imagePath)); // change imagePath with a valid image path
                ISupportedImageFormat format = new JpegFormat { Quality = 70 }; // convert to jpg
                
                var inStream = new MemoryStream(photoBytes);

                var imageFactory = new ImageFactory(preserveExifData: true);

                Size size = ResizeKeepAspect(image.Size, width, width);

                ResizeLayer resizeLayer = new ResizeLayer(size, ResizeMode.Max);
                imageFactory.Load(inStream)
                        .Resize(resizeLayer)
                        .Format(format)
                        .Save(outStream);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(outStream);
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return response;
            }
            else
            {
                return null;
            }
        }

        public static Size ResizeKeepAspect(Size CurrentDimensions, int maxWidth, int maxHeight)
        {
            int newHeight = CurrentDimensions.Height;
            int newWidth = CurrentDimensions.Width;
            if (maxWidth > 0 && newWidth > maxWidth) //WidthResize
            {
                Decimal divider = Math.Abs((Decimal)newWidth / (Decimal)maxWidth);
                newWidth = maxWidth;
                newHeight = (int)Math.Round((Decimal)(newHeight / divider));
            }
            if (maxHeight > 0 && newHeight > maxHeight) //HeightResize
            {
                Decimal divider = Math.Abs((Decimal)newHeight / (Decimal)maxHeight);
                newHeight = maxHeight;
                newWidth = (int)Math.Round((Decimal)(newWidth / divider));
            }
            return new Size(newWidth, newHeight);
        }
    }
}