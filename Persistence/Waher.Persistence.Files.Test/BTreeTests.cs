using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence.Files.Test.Classes;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public class BTreeTests
	{
		private const string FileName = "Data\\Objects.db";
		private const int BlockSize = 1024;
		private const int BlocksInCache = 1000;

		private ObjectBTreeFile file;
		private FilesProvider provider;
		private Random gen = new Random();

		[SetUp]
		public void SetUp()
		{
			if (File.Exists(FileName))
				File.Delete(FileName);

			this.provider = new FilesProvider("Data", "Default");
			this.file = new ObjectBTreeFile(FileName, "Default", "Blobs", BlockSize, BlocksInCache, this.provider, Encoding.UTF8, 10000);
		}

		[TearDown]
		public void TearDown()
		{
			this.file.Dispose();
			this.file = null;

			this.provider.Dispose();
			this.provider = null;
		}

		[Test]
		public async void Test_01_SaveNew()
		{
			Simple Result = this.CreateSimple();
			Guid ObjectId = await this.file.SaveNew(Result);
			Assert.AreNotEqual(Guid.Empty, ObjectId);
		}

		private Simple CreateSimple()
		{
			Simple Result = new Simple();

			Result.Boolean1 = this.gen.Next(2) == 1;
			Result.Boolean2 = this.gen.Next(2) == 1;
			Result.Byte = (byte)(this.gen.NextDouble() * 256);
			Result.Short = (short)(this.gen.NextDouble() * ((double)short.MaxValue - (double)short.MinValue + 1) + short.MinValue);
			Result.Int = (int)(this.gen.NextDouble() * ((double)int.MaxValue - (double)int.MinValue + 1) + int.MinValue);
			Result.Long = (long)(this.gen.NextDouble() * ((double)long.MaxValue - (double)long.MinValue + 1) + long.MinValue);
			Result.SByte = (sbyte)(this.gen.NextDouble() * ((double)sbyte.MaxValue - (double)sbyte.MinValue + 1) + sbyte.MinValue);
			Result.UShort = (ushort)(this.gen.NextDouble() * ((double)short.MaxValue + 1));
			Result.UInt = (uint)(this.gen.NextDouble() * ((double)short.MaxValue + 1));
			Result.ULong = (ulong)(this.gen.NextDouble() * ((double)short.MaxValue + 1));
			Result.Char = (char)(this.gen.Next(char.MaxValue));
			Result.Decimal = (decimal)this.gen.NextDouble();
			Result.Double = this.gen.NextDouble();
			Result.Single = (float)this.gen.NextDouble();
			Result.String = Guid.NewGuid().ToString();
			Result.DateTime = new DateTime(1900, 1, 1).AddDays(this.gen.NextDouble() * 73049);
			Result.TimeSpan = new TimeSpan((long)(this.gen.NextDouble() * 36000000000));
			Result.Guid = Guid.NewGuid();

			switch (this.gen.Next(4))
			{
				case 0:
					Result.NormalEnum = NormalEnum.Option1;
					break;

				case 1:
					Result.NormalEnum = NormalEnum.Option2;
					break;

				case 2:
					Result.NormalEnum = NormalEnum.Option3;
					break;

				case 3:
					Result.NormalEnum = NormalEnum.Option4;
					break;
			}

			Result.FlagsEnum = (FlagsEnum)this.gen.Next(16);

			return Result;
		}

		// TODO: Load Object
		// TODO: Delete Object
		// TODO: Multiple save (test node split)
		// TODO: Multiple delete (test node merge)
		// TODO: Update Object
		// TODO: Update Object (incl. node split)
		// TODO: Select i'th element.
		// TODO: Enumerate
		// TODO: Start enumeration from i'th element.
		// TODO: BLOBs
		// TODO: Update Object (normal -> BLOB)
		// TODO: Update Object (BLOB -> normal)
		// TODO: Statistics (nr objects, size, used vs. unused space)
		// TODO: Stress test
		// TODO: Check that node counts are correct after all tests.
	}
}
