using System.Collections.Generic;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a parameter with possible options on a command.
	/// </summary>
	public abstract class ReportParameterWithOptions : ReportParameter
	{
		/// <summary>
		/// Represents a parameter with possible options on a command.
		/// </summary>
		/// <param name="Page">Parameter Page</param>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Label">Parameter label.</param>
		/// <param name="Description">Parameter description.</param>
		/// <param name="Required">If parameter is required.</param>
		/// <param name="RestrictToOptions">If only values defined in options are valid values.</param>
		/// <param name="Options">Available options</param>
		public ReportParameterWithOptions(string Page, string Name, string Label, string Description,
			bool Required, bool RestrictToOptions, ParameterOption[] Options)
			: base(Page, Name, Label, Description, Required)
		{
			this.Options = Options;
			this.RestrictToOptions = RestrictToOptions;
		}

		/// <summary>
		/// If only values defined in options are valid values.
		/// </summary>
		public bool RestrictToOptions { get; }

		/// <summary>
		/// Available options
		/// </summary>
		public ParameterOption[] Options { get; }

		/// <summary>
		/// Gets the options as a collection of label-value pairs.
		/// </summary>
		/// <returns>Label-value pairs representing the options.</returns>
		public KeyValuePair<string, string>[] GetOptionTags()
		{
			KeyValuePair<string, string>[] Result = new KeyValuePair<string, string>[this.Options.Length];

			for (int i = 0; i < this.Options.Length; i++)
				Result[i] = new KeyValuePair<string, string>(this.Options[i].Label, this.Options[i].Value);

			return Result;
		}

	}
}
