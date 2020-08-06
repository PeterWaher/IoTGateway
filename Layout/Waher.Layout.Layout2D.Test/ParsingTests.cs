using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Layout.Layout2D.Test
{
	[TestClass]
	public class ParsingTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(ParsingTests).Assembly,
				typeof(InternetContent).Assembly,
				typeof(Log).Assembly,
				typeof(Expression).Assembly,
				typeof(Layout2DDocument).Assembly);
		}

		protected virtual void Test(string FileName)
		{
			Layout2DDocument.FromFile("Xml\\" + FileName);
		}

		[TestMethod]
		public void Test_01_Line()
		{
			this.Test("Test_01_Line.xml");
		}
	}
}
