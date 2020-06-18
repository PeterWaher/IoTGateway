using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Events;

namespace Waher.Things.ControlParameters
{
	/// <summary>
	/// Set handler delegate for time control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being set.</param>
	/// <param name="Value">Value set.</param>
	public delegate Task TimeSetHandler(IThingReference Node, TimeSpan Value);

	/// <summary>
	/// Get handler delegate for time control parameters.
	/// </summary>
	/// <param name="Node">Node whose parameter is being retrieved.</param>
	/// <returns>Current value, or null if not available.</returns>
	public delegate Task<TimeSpan?> TimeGetHandler(IThingReference Node);

	/// <summary>
	/// Time control parameter.
	/// </summary>
	public class TimeControlParameter : ControlParameter
	{
		private readonly TimeGetHandler getHandler;
		private readonly TimeSetHandler setHandler;
		private readonly TimeSpan? min, max;

		/// <summary>
		/// Time control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public TimeControlParameter(string Name, string Page, string Label, string Description, TimeGetHandler GetHandler, TimeSetHandler SetHandler)
			: base(Name, Page, Label, Description)
		{
			this.getHandler = GetHandler;
			this.setHandler = SetHandler;
			this.min = null;
			this.max = null;
		}

		/// <summary>
		/// Time control parameter.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Page">On which page in the control dialog the parameter should appear.</param>
		/// <param name="Label">Label for parameter.</param>
		/// <param name="Description">Description for parameter.</param>
		/// <param name="Min">Smallest value allowed.</param>
		/// <param name="Max">Largest value allowed.</param>
		/// <param name="GetHandler">This callback method is called when the value of the parameter is needed.</param>
		/// <param name="SetHandler">This callback method is called when the value of the parameter is set.</param>
		public TimeControlParameter(string Name, string Page, string Label, string Description, TimeSpan? Min, TimeSpan? Max, 
			TimeGetHandler GetHandler, TimeSetHandler SetHandler)
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
		public async Task<bool> Set(IThingReference Node, TimeSpan Value)
		{
			try
			{
				if ((this.min.HasValue && Value < this.min.Value) || (this.max.HasValue && Value > this.max.Value))
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
			if (!TimeSpan.TryParse(StringValue, out TimeSpan Value))
				return false;

			await this.Set(Node, Value);

			return true;
		}

		/// <summary>
		/// Gets the value of the control parameter.
		/// </summary>
		/// <returns>Current value, or null if not available.</returns>
		public async Task<TimeSpan?> Get(IThingReference Node)
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
			TimeSpan? Value = await this.Get(Node);

			if (Value.HasValue)
				return Value.Value.ToString();
			else
				return string.Empty;
		}

		/// <summary>
		/// Exports form validation rules for the parameter.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="Node">Node reference, if available.</param>
		public override Task ExportValidationRules(XmlWriter Output, IThingReference Node)
		{
			Output.WriteStartElement("xdv", "validate", null);
			Output.WriteAttributeString("datatype", "xs:time");
			Output.WriteEndElement();

			return Task.CompletedTask;
		}
	}
}
