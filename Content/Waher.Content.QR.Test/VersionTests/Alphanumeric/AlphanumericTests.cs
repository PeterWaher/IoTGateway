using System;
using System.Text;
using Waher.Content.QR.Encoding;
using Waher.Content.QR.Serialization;

namespace Waher.Content.QR.Test.VersionTests.Alphanumeric
{
	public abstract class AlphanumericTests : VersionTests
	{
		public override EncodingMode Mode => EncodingMode.Alphanumeric;
		public override string Folder => "Alphanumeric";

		public override string GetMessage(int ForVersion)
		{
			VersionInfo Version = Versions.FindVersionInfo(ForVersion, this.Level);
			int NrBytes = Version.TotalDataBytes - 3;
			int NrBits = NrBytes << 3;
			int Len = (NrBits / 11) * 2 - 1;
			int i;
			StringBuilder sb = new StringBuilder();
			
			for (i = 0; i < Len; i++)
				sb.Append(AlphanumericEncoder.AlphanumericCharacters[rnd.Next(0, AlphanumericEncoder.AlphanumericCharacters.Length)]);

			return sb.ToString();
		}
	}
}
