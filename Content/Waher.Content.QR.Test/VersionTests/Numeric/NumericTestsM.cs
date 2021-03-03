using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.QR.Test.VersionTests.Numeric
{
	[TestClass]
	public class NumericTestsM : NumericTests
	{
		public override CorrectionLevel Level => CorrectionLevel.M;
		public override string Folder => Path.Combine(base.Folder, "M");
	}
}
