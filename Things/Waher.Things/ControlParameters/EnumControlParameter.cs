using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Events;
using Waher.Things;

namespace Waher.Things.ControlParameters
{
	/// <summary>
	/// Set handler delegate for enumeration control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being set.</param>
	/// <param name="Value">Value set.</param>
	public delegate void EnumSetHandler(IThingReference Node, Enum Value);

	/// <summary>
	/// Get handler delegate for enumeration control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being retrieved.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate Enum EnumGetHandler(IThingReference Node);

	/// <summary>
	/// Enumeration control parameter.
	/// </summary>
	public class EnumControlParameter : ControlParameter
	{
		private EnumGetHandler getHandler;
		private EnumSetHandler setHandler;
		private Type enumType;
		private string[] labels;

		/// <summary>
		/// String control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="EnumType">Enumeration type.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		/// <param name="Labels">Labels for the enumeration values.</param>
		public EnumControlParameter(string Name, string Page, string Label, string Description, Type EnumType,
			EnumGetHandler GetHandler, EnumSetHandler SetHandler, params string[] Labels)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.enumType = EnumType;
			this.labels = Labels;
		}

		/// <summary>
		/// Sets the value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <param name="Value">Value to set.</param>
		/// <returns>If the parameter could be set (true), or if the value was invalid (false).</returns>
		public bool Set(IThingReference Node, Enum Value)
		{
			try
			{
				if (Value.GetType() != this.enumType)
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
		public override bool SetStringValue(IThingReference Node, string StringValue)
		{
			Enum Value;

			try
			{
				Value = (Enum)Enum.Parse(this.enumType, StringValue);
			}
			catch (Exception)
			{
				return false;
			}

			try
			{
				this.setHandler(Node, Value);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets the value of the control parameter.
		/// </summary>
		/// <returns>Current value, or null if not available.</returns>
		public Enum Get(IThingReference Node)
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
		public override string GetStringValue(IThingReference Node)
		{
			return this.Get(Node).ToString();
		}

		/// <summary>
		/// Data form field type.
		/// </summary>
		public override string FormFieldType
		{
			get { return "list-single"; }
		}

		/// <summary>
		/// Exports form validation rules for the parameter.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public override void ExportValidationRules(XmlWriter Output, IThingReference Node)
		{
			int i = 0;
			int c = this.labels.Length;

			foreach (Enum Option in Enum.GetValues(this.enumType))
			{
				Output.WriteStartElement("option");

				if (i < c)
					Output.WriteAttributeString("label", this.labels[i++]);
				else
					Output.WriteAttributeString("label", Option.ToString());

				Output.WriteElementString("value", Option.ToString());
				Output.WriteEndElement();
			}
		}
	}
}
