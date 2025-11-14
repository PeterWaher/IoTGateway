using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields.Comparisons
{
	/// <summary>
	/// Condition that is always true.
	/// </summary>
	public class Else : ConditionNode
	{
		/// <summary>
		/// Condition that is always true.
		/// </summary>
		public Else()
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
			return Language.GetStringAsync(typeof(Else), 15, "Else");
		}

		/// <summary>
		/// Checks if condition applies to field.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <returns>If the condition applies.</returns>
		public override Task<bool> AppliesTo(Field Field)
		{
			return Task.FromResult(true);
		}
	}
}
