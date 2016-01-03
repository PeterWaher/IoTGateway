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
		private DataForm form;
		private string var;
		private string label;
		private bool required;
		private string[] valueStrings;
		private KeyValuePair<string, string>[] options;
		private string description;
		private DataType dataType;
		private ValidationMethod validationMethod;
		private string error;
		private bool postBack;
		private bool readOnly;
		private bool notSame;
		private bool edited = false;

		/// <summary>
		/// Base class for form fields
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
		public Field(DataForm Form, string Var, string Label, bool Required, string[] ValueStrings, KeyValuePair<string, string>[] Options,
			string Description, DataType DataType, ValidationMethod ValidationMethod, string Error, bool PostBack, bool ReadOnly, bool NotSame)
		{
			this.form = Form;
			this.var = Var;
			this.label = Label;
			this.required = Required;
			this.valueStrings = ValueStrings;
			this.options = Options;
			this.description = Description;
			this.dataType = DataType;
			this.validationMethod = ValidationMethod;
			this.error = Error;
			this.postBack = PostBack;
			this.readOnly = ReadOnly;
			this.notSame = NotSame;
		}

		/// <summary>
		/// Data Form containing the field.
		/// </summary>
		public DataForm Form { get { return this.form; } }

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
				return XmppClient.Concat(this.valueStrings);
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
		/// If not null, flags the field as having an error.
		/// </summary>
		public string Error 
		{
			get { return this.error; }
			internal set { this.error = value; } 
		}

		/// <summary>
		/// If the field has an error. Any error message is available in the <see cref="Error"/> property.
		/// </summary>
		public bool HasError { get { return !string.IsNullOrEmpty(this.error); } }

		/// <summary>
		/// Flags the field as requiring server post-back after having been edited.
		/// </summary>
		public bool PostBack { get { return this.postBack; } }

		/// <summary>
		/// Flags the field as being read-only.
		/// </summary>
		public bool ReadOnly { get { return this.readOnly; } }

		/// <summary>
		/// Flags the field as having an undefined or uncertain value.
		/// </summary>
		public bool NotSame { get { return this.notSame; } }

		/// <summary>
		/// If the field has been edited.
		/// </summary>
		public bool Edited { get { return this.edited; } }

		/// <summary>
		/// Sets the value, or values, of the field.
		/// 
		/// The values are not validated.
		/// </summary>
		/// <param name="Value">Value(s).</param>
		public void SetValue(params string[] Value)
		{
			this.error = string.Empty;

			if (this.dataType != null)
			{
				List<object> Parsed = new List<object>();

				foreach (string s in Value)
				{
					object Obj = this.dataType.Parse(s);
					if (Obj == null)
						this.error = "Invalid input.";
					else
						Parsed.Add(Obj);
				}

				if (this.validationMethod != null)
					this.validationMethod.Validate(this, this.dataType, Parsed.ToArray(), Value);
			}

			this.edited = true;
			this.valueStrings = Value;

			if (this.postBack && string.IsNullOrEmpty(this.error))
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<submit xmlns='urn:xmpp:xdata:dynamic'>");
				this.form.Serialize(Xml, "submit", true);
				Xml.Append("</submit>");

				this.form.Client.SendIqSet(this.form.From, Xml.ToString(), this.FormUpdated, null);
			}
		}

		private void FormUpdated(object Sender, IqResultEventArgs e)
		{
			XmppClient Client = Sender as XmppClient;
			if (Client != null && e.Ok)
			{
				foreach (XmlNode N in e.Response)
				{
					if (N.LocalName == "x")
					{
						DataForm UpdatedForm = new DataForm(Client, (XmlElement)N, null, null, e.From, e.To);
						this.form.Join(UpdatedForm);
					}
				}
			}
		}

		internal void Serialize(StringBuilder Output, bool ValuesOnly)
		{
			string TypeName = this.TypeName;

			if (TypeName != "fixed")
			{
				Output.Append("<field var='");
				Output.Append(XML.Encode(this.var));

				if (!ValuesOnly)
				{
					if (!string.IsNullOrEmpty(this.label))
					{
						Output.Append("' label='");
						Output.Append(XML.Encode(this.label));
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
						Output.Append(XML.Encode(this.description));
						Output.Append("</desc>");
					}

					if (this.required)
						Output.Append("<required/>");

					if (this.dataType != null)
					{
						Output.Append("<validate xmlns='http://jabber.org/protocol/xdata-validate' datatype='");
						Output.Append(XML.Encode(this.dataType.TypeName));
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
						Output.Append(XML.Encode(Value));
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
							Output.Append(XML.Encode(P.Key));
							Output.Append("'>");
							Output.Append(XML.Encode(P.Value));
							Output.Append("</option>");
						}
					}
				}

				Output.Append("</field>");
			}
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
