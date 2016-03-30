using static System.Console;
using Microsoft.Owin.Hosting;
using System.Threading;

namespace FlashMyPi.Service
{
    public class Program
    {
        public const string Url = "http://+:8080";

        public static void Main()
        {
            WriteLine("Starting FlashMyPi service.");

            var cancellationTokenSource = new CancellationTokenSource();

            using(var socketServer = new SocketServer())
            {
                MessageBus.Start(cancellationTokenSource.Token);
                socketServer.Start();
                WriteLine("Service started. Hit return to quit");

                using(var webApp = WebApp.Start<Startup>(Url))
                {
                    WriteLine($"API started on {Url}");
                    ReadLine();
                    cancellationTokenSource.Cancel();
                }
            }
            WriteLine("Service has stopped");
        }
    }
}
