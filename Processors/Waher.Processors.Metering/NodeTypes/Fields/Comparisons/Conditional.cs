using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields.Comparisons
{
	/// <summary>
	/// Conditional processing statement.
	/// </summary>
	public class Conditional : DecisionTreeStatement
	{
		/// <summary>
		/// Conditional processing statement.
		/// </summary>
		public Conditional()
			: base()
		{
		}

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		public override bool ChildrenOrdered => true;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Conditional), 2, "Conditional Processing");
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is IConditionNode);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is IDecisionTreeStatements);
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override async Task<Field[]> ProcessField(ISensor Sensor, Field Field)
		{
			foreach (INode Node in await this.ChildNodes)
			{
				if (Node is IConditionNode ConditionNode)
				{
					if (await ConditionNode.AppliesTo(Field))
						return await ConditionNode.ProcessField(Sensor, Field);
				}
			}

			return null;
		}
	}
}
