using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Xml;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a date and time-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpDateTimeParameterAttribute : McpRangeParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a date and time-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpDateTimeParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
		}

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["format"] = "date-time";
		}

		/// <summary>
		/// Gets HTML input attributes for the parameter, if any.
		/// </summary>
		/// <param name="Attributes">Set of attributes.</param>
		public override void GetHtmlInputAttributes(Dictionary<string, string> Attributes)
		{
			base.GetHtmlInputAttributes(Attributes);
			Attributes["type"] = "datetime-local";
		}

		/// <summary>
		/// Gets the HTML attribute value for a parameter, if any.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>HTML attribute value</returns>
		public override string GetHtmlAttributeValue(object Value)
		{
			if (Value is DateTime TP)
				return XML.Encode(TP, false);
			else
				return base.GetHtmlAttributeValue(Value);
		}

		/// <summary>
		/// Checks if a value is valid for the parameter.
		/// </summary>
		/// <param name="Value">Value to check.</param>
		/// <returns>If the value is valid according to validation rules for the parameter.</returns>
		public override Task<bool> IsValid(object Value)
		{
			return Task.FromResult(Value is DateTime);
		}
	}
}
