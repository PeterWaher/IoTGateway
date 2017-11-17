using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Delegate for CanControl callback methods.
	/// </summary>
	/// <param name="Sender">Sender</param>
	/// <param name="e">Event arguments.</param>
	public delegate void CanControlCallback(object Sender, CanControlResponseEventArgs e);

	/// <summary>
	/// Event arguments for CanControl callback event arguments.
	/// </summary>
	public class CanControlResponseEventArgs : NodesEventArgs
	{
		private bool canControl;
		private string[] parameterNames;

		internal CanControlResponseEventArgs(IqResultEventArgs e, object State, string JID, bool CanControl, 
			ThingReference[] Nodes, string[] ParameterNames)
			: base(e, State, JID, Nodes)
		{
			this.canControl = CanControl;
			this.parameterNames = ParameterNames;
		}

		/// <summary>
		/// If the control operation can be performed.
		/// </summary>
		public bool CanControl
		{
			get { return this.canControl; }
		}

		/// <summary>
		/// Parameter names allowed to be processed. If null, no parameter restrictions exist.
		/// </summary>
		public string[] ParameterNames
		{
			get { return this.parameterNames; }
		}
	}
}
