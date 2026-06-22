using System;
using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a string-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = true, AllowMultiple = false)]
	public class McpStringParameterAttribute : McpParameterAttribute
	{
		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		public McpStringParameterAttribute(string? Title, string? Description)
			: base(Title, Description)
		{
			this.MinLength = null;
			this.MaxLength = null;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinLength">Minimum length of string.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpStringParameterAttribute(string? Title, string? Description,
			int? MinLength, int? MaxLength)
			: base(Title, Description)
		{
			this.MinLength = MinLength;
			this.MaxLength = MaxLength;
		}

		/// <summary>
		/// Minimum length of string.
		/// </summary>
		public int? MinLength { get; private set; }

		/// <summary>
		/// Maximum length of string.
		/// </summary>
		public int? MaxLength { get; private set; }

		/// <summary>
		/// Annotates a schema object with information in the attribute.
		/// </summary>
		/// <param name="Schema">Schema object being built.</param>
		public override void Annotate(Dictionary<string, object?> Schema)
		{
			base.Annotate(Schema);

			if (this.MinLength.HasValue)
				Schema["minLength"] = this.MinLength.Value;

			if (this.MaxLength.HasValue)
				Schema["maxLength"] = this.MaxLength.Value;
		}
	}
}
