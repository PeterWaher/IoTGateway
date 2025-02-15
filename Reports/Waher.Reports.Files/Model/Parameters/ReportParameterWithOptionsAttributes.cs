using System.Collections.Generic;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Attributes of a report parameter with options.
	/// </summary>
	public class ReportParameterWithOptionsAttributes : ReportParameterAttributes
	{
		/// <summary>
		/// If only values defined in options are valid values.
		/// </summary>
		public bool RestrictToOptions { get; set; }

		/// <summary>
		/// Available options
		/// </summary>
		public KeyValuePair<string, string>[] Options { get; set; }
	}
}
