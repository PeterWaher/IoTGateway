using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Events;

namespace Waher.Things.ControlParameters
{
	/// <summary>
	/// Set handler delegate for string control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being set.</param>
	/// <param name="Value">Value set.</param>
	public delegate Task StringSetHandler(IThingReference Node, string Value);

	/// <summary>
	/// Get handler delegate for string control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being retrieved.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate Task<string> StringGetHandler(IThingReference Node);

	/// <summary>
	/// String control parameter.
	/// </summary>
	public class StringControlParameter : ControlParameter
	{
		private readonly StringGetHandler getHandler;
		private readonly StringSetHandler setHandler;
		private readonly Regex regex;
		private readonly string regexString;
		private readonly string[] options;
		private readonly string[] labels;

		/// <summary>
		/// String control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public StringControlParameter(string Name, string Page, string Label, string Description,
			StringGetHandler GetHandler, StringSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.regexString = null;
			this.regex = null;
			this.options = null;
			this.labels = null;
		}

		/// <summary>
		/// String control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="RegularExpression">Regular expression used to validate string.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public StringControlParameter(string Name, string Page, string Label, string Description, string RegularExpression,
			StringGetHandler GetHandler, StringSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.regexString = RegularExpression;
			this.regex = new Regex(RegularExpression, RegexOptions.Compiled | RegexOptions.Singleline);
			this.options = null;
			this.labels = null;
		}

		/// <summary>
		/// String control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="Options">Options the user can choose from.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public StringControlParameter(string Name, string Page, string Label, string Description, string[] Options,
			StringGetHandler GetHandler, StringSetHandler SetHandler)
			: this(Name, Page, Label, Description, Options, null, GetHandler, SetHandler)
		{
		}

		/// <summary>
		/// String control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="Options">Options the user can choose from.</param>
		/// <param name="Labels">Labels for the corresponding options.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public StringControlParameter(string Name, string Page, string Label, string Description, string[] Options, string[] Labels,
			StringGetHandler GetHandler, StringSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.regexString = null;
			this.regex = null;
			this.options = Options;
			this.labels = Labels;
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

				if (this.options != null && Array.IndexOf<string>(this.options, Value) < 0)
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

			if (this.options != null && Array.IndexOf<string>(this.options, StringValue) < 0)
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
				if (this.options is null)
					return "text-single";
				else
					return "list-single";
			}
		}

		/// <summary>
		/// Exports form validation rules for the parameter.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public override Task ExportValidationRules(XmlWriter Output, IThingReference Node)
		{
			if (!(this.options is null))
			{
				int i = 0;
				int c = this.labels is null ? 0 : this.labels.Length;

				foreach (string Option in this.options)
				{
					Output.WriteStartElement("option");

					if (i < c)
						Output.WriteAttributeString("label", this.labels[i++]);
					else
						Output.WriteAttributeString("label", Option);

					Output.WriteElementString("value", Option);
					Output.WriteEndElement();
				}
			}
			else
			{
				Output.WriteStartElement("xdv", "validate", null);
				Output.WriteAttributeString("datatype", "xs:string");

				if (!string.IsNullOrEmpty(this.regexString))
					Output.WriteElementString("xdv", "regex", null, this.regexString);

				Output.WriteEndElement();
			}

			return Task.CompletedTask;
		}
	}
}
