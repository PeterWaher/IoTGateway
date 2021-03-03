using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.QR.Test.VersionTests.Bytes
{
	[TestClass]
	public class BytesTestsM : BytesTests
	{
		public override CorrectionLevel Level => CorrectionLevel.M;
		public override string Folder => Path.Combine(base.Folder, "M");
	}
}
