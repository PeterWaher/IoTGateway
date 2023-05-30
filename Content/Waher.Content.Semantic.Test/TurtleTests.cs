using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Test
{
	[TestClass]
	public class TurtleTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(TurtleDocument).Assembly,
				typeof(TurtleTests).Assembly);
		}

		[TestMethod]
		public void Test_01_Example1()
		{
			this.PerformTest("example1.ttl");
		}

		private void PerformTest(string FileName)
		{
			string Text = Resources.LoadResourceAsText(typeof(TurtleTests).Namespace + ".Data.Turtle.Input." + FileName);
			TurtleDocument Parsed = new(Text);

			foreach (ISemanticTriple Triple in Parsed)
			{
				Console.Out.Write(Triple.Subject);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Predicate);
				Console.Out.Write("\t");
				Console.Out.Write(Triple.Object);
				Console.Out.WriteLine();
			}
		}
	}
}