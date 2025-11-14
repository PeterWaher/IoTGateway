using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields.Comparisons
{
	/// <summary>
	/// Condition on field type.
	/// </summary>
	public class IfType : ConditionNode
	{
		/// <summary>
		/// Condition on field type.
		/// </summary>
		public IfType()
			: base()
		{
		}

		/// <summary>
		/// If momentary values should be read.
		/// </summary>
		[Header(3, "Momentary values.", 30)]
		[Page(21, "Processor", 0)]
		[ToolTip(4, "Check, if momentary values should be read.")]
		[DefaultValue(true)]
		public bool Momentary { get; set; } = true;

		/// <summary>
		/// If identity values should be read.
		/// </summary>
		[Header(5, "Identity values.", 40)]
		[Page(21, "Processor", 0)]
		[ToolTip(6, "Check, if identity values should be read.")]
		[DefaultValue(false)]
		public bool Identity { get; set; } = false;

		/// <summary>
		/// If status values should be read.
		/// </summary>
		[Header(7, "Status values.", 50)]
		[Page(21, "Processor", 0)]
		[ToolTip(8, "Check, if status values should be read.")]
		[DefaultValue(false)]
		public bool Status { get; set; } = false;

		/// <summary>
		/// If computed values should be read.
		/// </summary>
		[Header(9, "Computed values.", 60)]
		[Page(21, "Processor", 0)]
		[ToolTip(10, "Check, if computed values should be read.")]
		[DefaultValue(false)]
		public bool Computed { get; set; } = false;

		/// <summary>
		/// If peak values should be read.
		/// </summary>
		[Header(11, "Peak values.", 70)]
		[Page(21, "Processor", 0)]
		[ToolTip(12, "Check, if peak values should be read.")]
		[DefaultValue(false)]
		public bool Peak { get; set; } = false;

		/// <summary>
		/// If historical values should be read.
		/// </summary>
		[Header(13, "Historical values.", 80)]
		[Page(21, "Processor", 0)]
		[ToolTip(14, "Check, if historical values should be read.")]
		[DefaultValue(false)]
		public bool Historical { get; set; } = false;

		/// <summary>
		/// Types of fields to read.
		/// </summary>
		public FieldType FieldTypes
		{
			get
			{
				FieldType Result = 0;

				if (this.Momentary)
					Result |= FieldType.Momentary;

				if (this.Identity)
					Result |= FieldType.Identity;

				if (this.Status)
					Result |= FieldType.Status;

				if (this.Computed)
					Result |= FieldType.Computed;

				if (this.Peak)
					Result |= FieldType.Peak;

				if (this.Historical)
					Result |= FieldType.Historical;

				return Result;
			}
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(IfType), 2, "If Field Type");
		}

		/// <summary>
		/// Checks if condition applies to field.
		/// </summary>
		/// <param name="Field">Field</param>
		/// <returns>If the condition applies.</returns>
		public override Task<bool> AppliesTo(Field Field)
		{
			int i = (int)this.FieldTypes;
			return Task.FromResult(((int)Field.Type & i) == i);
		}
	}
}
