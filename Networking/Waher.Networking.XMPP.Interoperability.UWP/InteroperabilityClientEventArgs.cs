using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Interoperability
{
	/// <summary>
	/// Event arguments for interoperability interface requests.
	/// </summary>
	public class InteroperabilityClientEventArgs : IqResultEventArgs
	{
		private readonly string[] interfaces;

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
		public string[] Interfaces => this.interfaces;
	}
}
