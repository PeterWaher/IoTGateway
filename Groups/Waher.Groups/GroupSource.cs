using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Groups.NodeTypes;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Runtime.Settings;
using Waher.Things;
using Waher.Things.SourceEvents;

namespace Waher.Groups
{
	/// <summary>
	/// Defines the Groups data source. This data source contains a tree structure of 
	/// groups of nodes
	/// </summary>
	public class GroupSource : IDataSource
	{
		/// <summary>
		/// Source ID for the groups data source.
		/// </summary>
		public const string SourceID = "Groups";

		private static readonly Dictionary<string, IDataSource> sources = new Dictionary<string, IDataSource>();
		private static readonly Dictionary<string, GroupNode> nodes = new Dictionary<string, GroupNode>();
		private static Root root = null;
		private static GroupSource instance = null;
		private static IDataSources dataSources = null;

		private DateTime lastChanged;

		/// <summary>
		/// Defines the Metering Topology data source. This data source contains a tree structure of persistent 
		/// readable and controllable devices
		/// </summary>
		public GroupSource()
		{
			this.lastChanged = RuntimeSettings.Get(SourceID + ".LastChanged", DateTime.MinValue);
			instance ??= this;
		}

		/// <summary>
		/// ID of data source.
		/// </summary>
		string IDataSource.SourceID => SourceID;

		/// <summary>
		/// Child sources. If no child sources are available, null is returned.
		/// </summary>
		public IEnumerable<IDataSource> ChildSources => null;

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		public bool HasChildren => false;

