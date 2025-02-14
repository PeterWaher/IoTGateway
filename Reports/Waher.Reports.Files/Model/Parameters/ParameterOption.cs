namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a string-valued option.
	/// </summary>
	public class ParameterOption
	{
		/// <summary>
		/// Represents a string-valued option.
		/// </summary>
		public ParameterOption(string Label, string Value)
			: base()
		{
			this.Label = Label;
			this.Value = Value;
		}

		/// <summary>
		/// Option value.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Option label.
		/// </summary>
		public string Label { get; }
	}
}