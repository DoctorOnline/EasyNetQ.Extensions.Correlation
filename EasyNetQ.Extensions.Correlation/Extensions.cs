using System;
using System.Threading.Tasks;
using EasyNetQ.FluentConfiguration;
using Microsoft.Extensions.Logging;

namespace EasyNetQ.Extensions.Correlation
{
    /// <summary>
    /// Extensions for EasyNetQ for exchange with correlation data with messages.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Publish message to a message bus with a correlation identifier.
        /// </summary>
        public static Task PublishAsync<T>(this IBus messageBus, string correlationId, T body)
        {
            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ArgumentException($"{nameof(correlationId)} cannot be null or empty.");
            }

            var message = new CorrelationMessage<T>
            {
                CorrelationId = correlationId,
                Body = body
            };

            return messageBus.PublishAsync(message);
        }

        /// <summary>
        /// Send message to a message bus with a correlation identifier.
        /// </summary>
        public static Task SendAsync<T>(this IBus messageBus, string queue, string correlationId, T body)
        {
            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ArgumentException($"{nameof(correlationId)} cannot be null or empty.");
            }

            var message = new CorrelationMessage<T>
            {
                CorrelationId = correlationId,
                Body = body
            };

            return messageBus.SendAsync(queue, message);
        }

        /// <summary>
        /// Subscribe to getting messages from a message bus with a correlation identifier.
        /// </summary>
        public static ISubscriptionResult SubscribeAsync<T>(
            this IBus messageBus,
            string subscriptionId,
            Func<T, Task> onRecievedMessage,
            Action<string> onRecievedCorrelationId,
            Action<ISubscriptionConfiguration> configure,
            ILogger logger)
        {
            return messageBus.SubscribeAsync<CorrelationMessage<T>>(
                subscriptionId,
                message => ReceiveMessageAsync(message, onRecievedMessage, onRecievedCorrelationId, logger),
                configure
            );
        }

        /// <summary>
        /// Recieve a message from a message bus with a correlation identifier.
        /// </summary>
        public static IDisposable ReceiveAsync<T>(
            this IBus messageBus,
            string queue,
            Func<T, Task> onRecievedMessage,
            Action<string> onRecievedCorrelationId,
            ILogger logger)
        {
            return messageBus.Receive<CorrelationMessage<T>>(
                queue,
                message => ReceiveMessageAsync(message, onRecievedMessage, onRecievedCorrelationId, logger)
            );
        }

        /// <summary />
        private static async Task ReceiveMessageAsync<T>(
            CorrelationMessage<T> message,
            Func<T, Task> onRecievedMessage,
            Action<string> onRecievedCorrelationId,
            ILogger logger)
        {
            onRecievedCorrelationId(message.CorrelationId);

            // Put correlation identifier into a log context.
            using (logger.BeginScope("{CorrelationId}", message.CorrelationId))
            {
                await onRecievedMessage(message.Body).ConfigureAwait(false);
            }
        }
    }
}
