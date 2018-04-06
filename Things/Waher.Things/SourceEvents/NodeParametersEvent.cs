using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Abstract base class for all node events with parameters.
	/// </summary>
	public abstract class NodeParametersEvent : NodeStatusEvent 
    {
		private Parameter[] parameters = null;
		private string nodeType = string.Empty;
		private bool hasChildren = false;
		private bool isReadable = false;
		private bool isControllable = false;
		private bool hasCommands = false;
		private string parentId = string.Empty;
		private string parentPartition = string.Empty;
		private DateTime updated = DateTime.MinValue;

		/// <summary>
		/// Abstract base class for all node events with parameters.
		/// </summary>
		public NodeParametersEvent()
			: base()
		{
		}

		/// <summary>
		/// Displayable parameters.
		/// </summary>
		[DefaultValueNull]
		[ShortName("dp")]
		public Parameter[] Parameters
		{
			get { return this.parameters; }
			set { this.parameters = value; }
		}

		/// <summary>
		/// When node was last updated, at time of event.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		/// <summary>
		/// Type of node, at time of event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string NodeType
		{
			get { return this.nodeType; }
			set { this.nodeType = value; }
		}

		/// <summary>
		/// Parent identity of node, at time of event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ParentId
		{
			get { return this.parentId; }
			set { this.parentId = value; }
		}

		/// <summary>
		/// Partition of parent of node, at time of event.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ParentPartition
		{
			get { return this.parentPartition; }
			set { this.parentPartition = value; }
		}

		/// <summary>
		/// If node had children, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool HasChildren
		{
			get { return this.hasChildren; }
			set { this.hasChildren = value; }
		}

		/// <summary>
		/// If node was readable, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool IsReadable
		{
			get { return this.isReadable; }
			set { this.isReadable = value; }
		}

		/// <summary>
		/// If node was controllable, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool IsControllable
		{
			get { return this.isControllable; }
			set { this.isControllable = value; }
		}

		/// <summary>
		/// If node had commands, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool HasCommands
		{
			get { return this.hasCommands; }
			set { this.hasCommands = value; }
		}
	}
}
