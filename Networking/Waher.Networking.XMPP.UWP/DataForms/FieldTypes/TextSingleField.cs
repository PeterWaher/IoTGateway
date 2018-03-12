using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;

namespace Waher.Networking.XMPP.DataForms.FieldTypes
{
	/// <summary>
	/// TextSingle form field.
	/// </summary>
	public class TextSingleField : Field
	{
		/// <summary>
		/// TextSingle form field.
		/// </summary>
		/// <param name="Form">Form containing the field.</param>
		/// <param name="Var">Variable name</param>
		/// <param name="Label">Label</param>
		/// <param name="Required">If the field is required.</param>
		/// <param name="ValueStrings">Values for the field (string representations).</param>
		/// <param name="Options">Options, as (Label,Value) pairs.</param>
		/// <param name="Description">Description</param>
		/// <param name="DataType">Data Type</param>
		/// <param name="ValidationMethod">Validation Method</param>
		/// <param name="Error">Flags the field as having an error.</param>
		/// <param name="PostBack">Flags a field as requiring server post-back after having been edited.</param>
		/// <param name="ReadOnly">Flags a field as being read-only.</param>
		/// <param name="NotSame">Flags a field as having an undefined or uncertain value.</param>
		public TextSingleField(DataForm Form, string Var, string Label, bool Required, string[] ValueStrings, KeyValuePair<string, string>[] Options, string Description,
			DataType DataType, ValidationMethod ValidationMethod, string Error, bool PostBack, bool ReadOnly, bool NotSame)
			: base(Form, Var, Label, Required, ValueStrings, Options, Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame)
		{
		}

		/// <summary>
		/// TextSingle form field.
		/// </summary>
		/// <param name="Var">Variable name</param>
		/// <param name="Value">Value for the field (string representations).</param>
		public TextSingleField(string Var, string Value)
			: base(null, Var, string.Empty, false, new string[] { Value }, null, string.Empty, null, null,
				  string.Empty, false, false, false)
		{
		}

		/// <summary>
		/// TextSingle form field.
		/// </summary>
		/// <param name="Var">Variable name</param>
		/// <param name="Label">Label</param>
		/// <param name="Value">Value for the field (string representations).</param>
		public TextSingleField(string Var, string Label, string Value)
			: base(null, Var, Label, false, new string[] { Value }, null, string.Empty, null, null,
				  string.Empty, false, false, false)
		{
		}

		/// <summary>
		/// <see cref="Field.TypeName"/>
		/// </summary>
		public override string TypeName
		{
			get { return "text-single"; }
		}

		/// <summary>
		/// Validates field input. The <see cref="Field.Error"/> property will reflect any errors found.
		/// </summary>
		/// <param name="Value">Field Value(s)</param>
		public override void Validate(params string[] Value)
		{
			base.Validate(Value);

			if (!this.HasError && Value != null && Value.Length > 1)
				this.Error = "Only one value allowed.";
		}
	}
}
