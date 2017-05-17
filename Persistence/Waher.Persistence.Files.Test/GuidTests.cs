using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Files.Storage;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class GuidTests
	{
		private static SequentialGuidGenerator gen;

		[ClassInitialize]
		public static void ClassInitialize(TestContext Context)
		{
			gen = new SequentialGuidGenerator();
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (gen != null)
			{
				gen.Dispose();
				gen = null;
			}
		}

		[TestMethod]
		public void Test_01_Generate1()
		{
			Console.Out.WriteLine(gen.CreateGuid().ToString());
		}

		[TestMethod]
		public void Test_02_Generate100()
		{
			Guid Prev, Next;
			int i;

			Prev = gen.CreateGuid();
			Console.Out.WriteLine(Prev.ToString());

			for (i = 1; i < 100; i++)
			{
				Next = gen.CreateGuid();
				Console.Out.WriteLine(Next.ToString());

				AssertEx.Less(Prev, Next);

				Prev = Next;
			}
		}

		[TestMethod]
		public void Test_03_Generate1000000()
		{
			Guid Prev, Next;
			int i;

			Prev = gen.CreateGuid();

			for (i = 1; i < 1000000; i++)
			{
				Next = gen.CreateGuid();
				AssertEx.Less(Prev, Next);
				AssertEx.Less(Prev.ToString(), Next.ToString());
				Prev = Next;
			}
		}
	}
}
