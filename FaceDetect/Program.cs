using Newtonsoft.Json.Linq;
using System.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Immutable;
using System.Diagnostics;

namespace FaceDetect
{
    class Program
    {
        private static string msg = "Please provide the API key as the first command-line argument followed by the filename of the image to be submitted to the proccessing";


        static void Main(string[] args)
        {
            var apiKey = !string.IsNullOrWhiteSpace(args[0]) ? args[0] : throw new ArgumentException(msg, args[0]);
            var filename = File.Exists(args[1]) ? args[1] : throw new FileNotFoundException(msg, args[1]);
            var region = "westcentralus";
            var target = new Uri($"https://{region}.api.cognitive.microsoft.com/face/v1.0/detect/?subscription-key={apiKey}");
            var httpPost = CreateHttpRequest(target, "POST", "application/octet-stream");
            using (var fs = File.OpenRead(filename))
            {
                fs.CopyTo(httpPost.GetRequestStream());
            }
            string data = GetResponse(httpPost);
            var rectangles = GetRectangle(data);
            var img = Image.Load(filename);

            var count = 0;
            foreach (var rectangle in GetRectangle(data))
            {
                img.Mutate(a => a.DrawPolygon(Color.Green, 5, rectangle));
                count++;

            }
            Console.WriteLine($"Number of faces: {count}");
            var otputFileName = $"{Environment.CurrentDirectory}\\{Path.GetFileNameWithoutExtension(filename)}-2{Path.GetExtension(filename)}";
            SaveImage(img, otputFileName);

            OpenWithDefaultApp(otputFileName);

        }

        private static void OpenWithDefaultApp(string filename)
        {
            var si = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = filename,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process.Start(si);
        }

        private static void SaveImage(Image img, string outputfilename)
        {
            using (var fs = File.Create(outputfilename))
            {
                img.SaveAsJpeg(fs);
            }
        }

        private static IEnumerable<PointF[]> GetRectangle(string data)
        {
            var faces = JArray.Parse(data);
            foreach (var face in faces)
            {
                var id = (string)face["faceId"];

                var top = (int)face["faceRectangle"]["top"];
                var left = (int)face["faceRectangle"]["left"];
                var width = (int)face["faceRectangle"]["width"];
                var height = (int)face["faceRectangle"]["height"];

                var rectangle = new PointF[]
                {
                    new PointF(left, top),
                    new PointF(left + width, top),
                    new PointF(left + width, top+ height),
                    new PointF(left, top+ height),
                };
                yield return rectangle;
            }
        }

        private static HttpWebRequest CreateHttpRequest(Uri target, string method, string contentType)
        {
            var request = WebRequest.CreateHttp(target);
            request.Method = method;
            request.ContentType = contentType;
            return request;
        }

        private static string GetResponse(HttpWebRequest httpPost)
        {
            using (var response = httpPost.GetResponse())
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

    }
}
















