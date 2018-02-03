using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Things.Attributes
{
	/// <summary>
	/// Where the instructions are to be place.
	/// </summary>
	public enum TextPosition
	{
		/// <summary>
		/// Before the field.
		/// </summary>
		BeforeField,

		/// <summary>
		/// After the field.
		/// </summary>
		AfterField
	}

	/// <summary>
	/// Shows a text segment associated with the parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class TextAttribute : Attribute
	{
		private TextPosition position;
		private int stringId;
		private string label;

		/// <summary>
		/// Shows a text segment associated with the parameter.
		/// </summary>
		/// <param name="Position">Position of text in relation to the field.</param>
		/// <param name="Label">Label string</param>
		public TextAttribute(TextPosition Position, string Label)
			: this(Position, 0, Label)
		{
		}

		/// <summary>
		/// Shows a text segment associated with the parameter.
		/// </summary>
		/// <param name="Position">Position of text in relation to the field.</param>
		/// <param name="StringId">String ID in the namespace of the current class, in the default language defined for the class.</param>
		/// <param name="Label">Default label string, in the default language defined for the class.</param>
		public TextAttribute(TextPosition Position, int StringId, string Label)
		{
			this.position = Position;
			this.stringId = StringId;
			this.label = Label;
		}

		/// <summary>
		/// Where the text is to be placed in relation to the field.
		/// </summary>
		public TextPosition Position
		{
			get { return this.position; }
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
