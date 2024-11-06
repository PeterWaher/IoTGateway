using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Synchronization
{
	/// <summary>
	/// Event arguments containing the response of a clock source request.
	/// </summary>
	public class ClockSourceEventArgs : IqResultEventArgs
    {
		private readonly string clockSourceJID;
		
		internal ClockSourceEventArgs(string Jid, IqResultEventArgs e)
			: base(e)
		{
			this.clockSourceJID = Jid;
		}

		/// <summary>
		/// JID of clock source.
		/// </summary>
		public string ClockSourceJID => this.clockSourceJID;
	}
}
