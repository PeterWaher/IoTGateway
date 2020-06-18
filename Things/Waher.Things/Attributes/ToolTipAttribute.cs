using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Defines a localizable tooltip string for the property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ToolTipAttribute : Attribute
	{
		private readonly int stringId;
		private readonly string tooltip;

		/// <summary>
		/// Defines a tooltip string for the property.
		/// </summary>
		/// <param name="ToolTip">Tooltip string.</param>
		public ToolTipAttribute(string ToolTip)
			: this(0, ToolTip)
		{
		}

		/// <summary>
		/// Defines a localizable tooltip string for the property.
		/// </summary>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="ToolTip">Default tooltip string, in the default language defined for the class.</param>
		public ToolTipAttribute(int StringId, string ToolTip)
		{
			this.stringId = StringId;
			this.tooltip = ToolTip;
		}

		/// <summary>
		/// String ID in the namespace of the current class, in the default language defined for the class.
		/// </summary>
		public int StringId
		{
			get { return this.stringId; }
		}

		/// <summary>
		/// Default tooltip string, in the default language defined for the class.
		/// </summary>
		public string ToolTip
		{
			get { return this.tooltip; }
		}
	}
}
