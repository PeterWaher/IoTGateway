using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.DTLS
{
	/// <summary>
	/// Delegate for application data events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void ApplicationDataEventHandler(object Sender, ApplicationDataEventArgs e);

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
		public byte[] ApplicationData
		{
			get { return this.applicationData; }
		}
	}
}
