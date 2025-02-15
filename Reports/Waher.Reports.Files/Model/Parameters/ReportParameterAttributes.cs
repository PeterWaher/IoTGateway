namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Attributes of a report parameter.
	/// </summary>
	public class ReportParameterAttributes
	{
		/// <summary>
		/// Parameter page.
		/// </summary>
		public string Page { get; set; }

		/// <summary>
		/// Parameter name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Parameter label.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Parameter description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// If parameter is required.
		/// </summary>
		public bool Required { get; set; }
	}
}
