using System.Threading.Tasks;
using Waher.Processors.Metering.NodeTypes.Comparisons;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Actions
{
	/// <summary>
	/// Accepts a field.
	/// </summary>
	public class AcceptField : DecisionTreeLeafStatement
	{
		/// <summary>
		/// Accepts a field.
		/// </summary>
		public AcceptField()
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
			return Language.GetStringAsync(typeof(Conditional), 18, "Accept Field");
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override Task<Field[]> ProcessField(ISensor Sensor, Field Field)
		{
			return Task.FromResult(new Field[] { Field });
		}
	}
}
