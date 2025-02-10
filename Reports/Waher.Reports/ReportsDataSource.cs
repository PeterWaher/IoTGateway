﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.SourceEvents;

namespace Waher.Reports
{
	/// <summary>
	/// Defines the Reports data source. This data source contains a tree structure of 
	/// reports published by different modules running in memory.
	/// </summary>
	public class ReportsDataSource : IDataSource
	{
		/// <summary>
		/// Source ID for the reports data source.
		/// </summary>
		public const string SourceID = "Reports";

		private static readonly Dictionary<string, ReportNode> nodes = new Dictionary<string, ReportNode>();
		private static ReportNode[] roots = null;
		private static ReportsDataSource instance = null;
		private static DateTime lastChanged = DateTime.UtcNow;

		internal static ReportsDataSource Instance => instance;

		/// <summary>
		/// Defines the Reports data source. This data source contains a tree structure of 
		/// reports published by different modules running in memory.
		/// </summary>
		public ReportsDataSource()
		{
			if (instance is null)
				instance = this;
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
		public DateTime LastChanged => lastChanged;

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized name of data source.</returns>
		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ReportsDataSource), 1, "Reports");
		}

		/// <summary>
		/// If the data source is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the source is visible to the caller.</returns>
		public Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult(true);     // TODO: Check user privileges
		}

		/// <summary>
		/// Root node references. If no root nodes are available, null is returned.
		/// </summary>
		public IEnumerable<INode> RootNodes => (ReportNode[])roots.Clone();

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
		/// Gets the node, given a reference to it.
		/// </summary>
		/// <param name="NodeRef">Node reference.</param>
		/// <returns>Node, if found, null otherwise.</returns>
		public Task<INode> GetNodeAsync(IThingReference NodeRef)
		{
			if (NodeRef.SourceId != SourceID || !string.IsNullOrEmpty(NodeRef.Partition))
				return Task.FromResult<INode>(null);

			if (nodes.TryGetValue(NodeRef.NodeId, out ReportNode Result))
				return Task.FromResult<INode>(Result);
			else
				return Task.FromResult<INode>(null);
		}

	}
}
