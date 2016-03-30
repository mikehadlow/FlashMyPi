using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FlashMyPi.Service
{
    public static class MessageBus
    {
        private static BlockingCollection<Pattern> queue = new BlockingCollection<Pattern>();
        private static ConcurrentBag<Action<Pattern>> handlers = new ConcurrentBag<Action<Pattern>>();
        private static Thread messagePump;

        public static void Publish(Pattern pattern)
        {
            queue.Add(pattern);
        }

        public static void Subscribe(Action<Pattern> handler)
        {
            handlers.Add(handler);
        }

        public static void Start(CancellationToken cancellationToken)
        {
            messagePump = new Thread(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var pattern = queue.Take(cancellationToken);

                    foreach (var handler in handlers)
                    {
                        handler(pattern);
                    }
                }
            });

            messagePump.Start();
        }
    }
}
