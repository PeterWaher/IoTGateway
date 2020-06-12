using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node added event.
	/// </summary>
	public class NodeAdded : NodeParametersEvent
	{
		private string afterNodeId = string.Empty;
		private string afterPartition = string.Empty;
		private string displayName = string.Empty;
		private string nodeType = string.Empty;
		private bool sniffable = false;

		/// <summary>
		/// Node added event.
		/// </summary>
		public NodeAdded()
			: base()
		{
		}

		/// <summary>
		/// Creates an event object from a node object.
		/// </summary>
		/// <param name="Node">Node added.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Original caller.</param>
		/// <param name="Sniffable">If the node is sniffable.</param>
		public static Task<NodeAdded> FromNode(INode Node, Language Language, RequestOrigin Caller, bool Sniffable)
		{
			return FromNode(Node, Language, Caller, Sniffable, null);
		}

		/// <summary>
		/// Creates an event object from a node object.
		/// </summary>
		/// <param name="Node">Node added.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Original caller.</param>
		/// <param name="Sniffable">If the node is sniffable.</param>
		/// <param name="AdditionalParameters">Additional node parameters.</param>
		public static async Task<NodeAdded> FromNode(INode Node, Language Language, RequestOrigin Caller, bool Sniffable,
			params Parameter[] AdditionalParameters)
		{
			List<Parameter> Parameters = new List<Parameter>();

			foreach (Parameter P in await Node.GetDisplayableParametersAsync(Language, Caller))
				Parameters.Add(P);

			if (!(AdditionalParameters is null))
			{
				foreach (Parameter P in AdditionalParameters)
					Parameters.Add(P);
			}

			return new NodeAdded()
			{
				Parameters = Parameters.ToArray(),
				NodeType = Node.GetType().FullName,
				Sniffable = Sniffable,
				DisplayName = await Node.GetTypeNameAsync(Language),
				HasChildren = Node.HasChildren,
				ChildrenOrdered = Node.ChildrenOrdered,
				IsReadable = Node.IsReadable,
				IsControllable = Node.IsControllable,
				HasCommands = Node.HasCommands,
				ParentId = Node.Parent?.NodeId ?? string.Empty,
				ParentPartition = Node.Parent?.Partition ?? string.Empty,
				Updated = DateTime.MinValue,
				State = Node.State,
				NodeId = Node.NodeId,
				Partition = Node.Partition,
				LogId = EmptyIfSame(Node.LogId, Node.NodeId),
				LocalId = EmptyIfSame(Node.LocalId, Node.NodeId),
				SourceId = Node.SourceId,
				Timestamp = DateTime.Now
			};
		}

		/// <summary>
		/// Returns <paramref name="Id1"/> if different, <see cref="string.Empty"/> if the same.
		/// </summary>
		/// <param name="Id1">ID 1</param>
		/// <param name="Id2">ID 2</param>
		/// <returns><paramref name="Id1"/> if different, <see cref="string.Empty"/> if the same.</returns>
		public static string EmptyIfSame(string Id1, string Id2)
		{
			return Id1 == Id2 ? string.Empty : Id1;
		}

		/// <summary>
		/// In an ordered set of nodes, the new node is added after this node, if provided.
		/// </summary>
		[DefaultValueStringEmpty]
		public string AfterNodeId
		{
			get { return this.afterNodeId; }
			set { this.afterNodeId = value; }
		}

		/// <summary>
		/// In an ordered set of nodes, the new node is added after the node defined by <see cref="AfterNodeId"/>, in this partition, if provided.
		/// </summary>
		[DefaultValueStringEmpty]
		public string AfterPartition
		{
			get { return this.afterPartition; }
			set { this.afterPartition = value; }
		}

		/// <summary>
		/// Display name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string DisplayName
		{
			get { return this.displayName; }
			set { this.displayName = value; }
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
		/// If node was sniffable, at time of event.
		/// </summary>
		[DefaultValue(false)]
		public bool Sniffable
		{
			get { return this.sniffable; }
			set { this.sniffable = value; }
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public override SourceEventType EventType
		{
			get { return SourceEventType.NodeAdded; }
		}
	}
}
