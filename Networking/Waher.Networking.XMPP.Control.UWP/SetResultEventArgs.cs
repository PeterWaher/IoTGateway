using Waher.Networking.XMPP.Events;
using Waher.Things;

namespace Waher.Networking.XMPP.Control
{
	/// <summary>
	/// Event arguments for set-result callback methods.
	/// </summary>
	public class SetResultEventArgs : IqResultEventArgs
	{
		private readonly bool performed;
		private readonly ThingReference[] nodes;
		private readonly string[] parameterNames;

		internal SetResultEventArgs(IqResultEventArgs e, bool Performed, ThingReference[] Nodes, string[] ParameterNames, object State)
			: base(e)
		{
			this.performed = Performed;
			this.nodes = Nodes;
			this.parameterNames = ParameterNames;
			this.State = State;
		}

		/// <summary>
		/// If set-operation was performed or not. Partial operations also return true.
		/// </summary>
		public bool Performed => this.performed;

		/// <summary>
		/// Any nodes returnded in response. This array can be compared to the original array to see if a partial operation was 
		/// performed. If this array is null, all nodes in the original request were set.
		/// </summary>
		public ThingReference[] Nodes => this.nodes;

		/// <summary>
		/// Any parameter names returnded in response. This array can be compared to the original array to see if a partial operation 
		/// was performed. If this array is null, all parameters were set.
		/// </summary>
		public string[] ParameterNames => this.parameterNames;
	}
}
