using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Events;
using Waher.Content;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ParameterTypes
{
	/// <summary>
	/// Set handler delegate for duration control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Value">Value set.</param>
	public delegate void DurationSetHandler(ThingReference Node, Duration Value);

	/// <summary>
	/// Get handler delegate for duration control parameters.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate Duration DurationGetHandler(ThingReference Node);

	/// <summary>
	/// Duration control parameter.
	/// </summary>
	public class DurationControlParameter : ControlParameter
	{
		private DurationGetHandler getHandler;
		private DurationSetHandler setHandler;
		Duration min, max;

		/// <summary>
		/// Duration control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public DurationControlParameter(string Name, string Page, string Label, string Description, 
			DurationGetHandler GetHandler, DurationSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.min = null;
			this.max = null;
		}

		/// <summary>
		/// Duration control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="Min">Smallest value allowed.</param>
		/// <param name="Max">Largest value allowed.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public DurationControlParameter(string Name, string Page, string Label, string Description, Duration Min, Duration Max,
			DurationGetHandler GetHandler, DurationSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.min = Min;
			this.max = Max;
		}

		/// <summary>
		/// Sets the value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <param name="Value">Value to set.</param>
		/// <returns>If the parameter could be set (true), or if the value was invalid (false).</returns>
		public bool Set(ThingReference Node, Duration Value)
		{
			try
			{
				if ((this.min != null && Value < this.min) || (this.max != null && Value > this.max))
					return false;

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
			Duration Value;

			if (!Duration.TryParse(StringValue, out Value))
				return false;

			this.Set(Node, Value);

			return true;
		}

		/// <summary>
		/// Gets the value of the control parameter.
		/// </summary>
		/// <returns>Current value, or null if not available.</returns>
		public Duration Get(ThingReference Node)
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
			Duration Value = this.Get(Node);

			if (Value != null)
				return Value.ToString();
			else
				return string.Empty;
		}

		/// <summary>
		/// Exports form validation rules for the parameter.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public override void ExportValidationRules(XmlWriter Output, ThingReference Node)
		{
			Output.WriteStartElement("xdv", "validate", null);
			Output.WriteAttributeString("datatype", "xs:duration");
			Output.WriteEndElement();
		}
	}
}
