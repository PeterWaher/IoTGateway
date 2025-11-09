using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes
{
	/// <summary>
	/// Abstract base class for decision tree nodes containing its own set of statements.
	/// </summary>
	public abstract class DecisionTreeStatements : ProcessorNode, IDecisionTreeStatements
	{
		/// <summary>
		/// Abstract base class for decision tree nodes containing its own set of statements.
		/// </summary>
		public DecisionTreeStatements()
			: base()
		{
		}

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		public override bool ChildrenOrdered => true;

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(Child is IDecisionTreeStatement);
		}

		/// <summary>
		/// Process a collection of sensor data fields.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the fields.</param>
		/// <param name="Fields">Fields to process.</param>
		/// <returns>Processed set of fields. Can be null if no fields pass processing.</returns>
		public async Task<Field[]> ProcessFields(ISensor Sensor, Field[] Fields)
		{
			ChunkedList<Field> Step = null;

			foreach (INode Child in await this.ChildNodes)
			{
				if (!(Child is IDecisionTreeStatement DecisionTreeStatement))
					continue;

				foreach (Field Field in Fields)
				{
					Field[] Processed = await DecisionTreeStatement.ProcessField(Sensor, Field);
					if (Processed is null)
						continue;

					Step ??= new ChunkedList<Field>();
					Step.AddRange(Processed);
				}

				if (Step is null)
					return null;

				Fields = Step.ToArray();
			}

			return Fields;
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public virtual Task<Field[]> ProcessField(ISensor Sensor, Field Field)
		{
			Field[] Fields = new Field[] { Field };
			return this.ProcessFields(Sensor, Fields);
		}
	}
}
