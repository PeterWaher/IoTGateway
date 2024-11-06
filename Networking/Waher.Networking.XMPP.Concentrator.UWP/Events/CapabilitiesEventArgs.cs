using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
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
