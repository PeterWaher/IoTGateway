using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things;

namespace Waher.Jobs.NodeTypes
{
	/// <summary>
	/// Class for the root node of the Jobs data source.
	/// </summary>
	public class Root : JobNode
	{
		/// <summary>
		/// Class for the root node of the Jobs data source.
		/// </summary>
		public Root()
			: base()
		{
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Root), 1, "Root");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node can be edited by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be edited by the caller.</returns>
		public override Task<bool> CanEditAsync(RequestOrigin Caller)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node can be destroyed to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be destroyed to by the caller.</returns>
		public override Task<bool> CanDestroyAsync(RequestOrigin Caller)
        {
            return Task.FromResult(false);
        }
    }
}
