using Waher.Runtime.Console;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to the console error output.
	/// </summary>
	public class ConsoleErrorSniffer : TextWriterSniffer
	{
		/// <summary>
		/// Outputs sniffed data to the console error output.
		/// </summary>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public ConsoleErrorSniffer(BinaryPresentationMethod BinaryPresentationMethod)
			: base(ConsoleError.Writer, BinaryPresentationMethod, "Console Error")
		{
		} 

		/// <summary>
		/// If output can be disposed.
		/// </summary>
		public override bool CanDisposeOutput => false;
	}
}
