using System.Threading.Tasks;
using Waher.Groups.NodeTypes;
using Waher.Runtime.Language;
using Waher.Things;

namespace Waher.Groups.Metering.NodeTypes
{
	/// <summary>
	/// Represents a group of metering nodes.
	/// </summary>
	public class MeteringGroup : Group
	{
		/// <summary>
		/// Represents a group of metering nodes.
		/// </summary>
		public MeteringGroup()
		{
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MeteringGroup), 1, "Metering Group");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(
				Child is MeteringNodeReference || 
				Child is MeteringGroup);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is MeteringGroup);
		}
	}
}
