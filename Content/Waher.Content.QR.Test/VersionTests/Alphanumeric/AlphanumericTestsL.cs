using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.QR.Test.VersionTests.Alphanumeric
{
	[TestClass]
	public class AlphanumericTestsL : AlphanumericTests
	{
		public override CorrectionLevel Level => CorrectionLevel.L;
		public override string Folder => Path.Combine(base.Folder, "L");
	}
}
