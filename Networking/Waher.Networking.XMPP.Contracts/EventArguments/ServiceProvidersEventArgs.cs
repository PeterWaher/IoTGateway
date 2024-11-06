using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Contracts.EventArguments
{
    /// <summary>
    /// Event arguments for Service Provider callback methods.
    /// </summary>
    /// <typeparam name="T">Service-provider type.</typeparam>
    public class ServiceProvidersEventArgs<T> : IqResultEventArgs
        where T : IServiceProvider
    {
        private readonly T[] providers;

        internal ServiceProvidersEventArgs(IqResultEventArgs e, T[] ServiceProviders)
            : base(e)
        {
            this.providers = ServiceProviders;
        }

        /// <summary>
        /// Service Providers.
        /// </summary>
        public T[] ServiceProviders => this.providers;
    }
}
