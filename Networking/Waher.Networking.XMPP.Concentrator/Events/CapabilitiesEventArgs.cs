﻿using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for capabilities callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CapabilitiesEventHandler(object Sender, CapabilitiesEventArgs e);

	/// <summary>
	/// Event arguments for capabilities responses.
	/// </summary>
	public class CapabilitiesEventArgs : IqResultEventArgs
	{
		private readonly string[] capabilities;

		internal CapabilitiesEventArgs(string[] Capabilities, IqResultEventArgs Response)
			: base(Response)
		{
			this.capabilities = Capabilities;
		}

		/// <summary>
		/// Capabilities of the concentrator server.
		/// </summary>
		public string[] Capabilities => this.capabilities;
	}
}
