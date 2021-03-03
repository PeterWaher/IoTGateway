using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.QR.Test.VersionTests.Bytes
{
	[TestClass]
	public class BytesTestsL : BytesTests
	{
		public override CorrectionLevel Level => CorrectionLevel.L;
		public override string Folder => Path.Combine(base.Folder, "L");
	}
}
