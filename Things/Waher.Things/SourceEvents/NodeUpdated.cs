using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Node updated event.
	/// </summary>
	public class NodeUpdated : NodeParametersEvent
    {
		private string oldId = string.Empty;

		/// <summary>
		/// Node updated event.
		/// </summary>
		public NodeUpdated()
			: base()
		{
		}

		/// <summary>
		/// Creates an event object from a node object.
		/// </summary>
		/// <param name="Node">Node added.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Original caller.</param>
		public static Task<NodeUpdated> FromNode(INode Node, Language Language, RequestOrigin Caller)
		{
			return FromNode(Node, Language, Caller, string.Empty, null);
		}

		/// <summary>
		/// Creates an event object from a node object.
		/// </summary>
		/// <param name="Node">Node added.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Original caller.</param>
		/// <param name="AdditionalParameters">Additional node parameters.</param>
		public static Task<NodeUpdated> FromNode(INode Node, Language Language, RequestOrigin Caller,
			params Parameter[] AdditionalParameters)
		{
			return FromNode(Node, Language, Caller, string.Empty, AdditionalParameters);
		}

		/// <summary>
		/// Creates an event object from a node object.
		/// </summary>
		/// <param name="Node">Node added.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Original caller.</param>
		/// <param name="OldId">Old Node ID, if differrent from previous NodeId.</param>
		public static Task<NodeUpdated> FromNode(INode Node, Language Language, RequestOrigin Caller, string OldId)
		{
			return FromNode(Node, Language, Caller, OldId, null);
		}

		/// <summary>
		/// Creates an event object from a node object.
		/// </summary>
		/// <param name="Node">Node added.</param>
		/// <param name="Language">Language</param>
		/// <param name="Caller">Original caller.</param>
		/// <param name="OldId">Old Node ID, if differrent from previous NodeId.</param>
		/// <param name="AdditionalParameters">Additional node parameters.</param>
		public static async Task<NodeUpdated> FromNode(INode Node, Language Language, RequestOrigin Caller, string OldId,
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

			return new NodeUpdated()
			{
				OldId = OldId,
				Parameters = Parameters.ToArray(),
				HasChildren = Node.HasChildren,
				ChildrenOrdered = Node.ChildrenOrdered,
				IsReadable = Node.IsReadable,
				IsControllable = Node.IsControllable,
				HasCommands = Node.HasCommands,
				ParentId = Node.Parent?.NodeId ?? string.Empty,
				ParentPartition = Node.Parent?.Partition ?? string.Empty,
				Updated = DateTime.Now,
				State = Node.State,
				NodeId = Node.NodeId,
				Partition = Node.Partition,
				LogId = NodeAdded.EmptyIfSame(Node.LogId, Node.NodeId),
				LocalId = NodeAdded.EmptyIfSame(Node.LocalId, Node.NodeId),
				SourceId = Node.SourceId,
				Timestamp = DateTime.Now
			};
		}

		/// <summary>
		/// If renamed, this property contains the node identity before the node was renamed.
		/// </summary>
		[DefaultValueStringEmpty]
		public string OldId
		{
			get { return this.oldId; }
			set { this.oldId = value; }
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public override SourceEventType EventType
		{
			get { return SourceEventType.NodeUpdated; }
		}
	}
}
