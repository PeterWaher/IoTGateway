using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;
using Waher.Content;

namespace Waher.Things.ControlParameters
{
	/// <summary>
	/// Set handler delegate for boolean control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being set.</param>
	/// <param name="Value">Value set.</param>
	public delegate Task BooleanSetHandler(IThingReference Node, bool Value);

	/// <summary>
	/// Get handler delegate for boolean control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being retrieved.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate Task<bool?> BooleanGetHandler(IThingReference Node);

	/// <summary>
	/// Boolean control parameter.
	/// </summary>
	public class BooleanControlParameter : ControlParameter
	{
		private readonly BooleanGetHandler getHandler;
		private readonly BooleanSetHandler setHandler;

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
		public async Task<bool> Set(IThingReference Node, bool Value)
		{
			try
			{
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
			if (!CommonTypes.TryParse(StringValue, out bool Value))
				return false;

			await this.Set(Node, Value);

			return true;
		}

		/// <summary>
		/// Gets the value of the control parameter.
		/// </summary>
		/// <returns>Current value, or null if not available.</returns>
		public async Task<bool?> Get(IThingReference Node)
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
		public override async Task<string> GetStringValue(IThingReference Node)
		{
			bool? Value = await this.Get(Node);

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
		public override Task ExportValidationRules(XmlWriter Output, IThingReference Node)
		{
			Output.WriteStartElement("xdv", "validate", null);
			Output.WriteAttributeString("datatype", "xs:boolean");
			Output.WriteEndElement();

			return Task.CompletedTask;
		}
	}
}
