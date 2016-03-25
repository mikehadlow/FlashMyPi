using static System.Console;

namespace FlashMyPi.Service
{
    public class Program
    {
        public static void Main()
        {
            WriteLine("Starting FlashMyPi service.");

            var socketServer = new SocketServer();
            socketServer.Start();

            WriteLine("Service started. Hit return to quit");
            ReadLine();
            socketServer.Stop();
        }
    }
}
