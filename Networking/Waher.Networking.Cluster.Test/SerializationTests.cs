using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.Cluster.Test.TestObjects;
namespace Waher.Networking.Cluster.Test
{
	[TestClass]
	public class SerializationTests
	{
		private ClusterEndpoint endpoint = null;

		[TestInitialize]
		public void TestInitialize()
		{
			this.endpoint = new ClusterEndpoint(EndpointTests.clusterAddress, 12345, "UnitTest",
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.endpoint?.Dispose();
			this.endpoint = null;
		}

		[TestMethod]
		public void Test_01_Strings()
		{
			Strings Obj = new Strings()
			{
				S1 = "Hello",
				S2 = "World",
				S3 = "!"
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			Strings Obj2 = this.endpoint.Deserialize(Bin) as Strings;

			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.S1, Obj2.S1);
			Assert.AreEqual(Obj.S2, Obj2.S2);
			Assert.AreEqual(Obj.S3, Obj2.S3);
		}

		[TestMethod]
		public void Test_02_Integers()
		{
			Integers Obj = new Integers()
			{
				Int8 = -0x01,
				Int16 = -0x0102,
				Int32 = -0x01020304,
				Int64 = -0x0102030405060708,
				UInt8 = 0x01,
				UInt16 = 0x0102,
				UInt32 = 0x01020304,
				UInt64 = 0x0102030405060708,
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			Integers Obj2 = this.endpoint.Deserialize(Bin) as Integers;

			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.Int8, Obj2.Int8);
			Assert.AreEqual(Obj.Int16, Obj2.Int16);
			Assert.AreEqual(Obj.Int32, Obj2.Int32);
			Assert.AreEqual(Obj.Int64, Obj2.Int64);
			Assert.AreEqual(Obj.UInt8, Obj2.UInt8);
			Assert.AreEqual(Obj.UInt16, Obj2.UInt16);
			Assert.AreEqual(Obj.UInt32, Obj2.UInt32);
			Assert.AreEqual(Obj.UInt64, Obj2.UInt64);
		}

		[TestMethod]
		public void Test_03_FloatingPointNumbers()
		{
			FloatingPoints Obj = new FloatingPoints()
			{
				S = 1.2345F,
				D = 1.2345,
				Dec = 1.2345M
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			FloatingPoints Obj2 = this.endpoint.Deserialize(Bin) as FloatingPoints;

			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.S, Obj2.S);
			Assert.AreEqual(Obj.D, Obj2.D);
			Assert.AreEqual(Obj.Dec, Obj2.Dec);
		}

		[TestMethod]
		public void Test_04_MiscTypes()
		{
			MiscTypes Obj = new MiscTypes()
			{
				B = true,
				Ch = 'x',
				DT = DateTime.UtcNow,
				DTO = DateTimeOffset.UtcNow,
				TS = DateTime.Now.TimeOfDay,
				Id = Guid.NewGuid(),
				E1 = NormalEnum.B,
				E2 = FlagsEnum.A | FlagsEnum.C
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			MiscTypes Obj2 = this.endpoint.Deserialize(Bin) as MiscTypes;

			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.B, Obj2.B);
			Assert.AreEqual(Obj.Ch, Obj2.Ch);
			Assert.AreEqual(Obj.DT, Obj2.DT);
			Assert.AreEqual(Obj.DTO, Obj2.DTO);
			Assert.AreEqual(Obj.TS, Obj2.TS);
			Assert.AreEqual(Obj.Id, Obj2.Id);
			Assert.AreEqual(Obj.E1, Obj2.E1);
			Assert.AreEqual(Obj.E2, Obj2.E2);
		}

		[TestMethod]
		public void Test_05_Nullable_1()
		{
			NullableTypes Obj = new NullableTypes()
			{
				Int8 = null,
				Int16 = -0x0102,
				Int32 = null,
				Int64 = -0x0102030405060708,
				UInt8 = null,
				UInt16 = 0x0102,
				UInt32 = null,
				UInt64 = 0x0102030405060708,
				S = null,
				D = 1.2345,
				Dec = null,
				B = true,
				Ch = null,
				DT = DateTime.UtcNow,
				DTO = null,
				TS = DateTime.Now.TimeOfDay,
				Id = null,
				E1 = NormalEnum.B,
				E2 = null
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			NullableTypes Obj2 = this.endpoint.Deserialize(Bin) as NullableTypes;

			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.Int8, Obj2.Int8);
			Assert.AreEqual(Obj.Int16, Obj2.Int16);
			Assert.AreEqual(Obj.Int32, Obj2.Int32);
			Assert.AreEqual(Obj.Int64, Obj2.Int64);
			Assert.AreEqual(Obj.UInt8, Obj2.UInt8);
			Assert.AreEqual(Obj.UInt16, Obj2.UInt16);
			Assert.AreEqual(Obj.UInt32, Obj2.UInt32);
			Assert.AreEqual(Obj.UInt64, Obj2.UInt64);
			Assert.AreEqual(Obj.S, Obj2.S);
			Assert.AreEqual(Obj.D, Obj2.D);
			Assert.AreEqual(Obj.Dec, Obj2.Dec);
			Assert.AreEqual(Obj.B, Obj2.B);
			Assert.AreEqual(Obj.Ch, Obj2.Ch);
			Assert.AreEqual(Obj.DT, Obj2.DT);
			Assert.AreEqual(Obj.DTO, Obj2.DTO);
			Assert.AreEqual(Obj.TS, Obj2.TS);
			Assert.AreEqual(Obj.Id, Obj2.Id);
			Assert.AreEqual(Obj.E1, Obj2.E1);
			Assert.AreEqual(Obj.E2, Obj2.E2);
		}

		[TestMethod]
		public void Test_06_Nullable_2()
		{
			NullableTypes Obj = new NullableTypes()
			{
				Int8 = -0x01,
				Int16 = null,
				Int32 = -0x01020304,
				Int64 = null,
				UInt8 = 0x01,
				UInt16 = null,
				UInt32 = 0x01020304,
				UInt64 = null,
				S = 1.2345F,
				D = null,
				Dec = 1.2345M,
				B = null,
				Ch = 'x',
				DT = null,
				DTO = DateTimeOffset.UtcNow,
				TS = null,
				Id = Guid.NewGuid(),
				E1 = null,
				E2 = FlagsEnum.A | FlagsEnum.C
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			NullableTypes Obj2 = this.endpoint.Deserialize(Bin) as NullableTypes;

			Assert.IsNotNull(Obj2);
			Assert.AreEqual(Obj.Int8, Obj2.Int8);
			Assert.AreEqual(Obj.Int16, Obj2.Int16);
			Assert.AreEqual(Obj.Int32, Obj2.Int32);
			Assert.AreEqual(Obj.Int64, Obj2.Int64);
			Assert.AreEqual(Obj.UInt8, Obj2.UInt8);
			Assert.AreEqual(Obj.UInt16, Obj2.UInt16);
			Assert.AreEqual(Obj.UInt32, Obj2.UInt32);
			Assert.AreEqual(Obj.UInt64, Obj2.UInt64);
			Assert.AreEqual(Obj.S, Obj2.S);
			Assert.AreEqual(Obj.D, Obj2.D);
			Assert.AreEqual(Obj.Dec, Obj2.Dec);
			Assert.AreEqual(Obj.B, Obj2.B);
			Assert.AreEqual(Obj.Ch, Obj2.Ch);
			Assert.AreEqual(Obj.DT, Obj2.DT);
			Assert.AreEqual(Obj.DTO, Obj2.DTO);
			Assert.AreEqual(Obj.TS, Obj2.TS);
			Assert.AreEqual(Obj.Id, Obj2.Id);
			Assert.AreEqual(Obj.E1, Obj2.E1);
			Assert.AreEqual(Obj.E2, Obj2.E2);
		}

		[TestMethod]
		public void Test_07_Arrays()
		{
			Arrays Obj = new Arrays()
			{
				Integers = new int[] { 1, 2, 3, 4 },
				Binary = new byte[] { 5, 6, 7, 8, 9 },
				Objects = new Parent[]
				{
					new Parent()
					{
						S = "ABC"
					},
					new Child()
					{
						S = "DEF",
						I = 12345
					},
					new GrandChild()
					{
						S = "GHI",
						I = 23456,
						B = true
					}
				}
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			Arrays Obj2 = this.endpoint.Deserialize(Bin) as Arrays;
			int i, c;

			Assert.IsNotNull(Obj2);

			Assert.AreEqual(c = Obj.Integers.Length, Obj2.Integers.Length);
			for (i = 0; i < c; i++)
				Assert.AreEqual(Obj.Integers[i], Obj2.Integers[i]);

			Assert.AreEqual(c = Obj.Binary.Length, Obj2.Binary.Length);
			for (i = 0; i < c; i++)
				Assert.AreEqual(Obj.Binary[i], Obj2.Binary[i]);

			Assert.AreEqual(c = Obj.Objects.Length, Obj2.Objects.Length);
			for (i = 0; i < c; i++)
				Assert.IsTrue(Obj.Objects[i].Equals(Obj2.Objects[i]));
		}

		[TestMethod]
		public void Test_08_Objects()
		{
			Objects Obj = new Objects()
			{
				O1 = new Parent()
				{
					S = "ABC"
				},
				O2 = new Child()
				{
					S = "DEF",
					I = 12345
				},
				O3 = new GrandChild()
				{
					S = "GHI",
					I = 23456,
					B = true
				}
			};

			byte[] Bin = this.endpoint.Serialize(Obj);
			Objects Obj2 = this.endpoint.Deserialize(Bin) as Objects;

			Assert.IsNotNull(Obj2);
			Assert.IsTrue(Obj.O1.Equals(Obj2.O1));
			Assert.IsTrue(Obj.O2.Equals(Obj2.O2));
			Assert.IsTrue(Obj.O3.Equals(Obj2.O3));
		}

	}
}
