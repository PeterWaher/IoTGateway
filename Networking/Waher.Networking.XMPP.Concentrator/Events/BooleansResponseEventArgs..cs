using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Event arguments for boolean responses responsess.
	/// </summary>
	public class BooleansResponseEventArgs : IqResultEventArgs
	{
		private readonly bool[] result;

		internal BooleansResponseEventArgs(bool[] Result, IqResultEventArgs Responses)
			: base(Responses)
		{
			this.result = Result;
		}

		/// <summary>
		/// Result of operation.
		/// </summary>
		public bool[] Result => this.result;
	}
}
