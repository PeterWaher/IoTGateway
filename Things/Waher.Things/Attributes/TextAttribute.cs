﻿using System;

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
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class TextAttribute : Attribute
	{
		private readonly TextPosition position;
		private readonly int stringId;
		private readonly string label;

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
		public TextPosition Position => this.position;

		/// <summary>
		/// String ID in the namespace of the current class, in the default language defined for the class.
		/// </summary>
		public int StringId => this.stringId;

		/// <summary>
		/// Default label string, in the default language defined for the class.
		/// </summary>
		public string Label => this.label;
	}
}
