using System.Threading.Tasks;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes
{
	/// <summary>
	/// Interface for condition nodes.
	/// </summary>
	public interface IConditionNode : IDecisionTreeStatement, IDecisionTreeStatements
	{
		/// <summary>
		/// Checks if condition applies to field.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <returns>If the condition applies.</returns>
		Task<bool> AppliesTo(Field Field);
	}
}
