using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Language;
using Waher.Runtime.Settings;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.SensorData;
using Waher.Things.SourceEvents;

namespace Waher.Things.Metering
{
	/// <summary>
	/// Delegate for new momentary values event handlers.
	/// </summary>
	/// <param name="Reference">Thing reporting new momentary values.</param>
	/// <param name="Values">New momentary values.</param>
	public delegate void NewMomentaryValuesHandler(IThingReference Reference, IEnumerable<Field> Values);

	/// <summary>
	/// Defines the Metering Topology data source. This data source contains a tree structure of persistent 
	/// readable and controllable devices
	/// </summary>
	public class MeteringTopology : IDataSource
	{
		/// <summary>
		/// Source ID for the metering topology data source.
		/// </summary>
		public const string SourceID = "MeteringTopology";

		private static readonly Dictionary<string, MeteringNode> nodes = new Dictionary<string, MeteringNode>();
		private static Root root = null;
		private static MeteringTopology instance = null;

		private DateTime lastChanged;

		/// <summary>
		/// Defines the Metering Topology data source. This data source contains a tree structure of persistent 
		/// readable and controllable devices
		/// </summary>
		public MeteringTopology()
		{
			lastChanged = RuntimeSettings.Get(MeteringTopology.SourceID + ".LastChanged", DateTime.MinValue);

			if (instance is null)
				instance = this;
		}

		/// <summary>
		/// ID of data source.
		/// </summary>
		string IDataSource.SourceID
		{
			get { return MeteringTopology.SourceID; }
		}

		/// <summary>
		/// Child sources. If no child sources are available, null is returned.
		/// </summary>
		public IEnumerable<IDataSource> ChildSources
		{
			get { return null; }
		}

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		public bool HasChildren
		{
			get { return false; }
		}

		/// <summary>
		/// When the source was last updated.
		/// </summary>
		public DateTime LastChanged
		{
			get { return lastChanged; }
			internal set
			{
				if (lastChanged != value)
				{
					lastChanged = value;
					Task _ = RuntimeSettings.SetAsync("MeteringTopology.LastChanged", value);
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
			return Language.GetStringAsync(typeof(MeteringTopology), 13, "Metering Topology");
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

		internal static async Task<MeteringNode> GetNode(IThingReference NodeRef)
		{
			if (NodeRef.SourceId != MeteringTopology.SourceID || !string.IsNullOrEmpty(NodeRef.Partition))
				return null;

			lock (nodes)
			{
				if (nodes.TryGetValue(NodeRef.NodeId, out MeteringNode Node))
					return Node;
			}

			foreach (MeteringNode Node2 in await Database.Find<MeteringNode>(new FilterFieldEqualTo("NodeId", NodeRef.NodeId)))
			{
				lock (nodes)
				{
					if (nodes.TryGetValue(NodeRef.NodeId, out MeteringNode Node))
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

		internal static MeteringNode RegisterNode(MeteringNode Node)
		{
			if (Node.SourceId == MeteringTopology.SourceID && string.IsNullOrEmpty(Node.Partition))
			{
				lock (nodes)
				{
					if (nodes.TryGetValue(Node.NodeId, out MeteringNode Node2))
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

		internal static void UnregisterNode(MeteringNode Node)
		{
			if (Node.SourceId == MeteringTopology.SourceID && string.IsNullOrEmpty(Node.Partition))
			{
				lock (nodes)
				{
					if (nodes.TryGetValue(Node.NodeId, out MeteringNode Node2) && Node == Node2)
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
			return Task.FromResult<bool>(true);     // TODO: Check user privileges
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

			foreach (MeteringNode Node in await Database.Find<MeteringNode>(new FilterFieldEqualTo("ParentId", Guid.Empty)))
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
					NodeId = await (await Translator.GetDefaultLanguageAsync()).GetStringAsync(typeof(MeteringTopology), 14, "Root")
				};

				await Database.Insert(Result);

				Language Language = await Translator.GetDefaultLanguageAsync();
				await MeteringTopology.NewEvent(new NodeAdded()
				{
					Parameters = await Result.GetDisplayableParameterAraryAsync(Language, RequestOrigin.Empty),
					NodeType = Result.GetType().FullName,
					Sniffable = Result is ISniffable,
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
					Log.Informational("Deleting 1 metering topology event from the database.", SourceID, Tags);
				else
					Log.Informational("Deleting " + NrEvents.ToString() + " metering topology events from the database.", SourceID, Tags);
			}

			return NrEvents;
		}

		/// <summary>
		/// Event raised when a data source event has been raised.
		/// </summary>
		public event SourceEventEventHandler OnEvent = null;

		internal static async Task NewEvent(SourceEvent Event)
		{
			await Database.InsertLazy(Event);

			try
			{
				instance?.OnEvent?.Invoke(instance, Event);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		public static void NewMomentaryValues(IThingReference Reference, IEnumerable<Field> Values)
		{
			OnNewMomentaryValues?.Invoke(Reference, Values);
		}

		/// <summary>
		/// Event raised when a node in the metering topology reports a new momentary value.
		/// </summary>
		public static event NewMomentaryValuesHandler OnNewMomentaryValues = null;

	}
}
