using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Concentrator
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
		/// Name of data source.
		/// </summary>
		string Name
		{
			get;
		}

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
		/// Root node references. If no root nodes are available, null is returned.
		/// </summary>
		IEnumerable<ThingReference> RootNodes
		{
			get;
		}

		/// <summary>
		/// If the source contains a node.
		/// </summary>
		/// <param name="Node">Node reference.</param>
		/// <returns>If the source contains the node.</returns>
		bool ContainsNode(ThingReference Node);

	}
}
