using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.MUC
{
	/// <summary>
	/// Delegate for occupant list callback events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task OccupantListEventHandler(object Sender, OccupantListEventArgs e);

	/// <summary>
	/// Event arguments for a occupant list event handlers.
	/// </summary>
	public class OccupantListEventArgs : IqResultEventArgs
	{
		private readonly MucOccupant[] occupants;

		/// <summary>
		/// Event arguments for a occupant list event handlers.
		/// </summary>
		/// <param name="e">IQ Result event arguments.</param>
		/// <param name="Occupants">Occupants</param>
		public OccupantListEventArgs(IqResultEventArgs e, MucOccupant[] Occupants) 
			: base(e)
		{
			this.occupants = Occupants;
		}

		/// <summary>
		/// List of occupants.
		/// </summary>
		public MucOccupant[] Occupants => this.occupants;
	}
}
