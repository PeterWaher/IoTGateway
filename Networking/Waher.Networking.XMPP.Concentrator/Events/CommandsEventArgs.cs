using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Delegate for command array responses callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CommandsEventHandler(object Sender, CommandsEventArgs e);

	/// <summary>
	/// Event arguments for command array responses responsess.
	/// </summary>
	public class CommandsEventArgs : IqResultEventArgs
	{
		private readonly NodeCommand[] result;

		internal CommandsEventArgs(NodeCommand[] Result, IqResultEventArgs s)
			: base(s)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public NodeCommand[] Result
		{
			get { return this.result; }
		}
	}
}
