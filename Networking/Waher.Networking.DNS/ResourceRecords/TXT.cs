using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Descriptive text
	/// </summary>
	public class TXT : ResourceRecord
	{
		private readonly string[] text;

		/// <summary>
		/// Descriptive text
		/// </summary>
		public TXT(string[] Text)
		{
			this.text = Text;
		}

		/// <summary>
		/// Descriptive text.
		/// </summary>
		public string[] Text => this.text;
	}
}
