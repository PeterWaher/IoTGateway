using System;
using System.Collections.Generic;
using System.Text;

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
		public ConsoleErrorSniffer()
			: base(Console.Error)
		{
		}
	}
}
