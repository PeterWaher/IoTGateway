using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Abstract base class for all node status events.
	/// </summary>
	public abstract class NodeStatusEvent : NodeEvent 
    {
		private NodeState state = NodeState.None;

		/// <summary>
		/// Abstract base class for all node status events.
		/// </summary>
		public NodeStatusEvent()
			: base()
		{
		}

		/// <summary>
		/// State of node, at time of event.
		/// </summary>
		[DefaultValue(NodeState.None)]
		public NodeState State
		{
			get { return this.state; }
			set { this.state = value; }
		}
	}
}
