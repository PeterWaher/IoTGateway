using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things;

namespace Waher.Networking.XMPP.Interoperability
{
	/// <summary>
	/// Delegate for interoperability interfaces event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void InteroperabilityInterfacesClientEventHandler(object Sender, InteroperabilityClientEventArgs e);

	/// <summary>
	/// Event arguments for interoperability interface requests.
	/// </summary>
	public class InteroperabilityClientEventArgs : IqResultEventArgs
	{
		private string[] interfaces;

		/// <summary>
		/// Event arguments for interoperability interface requests.
		/// </summary>
		public InteroperabilityClientEventArgs(string[] Interfaces, IqResultEventArgs e)
			: base(e)
		{
			this.interfaces = Interfaces;
		}

		/// <summary>
		/// Reported Interoperability Interfaces.
		/// </summary>
		public string[] Interfaces
		{
			get { return this.interfaces; }
		}

	}
}
