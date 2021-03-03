using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Content.QR.Test.VersionTests.Numeric
{
	[TestClass]
	public class NumericTestsQ : NumericTests
	{
		public override CorrectionLevel Level => CorrectionLevel.Q;
		public override string Folder => Path.Combine(base.Folder, "Q");
	}
}
