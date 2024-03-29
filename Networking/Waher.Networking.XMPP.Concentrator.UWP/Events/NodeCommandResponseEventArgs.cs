﻿using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for node command callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task NodeCommandResponseEventHandler(object Sender, NodeCommandResponseEventArgs e);

	/// <summary>
	/// Event arguments for node command events.
	/// </summary>
	public class NodeCommandResponseEventArgs : IqResultEventArgs
	{
		private readonly DataForm parameters;

		internal NodeCommandResponseEventArgs(DataForm Parameters, IqResultEventArgs e)
			: base(e)
		{
			this.parameters = Parameters;
		}

		/// <summary>
		/// Node command parameters
		/// </summary>
		public DataForm Parameters => this.parameters;
	}
}
