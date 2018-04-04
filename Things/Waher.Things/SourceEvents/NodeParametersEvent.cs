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
		private DateTime lastChanged = DateTime.MinValue;

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
		/// When node was last changed, at time of event.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime LastChanged
		{
			get { return this.lastChanged; }
			set { this.lastChanged = value; }
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
