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
		/// <param name="State">Remote endpoint state object.</param>
		/// <param name="ApplicationData">Application Data.</param>
		public ApplicationDataEventArgs(EndpointState State, byte[] ApplicationData)
			: base(State)
		{
			this.applicationData = ApplicationData;
		}

		/// <summary>
		/// Application Data.
		/// </summary>
		public byte[] ApplicationData => this.applicationData;
	}
}
