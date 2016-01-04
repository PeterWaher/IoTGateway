using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to a text file.
	/// </summary>
	public class TextFileSniffer : TextWriterSniffer, IDisposable
	{
		private StreamWriter file;

		/// <summary>
		/// Outputs sniffed data to a text file.
		/// </summary>
		/// <param name="FileName">File Name</param>
		public TextFileSniffer(string FileName)
			: base(null)
		{
			this.file = File.CreateText(FileName);
			this.output = this.file;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.file != null)
			{
				this.file.Flush();
				this.file.Dispose();
				this.file = null;
			}
		}
	}
}