		/// <summary>
		/// When the source was last updated.
		/// </summary>
		public DateTime LastChanged
		{
			get => this.lastChanged;
			internal set
			{
				if (this.lastChanged != value)
				{
					this.lastChanged = value;
					Task _ = RuntimeSettings.SetAsync("GroupSource.LastChanged", value);
				}
			}
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized name of data source.</returns>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GroupSource), 1, "Groups");
		}

		/// <summary>
		/// Gets the node, given a reference to it.
		/// </summary>
		/// <param name="NodeRef">Node reference.</param>
		/// <returns>Node, if found, null otherwise.</returns>
		public async Task<INode> GetNodeAsync(IThingReference NodeRef)
		{
			return await GetNode(NodeRef);
		}

		/// <summary>
		/// Gets a node from the Metering Topology
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <returns>Node, if found, null otherwise.</returns>
		public static Task<GroupNode> GetNode(string NodeId)
		{
			return GetNode(new ThingReference(NodeId, SourceID));
		}

		/// <summary>
		/// Gets a node from the Metering Topology
		/// </summary>
		/// <param name="NodeRef">Node reference</param>
		/// <returns>Node, if found, null otherwise.</returns>
		public static async Task<GroupNode> GetNode(IThingReference NodeRef)
		{
			if (NodeRef is null || NodeRef.SourceId != SourceID || !string.IsNullOrEmpty(NodeRef.Partition))
				return null;

			lock (nodes)
			{
				if (nodes.TryGetValue(NodeRef.NodeId, out GroupNode Node))
					return Node;
			}

			foreach (GroupNode Node2 in await Database.Find<GroupNode>(new FilterFieldEqualTo("NodeId", NodeRef.NodeId)))
			{
				lock (nodes)
				{
					if (nodes.TryGetValue(NodeRef.NodeId, out GroupNode Node))
						return Node;
					else
					{
						nodes[NodeRef.NodeId] = Node2;
						return Node2;
					}
				}
			}

			return null;
		}

		internal static GroupNode RegisterNode(GroupNode Node)
		{
			if (Node.SourceId == SourceID && string.IsNullOrEmpty(Node.Partition))
			{
				lock (nodes)
				{
					if (nodes.TryGetValue(Node.NodeId, out GroupNode Node2))
						return Node2;
					else
					{
						nodes[Node.NodeId] = Node;
						return Node;
					}
				}
			}
			else
				return Node;
		}

		internal static GroupNode RegisterNewNodeId(GroupNode Node, string OldId)
		{
			if (Node.SourceId != SourceID || !string.IsNullOrEmpty(Node.Partition))
				return Node;

			lock (nodes)
			{
				if (!nodes.TryGetValue(OldId, out GroupNode Node2) || Node2 != Node)
					return Node;

				if (nodes.TryGetValue(Node.NodeId, out Node2))
					return Node2;

				nodes.Remove(OldId);
				nodes[Node.NodeId] = Node;

				return Node;
			}
		}

		internal static void UnregisterNode(GroupNode Node)
		{
			if (Node.SourceId == SourceID && string.IsNullOrEmpty(Node.Partition))
			{
				lock (nodes)
				{
					if (nodes.TryGetValue(Node.NodeId, out GroupNode Node2) && Node == Node2)
						nodes.Remove(Node.NodeId);
				}
			}
		}

		/// <summary>
		/// If the data source is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the source is visible to the caller.</returns>
		public Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult(Caller.HasPrivilege("Source." + SourceID + ".View"));
		}

		/// <summary>
		/// Root node references. If no root nodes are available, null is returned.
		/// </summary>
		public IEnumerable<INode> RootNodes
		{
			get
			{
				if (root is null)
					LoadRoot().Wait();

				return new INode[] { root };
			}
		}

		/// <summary>
		/// Root node.
		/// </summary>
		public static Root Root
		{
			get
			{
				if (root is null)
					LoadRoot().Wait();

				return root;
			}
		}

		private static async Task LoadRoot()
		{
			Root Result = null;

			foreach (GroupNode Node in await Database.Find<GroupNode>(new FilterFieldEqualTo("ParentId", Guid.Empty)))
			{
				if (Node is Root Root)
				{
					if (Result is null)
						Result = Root;
					else
						await Database.Delete(Node);
				}
			}

			if (Result is null)
			{
				Result = new Root()
				{
					NodeId = await (await Translator.GetDefaultLanguageAsync()).GetStringAsync(typeof(GroupSource), 5, "Root")
				};

				await Database.Insert(Result);

				Language Language = await Translator.GetDefaultLanguageAsync();
				await NewEvent(new NodeAdded()
				{
					Parameters = await Result.GetDisplayableParameterAraryAsync(Language, RequestOrigin.Empty),
					NodeType = Result.GetType().FullName,
					Sniffable = false,
					DisplayName = await Result.GetTypeNameAsync(Language),
					HasChildren = Result.HasChildren,
					ChildrenOrdered = Result.ChildrenOrdered,
					IsReadable = Result.IsReadable,
					IsControllable = Result.IsControllable,
					HasCommands = Result.HasCommands,
					ParentId = string.Empty,
					ParentPartition = string.Empty,
					Updated = Result.Updated,
					State = Result.State,
					NodeId = Result.NodeId,
					Partition = Result.Partition,
					LogId = NodeAdded.EmptyIfSame(Result.LogId, Result.NodeId),
					LocalId = NodeAdded.EmptyIfSame(Result.LocalId, Result.NodeId),
					SourceId = Result.SourceId,
					Timestamp = DateTime.Now
				});
			}

			lock (nodes)
			{
				nodes[Result.NodeId] = Result;
			}

			root = Result;
		}

		/// <summary>
		/// Deletes old data source events.
		/// </summary>
		/// <param name="MaxAge">Maximum age of events to keep.</param>
		/// <returns>Number of events deleted.</returns>
		public static async Task<int> DeleteOldEvents(TimeSpan MaxAge)
		{
			if (MaxAge <= TimeSpan.Zero)
				throw new ArgumentException("Age must be positive.", nameof(MaxAge));

			DateTime Limit = DateTime.Now.Subtract(MaxAge);
			int NrEvents = 0;
			bool Deleted;

			do
			{
				Deleted = false;

				foreach (SourceEvent Event in await Database.FindDelete<SourceEvent>(0, 100, new FilterAnd(
					new FilterFieldEqualTo("SourceId", SourceID), new FilterFieldLesserOrEqualTo("Timestamp", Limit))))
				{
					NrEvents++;
					Deleted = true;
				}
			}
			while (Deleted);

			if (NrEvents > 0)
			{
				KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("Limit", Limit),
					new KeyValuePair<string, object>("NrEvents", NrEvents)
				};

				if (NrEvents == 1)
					Log.Informational("Deleting 1 logical group event from the database.", SourceID, Tags);
				else
					Log.Informational("Deleting " + NrEvents.ToString() + " logical group events from the database.", SourceID, Tags);
			}

			return NrEvents;
		}

		/// <summary>
		/// Event raised when a data source event has been raised.
		/// </summary>
		public event EventHandlerAsync<SourceEvent> OnEvent = null;

		internal static async Task NewEvent(SourceEvent Event)
		{
			await Database.Insert(Event);
			await (instance?.OnEvent?.Raise(instance, Event) ?? Task.CompletedTask);
		}

		/// <summary>
		/// Deletes orphaned nodes in the metering topology source.
		/// </summary>
		/// <returns>Number of nodes deleted.</returns>
		public static async Task<int> DeleteOrphans()
		{
			int Result = 0;

			foreach (GroupNode Node in await Database.Find<GroupNode>())
			{
				if (Node.ParentId == Guid.Empty)
					continue;

				GroupNode ParentNode = await Database.TryLoadObject<GroupNode>(Node.ParentId);
				if (ParentNode is null)
				{
					await Database.Delete(Node);
					Result++;

					lock (nodes)
					{
						nodes.Remove(Node.NodeId);
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Tries to get a data source, given its Source ID.
		/// </summary>
		/// <param name="SourceId">Data Source ID.</param>
		/// <param name="Source">Source, if found.</param>
		/// <returns>If a source was found with the corresponding ID.</returns>
		public static bool TryGetDataSource(string SourceId, out IDataSource Source)
		{
			lock (sources)
			{
				if (sources.TryGetValue(SourceId, out Source))
					return !(Source is null);
			}

			if (dataSources is null)
				Types.TryGetModuleParameter("Concentrator", out dataSources);

			if (dataSources is null || !dataSources.TryGetDataSource(SourceId, out Source))
				Source = null;

			lock (sources)
			{
				sources[SourceId] = Source;
			}

			return !(Source is null);
		}
	}
}
