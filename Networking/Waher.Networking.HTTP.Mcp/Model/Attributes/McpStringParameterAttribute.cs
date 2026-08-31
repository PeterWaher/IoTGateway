using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Waher.Networking.HTTP.Mcp.Model.Attributes
{
	/// <summary>
	/// Provides meta-data about a string-valued parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue |
		AttributeTargets.Property | AttributeTargets.Field,
		Inherited = true, AllowMultiple = false)]
	public class McpStringParameterAttribute : McpParameterAttribute
	{
		private readonly string? pattern = null;
		private readonly Regex? regex = null;

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
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpStringParameterAttribute(string? Title, string? Description,
			int MaxLength)
			: base(Title, Description)
		{
			this.MinLength = null;
			this.MaxLength = MaxLength;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinLength">Minimum length of string.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		public McpStringParameterAttribute(string? Title, string? Description,
			int MinLength, int MaxLength)
			: base(Title, Description)
		{
			this.MinLength = MinLength;
			this.MaxLength = MaxLength;
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="Pattern">Regular expression pattern that the string 
		/// must match.</param>
		public McpStringParameterAttribute(string? Title, string? Description,
			string Pattern)
			: base(Title, Description)
		{
			this.MinLength = null;
			this.MaxLength = null;
			this.pattern = Pattern;
			this.regex = new Regex(Pattern, RegexOptions.Singleline);
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		/// <param name="Pattern">Regular expression pattern that the string 
		/// must match.</param>
		public McpStringParameterAttribute(string? Title, string? Description,
			string Pattern, int MaxLength)
			: base(Title, Description)
		{
			this.MinLength = null;
			this.MaxLength = MaxLength;
			this.pattern = Pattern;
			this.regex = new Regex(Pattern, RegexOptions.Singleline);
		}

		/// <summary>
		/// Provides meta-data about a string-valued parameter.
		/// </summary>
		/// <param name="Title">Title of parameter.</param>
		/// <param name="Description">Description of parameter.</param>
		/// <param name="MinLength">Minimum length of string.</param>
		/// <param name="MaxLength">Maximum length of string.</param>
		/// <param name="Pattern">Regular expression pattern that the string 
		/// must match.</param>
		public McpStringParameterAttribute(string? Title, string? Description,
			string Pattern, int MinLength, int MaxLength)
			: base(Title, Description)
		{
			this.MinLength = MinLength;
			this.MaxLength = MaxLength;
			this.pattern = Pattern;
			this.regex = new Regex(Pattern, RegexOptions.Singleline);
		}

		/// <summary>
		/// Minimum length of string.
		/// </summary>
		public int? MinLength { get; }

		/// <summary>
		/// Maximum length of string.
		/// </summary>
		public int? MaxLength { get; }

		/// <summary>
		/// Regular expression pattern that the string must match.
		/// </summary>
		public string? Pattern => this.pattern;

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

			if (!string.IsNullOrEmpty(this.Pattern))
				Schema["pattern"] = this.Pattern;
		}

		/// <summary>
		/// Gets HTML input attributes for the parameter, if any.
		/// </summary>
		/// <param name="Attributes">Set of attributes.</param>
		public override void GetHtmlInputAttributes(Dictionary<string, string> Attributes)
		{
			if (this.MinLength.HasValue)
				Attributes["minlength"] = this.MinLength.ToString();

			if (this.MaxLength.HasValue)
				Attributes["maxlength"] = this.MaxLength.ToString();

			if (!string.IsNullOrEmpty(this.Pattern))
				Attributes["pattern"] = this.Pattern;
		}

		/// <summary>
		/// Checks if a value is valid for the parameter.
		/// </summary>
		/// <param name="Value">Value to check.</param>
		/// <returns>If the value is valid according to validation rules for the parameter.</returns>
		public override Task<bool> IsValid(object Value)
		{
			if (!(Value is string s))
				return Task.FromResult(false);

			if (this.MinLength.HasValue && s.Length < this.MinLength.Value)
				return Task.FromResult(false);

			if (this.MaxLength.HasValue && s.Length > this.MaxLength.Value)
				return Task.FromResult(false);

			if (!(this.regex is null))
			{
				Match M = this.regex.Match(s);

				if (!M.Success || M.Index > 0 || M.Length < s.Length)
					return Task.FromResult(false);
			}

			return Task.FromResult(true);
		}

		/// <summary>
		/// If the parameter is required.
		/// </summary>
		public override bool IsRequired(Type ValueType)
		{
			if (this.MinLength.HasValue && this.MinLength.Value > 0)
				return true;

			if (!(this.regex is null))
			{
				Match M = this.regex.Match(string.Empty);

				if (!M.Success)
					return true;
			}

			return base.IsRequired(ValueType);
		}
	}
}
