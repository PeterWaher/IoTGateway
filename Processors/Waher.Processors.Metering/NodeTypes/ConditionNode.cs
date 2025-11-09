using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes
{
	/// <summary>
	/// Abstract base class of condition nodes.
	/// </summary>
	public abstract class ConditionNode : DecisionTreeStatements, IConditionNode
	{
		/// <summary>
		/// Condition on field type.
		/// </summary>
		public ConditionNode()
			: base()
		{
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Conditional);
		}

		/// <summary>
		/// Checks if condition applies to field.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <returns>If the condition applies.</returns>
		public abstract Task<bool> AppliesTo(Field Field);
	}
}
