using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Language;
using Waher.Runtime.Settings;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Metering
{
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

		private Root root = null;
		private DateTime lastChanged;

		/// <summary>
		/// Defines the Metering Topology data source. This data source contains a tree structure of persistent 
		/// readable and controllable devices
		/// </summary>
		public MeteringTopology()
		{
			lastChanged = RuntimeSettings.Get(MeteringTopology.SourceID + ".LastChanged", DateTime.MinValue);
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
					Task T = RuntimeSettings.SetAsync("MeteringTopology.LastChanged", value);
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
			if (NodeRef.SourceId != MeteringTopology.SourceID || !string.IsNullOrEmpty(NodeRef.CacheType))
				return null;

			foreach (MeteringNode Node in await Database.Find<MeteringNode>(new FilterFieldEqualTo("NodeId", NodeRef.NodeId)))
				return Node;

			return null;
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
				if (this.root == null)
					this.LoadRoot().Wait();

				yield return this.root;
			}
		}

		private async Task LoadRoot()
		{
			Root Result = null;

			foreach (MeteringNode Node in await Database.Find<MeteringNode>(new FilterFieldEqualTo("ParentId", Guid.Empty)))
			{
				if (Node is Root)
				{
					if (Result == null)
						Result = (Root)Node;
					else
						await Database.Delete(Node);
				}
			}

			if (Result == null)
			{
				Result = new Root()
				{
					NodeId = await (await Translator.GetDefaultLanguageAsync()).GetStringAsync(typeof(MeteringTopology), 14, "Root")
				};

				await Database.Insert(Result);
			}

			this.root = Result;
		}

	}
}
