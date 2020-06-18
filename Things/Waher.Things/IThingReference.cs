using System;
using System.Threading.Tasks;

namespace Waher.Things
{
	/// <summary>
	/// Delegate for methods returning thing reference nodes.
	/// </summary>
	/// <param name="NodeId">Node ID</param>
	/// <param name="SourceId">Source ID</param>
	/// <param name="Partition">Partition</param>
	/// <returns>Thing reference, if found, null if not.</returns>
	public delegate Task<IThingReference> GetThingReferenceMethod(string NodeId, string SourceId, string Partition);

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
		/// Optional partition in which the Node ID is unique.
		/// </summary>
		string Partition
		{
			get;
		}
	}
}
