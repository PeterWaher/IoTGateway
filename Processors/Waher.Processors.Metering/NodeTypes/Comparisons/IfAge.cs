using System;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Comparisons
{
	/// <summary>
	/// Condition on field age.
	/// </summary>
	public class IfAge : ComparisonNode
	{
		/// <summary>
		/// Condition on field age.
		/// </summary>
		public IfAge()
			: base()
		{
		}

		/// <summary>
		/// If historical values should be read.
		/// </summary>
		[Header(26, "Age Limit:", 0)]
		[Page(4, "Condition", 100)]
		[ToolTip(27, "Compares the field age with this value.")]
		public Duration AgeLimit { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Conditional), 28, "If Field Age");
		}

		/// <summary>
		/// Checks if condition applies to field.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <returns>If the condition applies.</returns>
		public override Task<bool> AppliesTo(Field Field)
		{
			return this.CompareTo(Field.Timestamp.ToUniversalTime(), DateTime.UtcNow - this.AgeLimit);
		}
	}
}
