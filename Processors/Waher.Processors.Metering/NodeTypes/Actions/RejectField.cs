using System.Threading.Tasks;
using Waher.Processors.Metering.NodeTypes.Comparisons;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Actions
{
	/// <summary>
	/// Rejects a field.
	/// </summary>
	public class RejectField : DecisionTreeLeafStatement
	{
		/// <summary>
		/// Rejects a field.
		/// </summary>
		public RejectField()
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
			return Language.GetStringAsync(typeof(Conditional), 19, "Reject Field");
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override Task<Field[]> ProcessField(ISensor Sensor, Field Field)
		{
			return Task.FromResult<Field[]>(null);
		}
	}
}
