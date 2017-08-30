using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.LWM2M
{
	/// <summary>
	/// Event handler for server reference events.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e"></param>
	public delegate void Lwm2mServerReferenceEventHandler(object Sender, Lwm2mServerReferenceEventArgs e);

	/// <summary>
	/// Event arguments for LWM2M server reference events.
	/// </summary>
	public class Lwm2mServerReferenceEventArgs : EventArgs
    {
		private Lwm2mServerReference server;

		/// <summary>
		/// Event arguments for LWM2M server reference events.
		/// </summary>
		/// <param name="Server">Server reference.</param>
		public Lwm2mServerReferenceEventArgs(Lwm2mServerReference Server)
		{
			this.server = Server;
		}

		/// <summary>
		/// Server reference.
		/// </summary>
		public Lwm2mServerReference Server
		{
			get { return this.server; }
		}
	}
}
