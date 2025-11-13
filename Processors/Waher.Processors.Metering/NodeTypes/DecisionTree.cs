using System.Threading.Tasks;
using Waher.Processors.Metering.NodeTypes.Comparisons;
using Waher.Processors.NodeTypes;
using Waher.Runtime.Language;
using Waher.Things;

namespace Waher.Processors.Metering.NodeTypes
{
	/// <summary>
	/// Processing of sensor data via a decision tree.
	/// </summary>
	public class DecisionTree : DecisionTreeStatements, ISensorDataProcessor
	{
		/// <summary>
		/// Processing of sensor data via a decision tree.
		/// </summary>
		public DecisionTree()
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
			return Language.GetStringAsync(typeof(DecisionTree), 1, "Decision Tree");
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root);
		}
	}
}