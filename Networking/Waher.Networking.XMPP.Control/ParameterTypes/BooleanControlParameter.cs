using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Events;
using Waher.Things;
using Waher.Content;

namespace Waher.Networking.XMPP.Control.ParameterTypes
{
	/// <summary>
	/// Set handler delegate for boolean control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Value">Value set.</param>
	public delegate void BooleanSetHandler(ThingReference Node, bool Value);

	/// <summary>
	/// Get handler delegate for boolean control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate bool? BooleanGetHandler(ThingReference Node);

	/// <summary>
	/// Boolean control parameter.
	/// </summary>
	public class BooleanControlParameter : ControlParameter
	{
		private BooleanGetHandler getHandler;
		private BooleanSetHandler setHandler;

		/// <summary>
		/// Boolean control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public BooleanControlParameter(string Name, string Page, string Label, string Description, BooleanGetHandler GetHandler, BooleanSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
		}

		/// <summary>
		/// Sets the value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <param name="Value">Value to set.</param>
		/// <returns>If the parameter could be set (true), or if the value was invalid (false).</returns>
		public bool Set(ThingReference Node, bool Value)
		{
			try
			{
				this.setHandler(Node, Value);
				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		/// <summary>
		/// Sets the value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <param name="StringValue">String representation of value to set.</param>
		/// <returns>If the parameter could be set (true), or if the value could not be parsed or its value was invalid (false).</returns>
		public override bool SetStringValue(ThingReference Node, string StringValue)
		{
			bool Value;

			if (!CommonTypes.TryParse(StringValue, out Value))
				return false;

			this.Set(Node, Value);

			return true;
		}

		/// <summary>
		/// Gets the value of the control parameter.
		/// </summary>
		/// <returns>Current value, or null if not available.</returns>
		public bool? Get(ThingReference Node)
		{
			try
			{
				return this.getHandler(Node);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return null;
			}
		}

		/// <summary>
		/// Gets the string value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <returns>String representation of the value.</returns>
		public override string GetStringValue(ThingReference Node)
		{
			bool? Value = this.Get(Node);

			if (Value.HasValue)
				return CommonTypes.Encode(Value.Value);
			else
				return string.Empty;
		}

		/// <summary>
		/// Data form field type.
		/// </summary>
		public override string FormFieldType
		{
			get { return "boolean"; }
		}

		/// <summary>
		/// Exports form validation rules for the parameter.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public override void ExportValidationRules(XmlWriter Output, ThingReference Node)
		{
			Output.WriteStartElement("xdv", "validate", null);
			Output.WriteAttributeString("datatype", "xs:boolean");
			Output.WriteEndElement();
		}
	}
}
