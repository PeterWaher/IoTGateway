using System;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Places the parameter in a localizable section.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class SectionAttribute : Attribute
	{
		private readonly int stringId;
		private readonly string label;

		/// <summary>
		/// Places the parameter in a section.
		/// </summary>
		/// <param name="Label">Label string</param>
		public SectionAttribute(string Label)
			: this(0, Label)
		{
		}

		/// <summary>
		/// Places the parameter in a localizable section.
		/// </summary>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Label">Default label string, in the default language defined for the class.</param>
		public SectionAttribute(int StringId, string Label)
		{
			this.stringId = StringId;
			this.label = Label;
		}

		/// <summary>
		/// String ID in the namespace of the current class, in the default language defined for the class.
		/// </summary>
		public int StringId
		{
			get { return this.stringId; }
		}

		/// <summary>
		/// Default label string, in the default language defined for the class.
		/// </summary>
		public string Label
		{
			get { return this.label; }
		}
	}
}
