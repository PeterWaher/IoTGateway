using System;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Delegate for custom presence XML event handlers.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task CustomPresenceEventHandler(object Sender, CustomPresenceEventArgs e);

	/// <summary>
	/// Event Argument for custom presence XML events.
	/// </summary>
	public class CustomPresenceEventArgs : EventArgs
	{
		private readonly StringBuilder stanza;
		private readonly Availability availability;

		/// <summary>
		/// Event Argument for custom presence XML events.
		/// </summary>
		/// <param name="Availability">Availability being set.</param>
		/// <param name="Stanza">Stanza being built.</param>
		public CustomPresenceEventArgs(Availability Availability, StringBuilder Stanza)
		{
			this.stanza = Stanza;
			this.availability = Availability;
		}

		/// <summary>
		/// Stanza being built.
		/// </summary>
		public StringBuilder Stanza => this.stanza;

		/// <summary>
		/// Availability being set.
		/// </summary>
		public Availability Availability => this.availability;
	}
}
