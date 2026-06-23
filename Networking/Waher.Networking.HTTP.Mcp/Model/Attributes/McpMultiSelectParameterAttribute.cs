using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a multi-select parameter (Enumeration value with 
	/// <see cref="FlagsAttribute"> defined.)
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field, 
		Inherited = true, AllowMultiple = false)]
	public class McpMultiSelectParameterAttribute : McpParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a multi-select parameter (Enumeration value with 
		/// <see cref="FlagsAttribute"> defined.)
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpMultiSelectParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
			this.MinItems = null;
			this.MaxItems = null;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinItems">Minimum number of items.</param>
		/// <param name="MaxItems">Maximum number of items.</param>
		public McpMultiSelectParameterAttribute(string? Title, string? Description,
			int? MinItems, int? MaxItems)
			: base(Title, Description)
		{
			this.MinItems = MinItems;
			this.MaxItems = MaxItems;
		}

		/// <summary>
		/// Minimum number of items.
		/// </summary>
		public int? MinItems { get; private set; }

		/// <summary>
		/// Maximum number of items.
		/// </summary>
		public int? MaxItems { get; private set; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			Schema["type"] = "array";

			if (this.MinItems.HasValue)
				Schema["minItems"] = this.MinItems.Value;

			if (this.MaxItems.HasValue)
				Schema["maxItems"] = this.MaxItems.Value;
		}
	}
}
