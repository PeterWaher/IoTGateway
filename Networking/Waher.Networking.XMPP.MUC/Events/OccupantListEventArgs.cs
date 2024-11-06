using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.MUC
{
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
