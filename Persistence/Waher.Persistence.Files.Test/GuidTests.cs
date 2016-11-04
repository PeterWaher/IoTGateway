using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public class GuidTests
	{
		private SequentialGuidGenerator gen;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			this.gen = new SequentialGuidGenerator();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			this.gen.Dispose();
			this.gen = null;
		}

		[Test]
		public void Test_01_Generate1()
		{
			Console.Out.WriteLine(this.gen.CreateGuid().ToString());
		}

		[Test]
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

				Assert.Less(Prev, Next);

				Prev = Next;
			}
		}

		[Test]
		public void Test_03_Generate1000000()
		{
			Guid Prev, Next;
			int i;

			Prev = this.gen.CreateGuid();

			for (i = 1; i < 1000000; i++)
			{
				Next = this.gen.CreateGuid();
				Assert.Less(Prev, Next);
				Assert.Less(Prev.ToString(), Next.ToString());
				Prev = Next;
			}
		}
	}
}
