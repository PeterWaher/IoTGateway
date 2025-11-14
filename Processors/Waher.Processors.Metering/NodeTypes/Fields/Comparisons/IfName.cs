using System;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields.Comparisons
{
	/// <summary>
	/// Condition on field name.
	/// </summary>
	public class IfName : ConditionNode
	{
		/// <summary>
		/// Condition on field name.
		/// </summary>
		public IfName()
			: base()
		{
		}

		/// <summary>
		/// If historical values should be read.
		/// </summary>
		[Header(18, "Field Names:", 30)]
		[Page(21, "Processor", 0)]
		[ToolTip(19, "Check, if field name matches any in the list.")]
		public string[] FieldNames { get; set; }

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(IfName), 20, "If Field Name");
		}

		/// <summary>
		/// Checks if condition applies to field.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <returns>If the condition applies.</returns>
		public override Task<bool> AppliesTo(Field Field)
		{
			if (this.FieldNames is null)
				return Task.FromResult(false);
			else
				return Task.FromResult(Array.IndexOf(this.FieldNames, Field.Name) >= 0);
		}
	}
}
