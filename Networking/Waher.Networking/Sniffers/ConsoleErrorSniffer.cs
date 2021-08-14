using System;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to <see cref="Console.Error"/>.
	/// </summary>
	public class ConsoleErrorSniffer : TextWriterSniffer
	{
		/// <summary>
		/// Outputs sniffed data to <see cref="Console.Error"/>.
		/// </summary>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public ConsoleErrorSniffer(BinaryPresentationMethod BinaryPresentationMethod)
			: base(Console.Error, BinaryPresentationMethod)
		{
		}
	}
}
