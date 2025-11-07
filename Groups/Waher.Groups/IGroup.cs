using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Things;

namespace Waher.Groups
{
	/// <summary>
	/// Base Interface for all metering nodes.
	/// </summary>
	public interface IGroup : INode
	{
		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		Task<T[]> FindNodes<T>()
			where T : INode;

		/// <summary>
		/// Finds nodes referenced by the group node.
		/// </summary>
		/// <param name="Nodes">Nodes found will be added to this collection.</param>
		Task FindNodes<T>(ChunkedList<T> Nodes)
			where T : INode;
	}
}
