using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Base class for form fields
	/// </summary>
	public abstract class Field
	{
		private string var;
		private string label;
		private bool required;
		private string[] valueStrings;
		private KeyValuePair<string, string>[] options;
		private string description;
		private DataType dataType;
		private ValidationMethod validationMethod;

		/// <summary>
		/// Base class for form fields
		/// </summary>
		/// <param name="Var">Variable name</param>
		/// <param name="Label">Label</param>
		/// <param name="Required">If the field is required.</param>
		/// <param name="ValueStrings">Values for the field (string representations).</param>
		/// <param name="Options">Options, as (Label,Value) pairs.</param>
		/// <param name="Description">Description</param>
		/// <param name="DataType">Data Type</param>
		/// <param name="ValidationMethod">Validation Method</param>
		public Field(string Var, string Label, bool Required, string[] ValueStrings, KeyValuePair<string, string>[] Options, string Description,
			DataType DataType, ValidationMethod ValidationMethod)
		{
			this.var = Var;
			this.label = Label;
			this.required = Required;
			this.valueStrings = ValueStrings;
			this.options = Options;
			this.description = Description;
			this.dataType = DataType;
			this.validationMethod = ValidationMethod;
		}

		/// <summary>
		/// Variable name
		/// </summary>
		public string Var { get { return this.var; } }

		/// <summary>
		/// Label
		/// </summary>
		public string Label { get { return this.label; } }

		/// <summary>
		/// If the field is required.
		/// </summary>
		public bool Required { get { return this.required; } }

		/// <summary>
		/// Values for the field (string representations).
		/// </summary>
		public string[] ValueStrings { get { return this.valueStrings; } }

		/// <summary>
		/// Value as a single string. If field contains multiple values, they will be concatenated into a single string, each one delimited by a CRLF.
		/// </summary>
		public string ValueString
		{
			get
			{
				StringBuilder sb = null;

				foreach (string s in this.valueStrings)
				{
					if (sb == null)
						sb = new StringBuilder(s);
					else
					{
						sb.AppendLine();
						sb.Append(s);
					}
				}

				if (sb == null)
					return string.Empty;
				else
					return sb.ToString();
			}
		}

		/// <summary>
		/// Options, as (Label,Value) pairs.
		/// </summary>
		public KeyValuePair<string, string>[] Options { get { return this.options; } }

		/// <summary>
		/// Description
		/// </summary>
		public string Description { get { return this.description; } }

		/// <summary>
		/// Data Type
		/// </summary>
		public DataType DataType { get { return this.dataType; } }

		/// <summary>
		/// Validation Method
		/// </summary>
		public ValidationMethod ValidationMethod { get { return this.validationMethod; } }

		/// <summary>
		/// Sets the value, or values, of the field.
		/// 
		/// The values are not validated.
		/// </summary>
		/// <param name="Validate">If the values are to be validated according to validation rules specified in the form.</param>
		/// <param name="Value">Value(s).</param>
		public void SetValue(bool Validate, params string[] Value)
		{
			if (Validate)
			{
				if (this.dataType != null)
				{
					List<object> Parsed = new List<object>();

					foreach (string s in Value)
					{
						object Obj = this.dataType.Parse(s);
						if (Obj == null)
							throw new ArgumentException("Invalid input.", this.var);

						Parsed.Add(Obj);
					}

					if (this.validationMethod != null)
						this.validationMethod.Validate(this, this.dataType, Parsed.ToArray(), Value);
				}
			}

			this.valueStrings = Value;
		}

		internal void Serialize(StringBuilder Output, bool ValuesOnly)
		{
			Output.Append("<field var='");
			Output.Append(XmppClient.XmlEncode(this.var));

			if (!ValuesOnly)
			{
				if (!string.IsNullOrEmpty(this.label))
				{
					Output.Append("' label='");
					Output.Append(XmppClient.XmlEncode(this.label));
				}

				Output.Append("' type='");
				Output.Append(this.TypeName);
			}

			Output.Append("'>");

			if (!ValuesOnly)
			{
				if (!string.IsNullOrEmpty(this.description))
				{
					Output.Append("<desc>");
					Output.Append(XmppClient.XmlEncode(this.description));
					Output.Append("</desc>");
				}

				if (this.required)
					Output.Append("<required/>");

				if (this.dataType != null)
				{
					Output.Append("<validate xmlns='http://jabber.org/protocol/xdata-validate' datatype='");
					Output.Append(XmppClient.XmlEncode(this.dataType.TypeName));
					Output.Append("'>");

					if (this.validationMethod != null)
						this.validationMethod.Serialize(Output);

					Output.Append("</validate>");
				}
			}

			if (this.valueStrings != null)
			{
				foreach (string Value in this.valueStrings)
				{
					Output.Append("<value>");
					Output.Append(XmppClient.XmlEncode(Value));
					Output.Append("</value>");
				}
			}
			else if (ValuesOnly)
				Output.Append("<value/>");

			if (!ValuesOnly)
			{
				if (this.options != null)
				{
					foreach (KeyValuePair<string, string> P in this.options)
					{
						Output.Append("<option label='");
						Output.Append(XmppClient.XmlEncode(P.Key));
						Output.Append("'>");
						Output.Append(XmppClient.XmlEncode(P.Value));
						Output.Append("</option>");
					}
				}
			}

			Output.Append("</field>");
		}

		/// <summary>
		/// XMPP Field Type Name.
		/// </summary>
		public abstract string TypeName
		{
			get;
		}

	}
}
