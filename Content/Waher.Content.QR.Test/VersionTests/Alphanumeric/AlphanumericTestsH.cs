using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.QR.Test.VersionTests.Alphanumeric
{
	[TestClass]
	public class AlphanumericTestsH : AlphanumericTests
	{
		public override CorrectionLevel Level => CorrectionLevel.H;
		public override string Folder => Path.Combine(base.Folder, "H");
	}
}
