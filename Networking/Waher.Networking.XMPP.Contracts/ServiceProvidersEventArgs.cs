using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Delegate for service provider callback methods.
	/// </summary>
	/// <typeparam name="T">Service-provider type.</typeparam>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments</param>
	public delegate Task ServiceProvidersEventHandler<T>(object Sender, ServiceProvidersEventArgs<T> e)
        where T : IServiceProvider;

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
