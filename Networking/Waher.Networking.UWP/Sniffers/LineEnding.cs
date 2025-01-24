namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Type of line ending.
	/// </summary>
	public enum LineEnding
	{
		/// <summary>
		/// Pad with spaces until next rows. Makes sure line is colored properly.
		/// </summary>
		PadWithSpaces,

		/// <summary>
		/// End with new line characters. Is easier to read in a text editor.
		/// </summary>
		NewLine
	}
}
