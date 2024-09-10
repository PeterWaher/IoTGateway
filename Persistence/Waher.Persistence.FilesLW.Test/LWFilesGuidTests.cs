using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Waher.Persistence.Files.Storage;
using Waher.Runtime.Console;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class DBFilesGuidTests
	{
		private static SequentialGuidGenerator gen;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			gen = new SequentialGuidGenerator();
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (gen is not null)
			{
				gen.Dispose();
				gen = null;
			}
		}

		[TestMethod]
		public void DBFiles_Guid_Test_01_Generate1()
		{
			ConsoleOut.WriteLine(gen.CreateGuid().ToString());
		}

		[TestMethod]
		public void DBFiles_Guid_Test_02_Generate100()
		{
			Guid Prev, Next;
			int i;

			Prev = gen.CreateGuid();
			ConsoleOut.WriteLine(Prev.ToString());

			for (i = 1; i < 100; i++)
			{
				Next = gen.CreateGuid();
				ConsoleOut.WriteLine(Next.ToString());

				AssertEx.Less(Prev, Next);

				Prev = Next;
			}
		}

		[TestMethod]
		public void DBFiles_Guid_Test_03_Generate1000000()
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

		[TestMethod]
		public void DBFiles_Guid_Test_04_Order()
		{
			Guid[] Guids = new Guid[]
			{
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }),
				new(new byte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }),
			};

			Guid[] Guids2 = (Guid[])Guids.Clone();
			Array.Sort<Guid>(Guids2);

			foreach (Guid Guid in Guids2)
				ConsoleOut.WriteLine(Array.IndexOf(Guids, Guid));
		}
	}
}
