using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System.IO;
using System.Threading.Tasks;

namespace FlashMyPi.Service
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Run(WebApplication.HandleRequest);
        }
    }

    public static class WebApplication
    {
        public async static Task HandleRequest(IOwinContext context)
        {
            if(context.Request.Path.ToString() == "/" && context.Request.Method == "GET")
            {
                await context.Response.WriteAsync("Welcome to the FlashMyPi API.");
                return;
            }

            if (context.Request.Path.ToString() == "/api/pattern" && context.Request.Method == "POST")
            {
                await HandlePatternPost(context);
                return;
            }

            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("This is not the path you are looking for");
        }

        public async static Task HandlePatternPost(IOwinContext context)
        {
            Pattern pattern = null;
            using (var reader = new StreamReader(context.Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                pattern = JsonConvert.DeserializeObject<Pattern>(body);
            }

            MessageBus.Publish(pattern);

            var responseBody = JsonConvert.SerializeObject(pattern);
            await context.Response.WriteAsync(responseBody);
        }
    }

    public class Pattern
    {
        [JsonProperty("pixels")]
        public int[] Pixels { get; set; }
    }
}
