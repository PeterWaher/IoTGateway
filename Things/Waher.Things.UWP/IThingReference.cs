using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Things
{
	/// <summary>
	/// Interface for thing references.
	/// </summary>
	public interface IThingReference
	{
		/// <summary>
		/// ID of node.
		/// </summary>
		string NodeId
		{
			get;
		}

		/// <summary>
		/// Optional ID of source containing node.
		/// </summary>
		string SourceId
		{
			get;
		}

		/// <summary>
		/// Optional Type of cache in which the Node ID is unique.
		/// </summary>
		string CacheType
		{
			get;
		}
	}
}
