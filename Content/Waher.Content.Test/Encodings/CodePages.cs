using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Test.Encodings
{
	public class CodePages : EncodingProvider
	{
		private static readonly Windows1252 windows1252 = new Windows1252();

		public override Encoding GetEncoding(int codepage)
		{
			switch (codepage)
			{
				case 1252: return windows1252;
				default: return null;
			}
		}

		public override Encoding GetEncoding(string name)
		{
			switch (name.ToUpper())
			{
				case "WINDOWS-1252": return windows1252;
				default: return null;
			}
		}
	}
}
