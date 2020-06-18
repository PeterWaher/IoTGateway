using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Events;

namespace Waher.Things.ControlParameters
{
	/// <summary>
	/// Multi-line text control parameter.
	/// </summary>
	public class MultiLineTextControlParameter : ControlParameter
	{
		private readonly StringGetHandler getHandler;
		private readonly StringSetHandler setHandler;
		private readonly Regex regex;
		private readonly string regexString;

		/// <summary>
		/// Multi-line text control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public MultiLineTextControlParameter(string Name, string Page, string Label, string Description,
			StringGetHandler GetHandler, StringSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.regexString = null;
			this.regex = null;
		}

		/// <summary>
		/// Multi-line text control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="RegularExpression">Regular expression used to validate string.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public MultiLineTextControlParameter(string Name, string Page, string Label, string Description, string RegularExpression,
			StringGetHandler GetHandler, StringSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.regexString = RegularExpression;
			this.regex = new Regex(RegularExpression, RegexOptions.Compiled | RegexOptions.Singleline);
		}

		/// <summary>
		/// Sets the value of the control parameter.
		/// </summary>
		/// <param name="Node">Node reference, if available.</param>
		/// <param name="Value">Value to set.</param>
		/// <returns>If the parameter could be set (true), or if the value was invalid (false).</returns>
		public async Task<bool> Set(IThingReference Node, string Value)
		{
			try
			{
				if (this.regex != null && !this.regex.IsMatch(Value))
					return false;

				await this.setHandler(Node, Value);
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
		public override async Task<bool> SetStringValue(IThingReference Node, string StringValue)
		{
			if (this.regex != null && !this.regex.IsMatch(StringValue))
				return false;

			try
			{
				await this.setHandler(Node, StringValue);
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
		public async Task<string> Get(IThingReference Node)
		{
			try
			{
				return await this.getHandler(Node);
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
		public override Task<string> GetStringValue(IThingReference Node)
		{
			return this.Get(Node);
		}

		/// <summary>
		/// Data form field type.
		/// </summary>
		public override string FormFieldType
		{
			get
			{
				return "text-multi";
			}
		}

		/// <summary>
		/// Exports form validation rules for the parameter.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public override Task ExportValidationRules(XmlWriter Output, IThingReference Node)
		{
			Output.WriteStartElement("xdv", "validate", null);
			Output.WriteAttributeString("datatype", "xs:string");

			if (!string.IsNullOrEmpty(this.regexString))
				Output.WriteElementString("xdv", "regex", null, this.regexString);

			Output.WriteEndElement();

			return Task.CompletedTask;
		}
	}
}
