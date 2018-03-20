using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things;
using Waher.Things.DisplayableParameters;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Contains information about a node.
	/// </summary>
	public class NodeInformation : IThingReference
	{
		private string nodeId;
		private string sourceId;
		private string partition;
		private string nodeType;
		private string displayName;
		private NodeState nodeState;
		private string localId;
		private string logId;
		private bool hasChildren;
		private bool childrenOrdered;
		private bool isReadable;
		private bool isControllable;
		private bool hasCommands;
		private bool sniffable;
		private string parentId;
		private string parentPartition;
		private DateTime lastChanged;
		private Parameter[] parameterList;
		private Message[] messageList;

		/// <summary>
		/// Contains information about a node.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="SourceId">Source ID</param>
		/// <param name="Partition">Partition</param>
		/// <param name="NodeType">Node Type.</param>
		/// <param name="DisplayName">Display name.</param>
		/// <param name="NodeState">State</param>
		/// <param name="LocalId">Local ID</param>
		/// <param name="LogId">Log ID</param>
		/// <param name="HasChildren">If the node has child nodes.</param>
		/// <param name="ChildrenOrdered">If the child nodes are ordered.</param>
		/// <param name="IsReadable">If the node is readable.</param>
		/// <param name="IsControllable">If the node is controllable.</param>
		/// <param name="HasCommands">If the node has commands.</param>
		/// <param name="Sniffable">If the node is sniffable.</param>
		/// <param name="ParentId">Node ID of parent node.</param>
		/// <param name="ParentPartition">Partition of parent node.</param>
		/// <param name="LastChanged">When the node was last changed.</param>
		/// <param name="ParameterList">List of displayable parameters.</param>
		/// <param name="MessageList">List of messages.</param>
		public NodeInformation(string NodeId, string SourceId, string Partition, string NodeType, string DisplayName, NodeState NodeState,
			string LocalId, string LogId, bool HasChildren, bool ChildrenOrdered, bool IsReadable, bool IsControllable, bool HasCommands,
			bool Sniffable, string ParentId, string ParentPartition, DateTime LastChanged, Parameter[] ParameterList, Message[] MessageList)
		{
			this.nodeId = NodeId;
			this.sourceId = SourceId;
			this.partition = Partition;
			this.nodeType = NodeType;
			this.displayName = DisplayName;
			this.nodeState = NodeState;
			this.localId = LocalId;
			this.logId = LogId;
			this.hasChildren = HasChildren;
			this.childrenOrdered = ChildrenOrdered;
			this.isReadable = IsReadable;
			this.isControllable = IsControllable;
			this.hasCommands = HasCommands;
			this.sniffable = Sniffable;
			this.parentId = ParentId;
			this.parentPartition = ParentPartition;
			this.lastChanged = LastChanged;
			this.parameterList = ParameterList;
			this.messageList = MessageList;
		}

		/// <summary>
		/// Node ID
		/// </summary>
		public string NodeId => this.nodeId;

		/// <summary>
		/// Source ID
		/// </summary>
		public string SourceId => this.sourceId;

		/// <summary>
		/// Partition
		/// </summary>
		public string Partition => this.partition;

		/// <summary>
		/// Node Type
		/// </summary>
		public string NodeType => this.nodeType;

		/// <summary>
		/// Display name.
		/// </summary>
		public string DisplayName => this.displayName;

		/// <summary>
		/// Node state.
		/// </summary>
		public NodeState NodeState => this.nodeState;

		/// <summary>
		/// Local ID.
		/// </summary>
		public string LocalId => this.localId;

		/// <summary>
		/// Log ID.
		/// </summary>
		public string LogId => this.logId;

		/// <summary>
		/// If the node has child nodes.
		/// </summary>
		public bool HasChildren => this.hasChildren;

		/// <summary>
		/// If the child nodes are ordered.
		/// </summary>
		public bool ChildrenOrdered => this.childrenOrdered;

		/// <summary>
		/// If the node is readable.
		/// </summary>
		public bool IsReadable => this.isReadable;

		/// <summary>
		/// If the node is controllable.
		/// </summary>
		public bool IsControllable => this.isControllable;

		/// <summary>
		/// If the node has commands.
		/// </summary>
		public bool HasCommands => this.hasCommands;

		/// <summary>
		/// If the node is sniffable.
		/// </summary>
		public bool Sniffable => this.sniffable;

		/// <summary>
		/// The Ndoe ID of the parent node.
		/// </summary>
		public string ParentId => this.parentId;

		/// <summary>
		/// The partition of the parent node.
		/// </summary>
		public string ParentPartition => this.parentPartition;

		/// <summary>
		/// When the node was last changed.
		/// </summary>
		public DateTime LastChanged => this.lastChanged;

		/// <summary>
		/// List of displayable parameters.
		/// </summary>
		public Parameter[] ParameterList => this.parameterList;

		/// <summary>
		/// List of messages.
		/// </summary>
		public Message[] MessageList => this.messageList;
	}
}
