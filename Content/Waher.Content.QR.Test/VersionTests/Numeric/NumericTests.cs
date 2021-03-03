using System;
using System.Text;
using Waher.Content.QR.Encoding;

namespace Waher.Content.QR.Test.VersionTests.Numeric
{
	public abstract class NumericTests : VersionTests
	{
		public override EncodingMode Mode => EncodingMode.Numeric;
		public override string Folder => "Numeric";

		public override string GetMessage(int ForVersion)
		{
			VersionInfo Version = Versions.FindVersionInfo(ForVersion, this.Level);
			int NrBytes = Version.TotalDataBytes - 2;
			int NrBits = NrBytes << 3;
			int Len = (NrBits / 10) * 3 - 1;
			int i;
			StringBuilder sb = new StringBuilder();

			for (i = 0; i < Len; i++)
				sb.Append((char)('0' + rnd.Next(0, 10)));

			return sb.ToString();
		}
	}
}
