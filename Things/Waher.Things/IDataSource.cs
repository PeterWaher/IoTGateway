using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Runtime.Language;

namespace Waher.Things
{
	/// <summary>
	/// Interface for datasources that are published through the concentrator interface.
	/// </summary>
	public interface IDataSource
	{
		/// <summary>
		/// ID of data source.
		/// </summary>
		string SourceID
		{
			get;
		}

		/// <summary>
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use. Can be null.</param>
		Task<string> GetNameAsync(Language Language);

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		bool HasChildren
		{
			get;
		}

		/// <summary>
		/// When the source was last updated.
		/// </summary>
		DateTime LastChanged
		{
			get;
		}

		/// <summary>
		/// Child sources. If no child sources are available, null is returned.
		/// </summary>
		IEnumerable<IDataSource> ChildSources
		{
			get;
		}

		/// <summary>
		/// If the data source is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the source is visible to the caller.</returns>
		Task<bool> CanViewAsync(RequestOrigin Caller);

		/// <summary>
		/// Root node references. If no root nodes are available, null is returned.
		/// </summary>
		IEnumerable<INode> RootNodes
		{
			get;
		}

		/// <summary>
		/// Gets the node, given a reference to it.
		/// </summary>
		/// <param name="NodeRef">Node reference.</param>
		/// <returns>Node, if found, null otherwise.</returns>
		Task<INode> GetNodeAsync(IThingReference NodeRef);

	}
}
