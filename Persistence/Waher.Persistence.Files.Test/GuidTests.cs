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
		private SequentialGuidGenerator gen;

		[ClassInitialize]
		public void ClassInitialize()
		{
			this.gen = new SequentialGuidGenerator();
		}

		[ClassCleanup]
		public void ClassCleanup()
		{
			this.gen.Dispose();
			this.gen = null;
		}

		[TestMethod]
		public void Test_01_Generate1()
		{
			Console.Out.WriteLine(this.gen.CreateGuid().ToString());
		}

		[TestMethod]
		public void Test_02_Generate100()
		{
			Guid Prev, Next;
			int i;

			Prev = this.gen.CreateGuid();
			Console.Out.WriteLine(Prev.ToString());

			for (i = 1; i < 100; i++)
			{
				Next = this.gen.CreateGuid();
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

			Prev = this.gen.CreateGuid();

			for (i = 1; i < 1000000; i++)
			{
				Next = this.gen.CreateGuid();
				AssertEx.Less(Prev, Next);
				AssertEx.Less(Prev.ToString(), Next.ToString());
				Prev = Next;
			}
		}
	}
}
