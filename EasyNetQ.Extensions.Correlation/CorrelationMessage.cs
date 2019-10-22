namespace EasyNetQ.Extensions.Correlation
{
    /// <summary>
    /// Class that contains information about correlation.
    /// </summary>
    internal sealed class CorrelationMessage<T>
    {
        /// <summary>
        /// Unique correlation identifier.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Body of a message.
        /// </summary>
        public T Body { get; set; }
    }
}
