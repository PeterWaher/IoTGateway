using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
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
