using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.QR.Test.VersionTests.Alphanumeric
{
	[TestClass]
	public class AlphanumericTestsM : AlphanumericTests
	{
		public override CorrectionLevel Level => CorrectionLevel.M;
		public override string Folder => Path.Combine(base.Folder, "M");
	}
}
