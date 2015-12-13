using System;
using System.Collections.Generic;
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
	}
}
