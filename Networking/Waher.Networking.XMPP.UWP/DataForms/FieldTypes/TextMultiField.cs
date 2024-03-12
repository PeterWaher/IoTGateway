using System.Collections.Generic;
using System.Text;
using Waher.Content.Xml;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;

namespace Waher.Networking.XMPP.DataForms.FieldTypes
{
	/// <summary>
	/// TextMulti form field.
	/// </summary>
	public class TextMultiField : Field
	{
		private readonly string contentType;

		/// <summary>
		/// TextMulti form field.
		/// </summary>
		/// <param name="Form">Form containing the field.</param>
		/// <param name="Var">Variable name</param>
		/// <param name="Label">Label</param>
		/// <param name="Required">If the field is required.</param>
		/// <param name="ValueStrings">Values for the field (string representations).</param>
		/// <param name="Options">Options, as (Key=M2H Label, Value=M2M Value) pairs.</param>
		/// <param name="Description">Description</param>
		/// <param name="DataType">Data Type</param>
		/// <param name="ValidationMethod">Validation Method</param>
		/// <param name="Error">Flags the field as having an error.</param>
		/// <param name="PostBack">Flags a field as requiring server post-back after having been edited.</param>
		/// <param name="ReadOnly">Flags a field as being read-only.</param>
		/// <param name="NotSame">Flags a field as having an undefined or uncertain value.</param>
		/// <param name="ContentType">Internet Content-Type of content of parameter.</param>
		public TextMultiField(DataForm Form, string Var, string Label, bool Required, string[] ValueStrings, KeyValuePair<string, string>[] Options, string Description,
			DataType DataType, ValidationMethod ValidationMethod, string Error, bool PostBack, bool ReadOnly, bool NotSame, string ContentType)
			: base(Form, Var, Label, Required, ValueStrings, Options, Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame)
		{
			this.contentType = ContentType;
		}


		/// <summary>
		/// TextMulti form field.
		/// </summary>
		/// <param name="Form">Form containing the field.</param>
		/// <param name="Var">Variable name</param>
		/// <param name="Label">Label</param>
		/// <param name="Required">If the field is required.</param>
		/// <param name="ValueStrings">Values for the field (string representations).</param>
		/// <param name="Options">Options, as (Key=M2H Label, Value=M2M Value) pairs.</param>
		/// <param name="Description">Description</param>
		/// <param name="DataType">Data Type</param>
		/// <param name="ValidationMethod">Validation Method</param>
		/// <param name="Error">Flags the field as having an error.</param>
		/// <param name="PostBack">Flags a field as requiring server post-back after having been edited.</param>
		/// <param name="ReadOnly">Flags a field as being read-only.</param>
		/// <param name="NotSame">Flags a field as having an undefined or uncertain value.</param>
		public TextMultiField(DataForm Form, string Var, string Label, bool Required, string[] ValueStrings, KeyValuePair<string, string>[] Options, string Description,
			DataType DataType, ValidationMethod ValidationMethod, string Error, bool PostBack, bool ReadOnly, bool NotSame)
			: base(Form, Var, Label, Required, ValueStrings, Options, Description, DataType, ValidationMethod, Error, PostBack, ReadOnly, NotSame)
		{
			this.contentType = null;
		}

		/// <summary>
		/// TextMulti form field.
		/// </summary>
		/// <param name="Var">Variable name</param>
		/// <param name="Values">Values for the field (string representations).</param>
		public TextMultiField(string Var, string[] Values)
			: base(null, Var, string.Empty, false, Values, null, string.Empty, null, null,
				  string.Empty, false, false, false)
		{
			this.contentType = null;
		}

		/// <summary>
		/// TextMulti form field.
		/// </summary>
		/// <param name="Var">Variable name</param>
		/// <param name="Label">Label</param>
		/// <param name="Values">Values for the field (string representations).</param>
		public TextMultiField(string Var, string Label, string[] Values)
			: base(null, Var, Label, false, Values, null, string.Empty, null, null,
				  string.Empty, false, false, false)
		{
			this.contentType = null;
		}

		/// <inheritdoc/>
		public override string TypeName => "text-multi";

		/// <summary>
		/// Internet Content-Type of content of parameter.
		/// </summary>
		public string ContentType => this.contentType;

		/// <summary>
		/// Allows fields to add additional information to the XML serialization of the field.
		/// </summary>
		/// <param name="Output">XML output.</param>
		/// <param name="ValuesOnly">If only values are to be output.</param>
		/// <param name="IncludeLabels">If labels are to be included.</param>
		public override void AnnotateField(StringBuilder Output, bool ValuesOnly, bool IncludeLabels)
		{
			base.AnnotateField(Output, ValuesOnly, IncludeLabels);

			if (!ValuesOnly)
			{
				Output.Append("<xdd:contentType>");
				Output.Append(XML.Encode(this.contentType));
				Output.Append("</xdd:contentType>");
			}
		}
	}
}
