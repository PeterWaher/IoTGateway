namespace Waher.Security.DTLS
{
	/// <summary>
	/// Event arguments for application data events.
	/// </summary>
	public class ApplicationDataEventArgs : RemoteEndpointEventArgs
	{
		private readonly byte[] applicationData;

		/// <summary>
		/// Event arguments for application data events.
		/// </summary>
		/// <param name="RemoteEndpoint">Remote endpoint.</param>
		/// <param name="ApplicationData">Application Data.</param>
		public ApplicationDataEventArgs(object RemoteEndpoint, byte[] ApplicationData)
			: base(RemoteEndpoint)
		{
			this.applicationData = ApplicationData;
		}

		/// <summary>
		/// Application Data.
		/// </summary>
		public byte[] ApplicationData => this.applicationData;
	}
}
