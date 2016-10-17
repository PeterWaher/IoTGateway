using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Test.Classes;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public class ObjectSerializationTests
	{
		private FilesProvider provider;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			this.provider = new FilesProvider("Data", "Default", true);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			this.provider.Dispose();
			this.provider = null;
		}

		[Test]
		public void Test_01_SimpleObject()
		{
			Simple Obj = new Simple();

			Obj.Boolean1 = true;
			Obj.Boolean2 = false;
			Obj.Byte = 15;
			Obj.Short = -1234;
			Obj.Int = -23456789;
			Obj.Long = -345456456456456345;
			Obj.SByte = -45;
			Obj.UShort = 23456;
			Obj.UInt = 334534564;
			Obj.ULong = 4345345345345345;
			Obj.Char = '☀';
			Obj.Decimal = 12345.6789M;
			Obj.Double = 12345.6789;
			Obj.Single = 12345.6789f;
			Obj.String = "Today, there will be a lot of ☀.";
			Obj.DateTime = DateTime.Now;
			Obj.TimeSpan = Obj.DateTime.TimeOfDay;
			Obj.Guid = Guid.NewGuid();
			Obj.NormalEnum = NormalEnum.Option3;
			Obj.FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4;

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Simple));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			Simple Obj2 = (Simple)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Boolean1, Obj2.Boolean1);
			Assert.AreEqual(Obj.Boolean2, Obj2.Boolean2);
			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
			Assert.AreEqual(Obj.Char, Obj2.Char);
			Assert.AreEqual(Obj.Decimal, Obj2.Decimal);
			Assert.AreEqual(Obj.Double, Obj2.Double);
			Assert.AreEqual(Obj.Single, Obj2.Single);
			Assert.AreEqual(Obj.String, Obj2.String);
			Assert.AreEqual(Obj.DateTime, Obj2.DateTime);
			Assert.AreEqual(Obj.TimeSpan, Obj2.TimeSpan);
			Assert.AreEqual(Obj.Guid, Obj2.Guid);
			Assert.AreEqual(Obj.NormalEnum, Obj2.NormalEnum);
			Assert.AreEqual(Obj.FlagsEnum, Obj2.FlagsEnum);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		private void WriteData(byte[] Data)
		{
			int i, c = Data.Length;

			for (i = 0; i < c; i++)
			{
				if ((i & 15) == 0)
					Console.Out.WriteLine();
				else
					Console.Out.Write(' ');

				Console.Out.Write(Data[i].ToString("x2"));
			}
		}

		[Test]
		public void Test_02_Nullable1()
		{
			Classes.Nullable Obj = new Classes.Nullable();

			Obj.Boolean1 = true;
			Obj.Byte = 15;
			Obj.Int = -23456789;
			Obj.SByte = -45;
			Obj.UInt = 334534564;
			Obj.Char = '☀';
			Obj.Double = 12345.6789;
			Obj.String = "Today, there will be a lot of ☀.";
			Obj.TimeSpan = DateTime.Now.TimeOfDay;
			Obj.NormalEnum = NormalEnum.Option3;

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Classes.Nullable));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			Classes.Nullable Obj2 = (Classes.Nullable)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Boolean1, Obj2.Boolean1);
			Assert.AreEqual(Obj.Boolean2, Obj2.Boolean2);
			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
			Assert.AreEqual(Obj.Char, Obj2.Char);
			Assert.AreEqual(Obj.Decimal, Obj2.Decimal);
			Assert.AreEqual(Obj.Double, Obj2.Double);
			Assert.AreEqual(Obj.Single, Obj2.Single);
			Assert.AreEqual(Obj.String, Obj2.String);
			Assert.AreEqual(Obj.DateTime, Obj2.DateTime);
			Assert.AreEqual(Obj.TimeSpan, Obj2.TimeSpan);
			Assert.AreEqual(Obj.Guid, Obj2.Guid);
			Assert.AreEqual(Obj.NormalEnum, Obj2.NormalEnum);
			Assert.AreEqual(Obj.FlagsEnum, Obj2.FlagsEnum);
		}

		[Test]
		public void Test_03_Nullable2()
		{
			Classes.Nullable Obj = new Classes.Nullable();

			Obj.Boolean2 = false;
			Obj.Short = -1234;
			Obj.Long = -345456456456456345;
			Obj.UShort = 23456;
			Obj.ULong = 4345345345345345;
			Obj.Decimal = 12345.6789M;
			Obj.Single = 12345.6789f;
			Obj.DateTime = DateTime.Now;
			Obj.Guid = Guid.NewGuid();
			Obj.FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4;

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Classes.Nullable));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			Classes.Nullable Obj2 = (Classes.Nullable)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Boolean1, Obj2.Boolean1);
			Assert.AreEqual(Obj.Boolean2, Obj2.Boolean2);
			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
			Assert.AreEqual(Obj.Char, Obj2.Char);
			Assert.AreEqual(Obj.Decimal, Obj2.Decimal);
			Assert.AreEqual(Obj.Double, Obj2.Double);
			Assert.AreEqual(Obj.Single, Obj2.Single);
			Assert.AreEqual(Obj.String, Obj2.String);
			Assert.AreEqual(Obj.DateTime, Obj2.DateTime);
			Assert.AreEqual(Obj.TimeSpan, Obj2.TimeSpan);
			Assert.AreEqual(Obj.Guid, Obj2.Guid);
			Assert.AreEqual(Obj.NormalEnum, Obj2.NormalEnum);
			Assert.AreEqual(Obj.FlagsEnum, Obj2.FlagsEnum);
		}

		[Test]
		public void Test_04_Default1()
		{
			Default Obj = new Default();

			Obj.Boolean1 = true;
			Obj.Boolean2 = false;
			Obj.Byte = 10;
			Obj.Short = -1234;
			Obj.Int = 10;
			Obj.Long = -345456456456456345;
			Obj.SByte = 10;
			Obj.UShort = 23456;
			Obj.UInt = 10;
			Obj.ULong = 4345345345345345;
			Obj.Char = 'x';
			Obj.Decimal = 12345.6789M;
			Obj.Double = 10;
			Obj.Single = 12345.6789f;
			Obj.String = string.Empty;
			Obj.DateTime = DateTime.Now;
			Obj.TimeSpan = TimeSpan.MinValue;
			Obj.Guid = Guid.NewGuid();
			Obj.NormalEnum = NormalEnum.Option1;
			Obj.FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4;
			Obj.String2 = null;

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Default));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			Default Obj2 = (Default)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Boolean1, Obj2.Boolean1);
			Assert.AreEqual(Obj.Boolean2, Obj2.Boolean2);
			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
			Assert.AreEqual(Obj.Char, Obj2.Char);
			Assert.AreEqual(Obj.Decimal, Obj2.Decimal);
			Assert.AreEqual(Obj.Double, Obj2.Double);
			Assert.AreEqual(Obj.Single, Obj2.Single);
			Assert.AreEqual(Obj.String, Obj2.String);
			Assert.AreEqual(Obj.DateTime, Obj2.DateTime);
			Assert.AreEqual(Obj.TimeSpan, Obj2.TimeSpan);
			Assert.AreEqual(Obj.Guid, Obj2.Guid);
			Assert.AreEqual(Obj.NormalEnum, Obj2.NormalEnum);
			Assert.AreEqual(Obj.FlagsEnum, Obj2.FlagsEnum);
			Assert.AreEqual(Obj.String2, Obj2.String2);
		}

		[Test]
		public void Test_05_Default2()
		{
			Default Obj = new Default();

			Obj.Boolean1 = false;
			Obj.Boolean2 = true;
			Obj.Byte = 15;
			Obj.Short = 10;
			Obj.Int = -23456789;
			Obj.Long = 10;
			Obj.SByte = -45;
			Obj.UShort = 10;
			Obj.UInt = 334534564;
			Obj.ULong = 10;
			Obj.Char = '☀';
			Obj.Decimal = 10;
			Obj.Double = 12345.6789;
			Obj.Single = 10;
			Obj.String = "Today, there will be a lot of ☀.";
			Obj.DateTime = DateTime.MinValue;
			Obj.TimeSpan = DateTime.Now.TimeOfDay;
			Obj.Guid = Guid.Empty;
			Obj.NormalEnum = NormalEnum.Option3;
			Obj.FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option2;
			Obj.String2 = "Hello";

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Default));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			Default Obj2 = (Default)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Boolean1, Obj2.Boolean1);
			Assert.AreEqual(Obj.Boolean2, Obj2.Boolean2);
			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
			Assert.AreEqual(Obj.Char, Obj2.Char);
			Assert.AreEqual(Obj.Decimal, Obj2.Decimal);
			Assert.AreEqual(Obj.Double, Obj2.Double);
			Assert.AreEqual(Obj.Single, Obj2.Single);
			Assert.AreEqual(Obj.String, Obj2.String);
			Assert.AreEqual(Obj.DateTime, Obj2.DateTime);
			Assert.AreEqual(Obj.TimeSpan, Obj2.TimeSpan);
			Assert.AreEqual(Obj.Guid, Obj2.Guid);
			Assert.AreEqual(Obj.NormalEnum, Obj2.NormalEnum);
			Assert.AreEqual(Obj.FlagsEnum, Obj2.FlagsEnum);
			Assert.AreEqual(Obj.String2, Obj2.String2);
		}

		[Test]
		public void Test_06_SimpleArrays()
		{
			SimpleArrays Obj = new SimpleArrays();

			Obj.Boolean = new bool[] { true, false };
			Obj.Byte = new byte[] { 1, 2, 3 };
			Obj.Short = new short[] { 1, 2, 3 };
			Obj.Int = new int[] { 1, 2, 3 };
			Obj.Long = new long[] { 1, 2, 3 };
			Obj.SByte = new sbyte[] { 1, 2, 3 };
			Obj.UShort = new ushort[] { 1, 2, 3 };
			Obj.UInt = new uint[] { 1, 2, 3 };
			Obj.ULong = new ulong[] { 1, 2, 3 };
			Obj.Char = new char[] { 'a', 'b', 'c', '☀' };
			Obj.Decimal = new decimal[] { 1, 2, 3 }; ;
			Obj.Double = new double[] { 1, 2, 3 }; ;
			Obj.Single = new float[] { 1, 2, 3 }; ;
			Obj.String = new string[] { "a", "b", "c", "Today, there will be a lot of ☀." };
			Obj.DateTime = new DateTime[] { DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue };
			Obj.TimeSpan = new TimeSpan[] { DateTime.Now.TimeOfDay, TimeSpan.Zero };
			Obj.Guid = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
			Obj.NormalEnum = new NormalEnum[] { NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4 };
			Obj.FlagsEnum = new FlagsEnum[] { FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3 };

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(SimpleArrays));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			SimpleArrays Obj2 = (SimpleArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Boolean, Obj2.Boolean);
			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
			Assert.AreEqual(Obj.Char, Obj2.Char);
			Assert.AreEqual(Obj.Decimal, Obj2.Decimal);
			Assert.AreEqual(Obj.Double, Obj2.Double);
			Assert.AreEqual(Obj.Single, Obj2.Single);
			Assert.AreEqual(Obj.String, Obj2.String);
			Assert.AreEqual(Obj.DateTime, Obj2.DateTime);
			Assert.AreEqual(Obj.TimeSpan, Obj2.TimeSpan);
			Assert.AreEqual(Obj.Guid, Obj2.Guid);
			Assert.AreEqual(Obj.NormalEnum, Obj2.NormalEnum);
			Assert.AreEqual(Obj.FlagsEnum, Obj2.FlagsEnum);
		}

		[Test]
		public void Test_07_NullableArrays()
		{
			NullableArrays Obj = new NullableArrays();

			Obj.Boolean = new bool?[] { true, null, false };
			Obj.Byte = new byte?[] { 1, null, 3 };
			Obj.Short = new short?[] { 1, null, 3 };
			Obj.Int = new int?[] { 1, null, 3 };
			Obj.Long = new long?[] { 1, null, 3 };
			Obj.SByte = new sbyte?[] { 1, null, 3 };
			Obj.UShort = new ushort?[] { 1, null, 3 };
			Obj.UInt = new uint?[] { 1, null, 3 };
			Obj.ULong = new ulong?[] { 1, null, 3 };
			Obj.Char = new char?[] { 'a', 'b', null, '☀' };
			Obj.Decimal = new decimal?[] { 1, null, 3 }; ;
			Obj.Double = new double?[] { 1, null, 3 }; ;
			Obj.Single = new float?[] { 1, null, 3 }; ;
			Obj.DateTime = new DateTime?[] { DateTime.Now, null, DateTime.MinValue, DateTime.MaxValue };
			Obj.TimeSpan = new TimeSpan?[] { DateTime.Now.TimeOfDay, null, TimeSpan.Zero };
			Obj.Guid = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() };
			Obj.NormalEnum = new NormalEnum?[] { NormalEnum.Option3, null, NormalEnum.Option4 };
			Obj.FlagsEnum = new FlagsEnum?[] { FlagsEnum.Option1 | FlagsEnum.Option4, null, FlagsEnum.Option3 };

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(NullableArrays));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			NullableArrays Obj2 = (NullableArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Boolean, Obj2.Boolean);
			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
			Assert.AreEqual(Obj.Char, Obj2.Char);
			Assert.AreEqual(Obj.Decimal, Obj2.Decimal);
			Assert.AreEqual(Obj.Double, Obj2.Double);
			Assert.AreEqual(Obj.Single, Obj2.Single);
			Assert.AreEqual(Obj.DateTime, Obj2.DateTime);
			Assert.AreEqual(Obj.TimeSpan, Obj2.TimeSpan);
			Assert.AreEqual(Obj.Guid, Obj2.Guid);
			Assert.AreEqual(Obj.NormalEnum, Obj2.NormalEnum);
			Assert.AreEqual(Obj.FlagsEnum, Obj2.FlagsEnum);
		}
		[Test]
		public void Test_08_Embedded()
		{
			Container Obj = new Container();

			Obj.Embedded = new Embedded();
			Obj.Embedded.Byte = 10;
			Obj.Embedded.Short = 1000;
			Obj.Embedded.Int = 10000000;
			Obj.EmbeddedNull = null;
			Obj.MultipleEmbedded = new Embedded[] { new Embedded(), new Embedded(), new Embedded() };
			Obj.MultipleEmbedded[0].Byte = 20;
			Obj.MultipleEmbedded[0].Short = 2000;
			Obj.MultipleEmbedded[0].Int = 20000000;
			Obj.MultipleEmbedded[1].Byte = 30;
			Obj.MultipleEmbedded[1].Short = 3000;
			Obj.MultipleEmbedded[1].Int = 30000000;
			Obj.MultipleEmbedded[2].Byte = 40;
			Obj.MultipleEmbedded[2].Short = 4000;
			Obj.MultipleEmbedded[2].Int = 40000000;
			Obj.MultipleEmbeddedNullable = new Embedded[] { new Embedded(), null, new Embedded() };
			Obj.MultipleEmbeddedNullable[0].Byte = 20;
			Obj.MultipleEmbeddedNullable[0].Short = 2000;
			Obj.MultipleEmbeddedNullable[0].Int = 20000000;
			Obj.MultipleEmbeddedNullable[2].Byte = 40;
			Obj.MultipleEmbeddedNullable[2].Short = 4000;
			Obj.MultipleEmbeddedNullable[2].Int = 40000000;
			Obj.MultipleEmbeddedNull = null;

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Container));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			Container Obj2 = (Container)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Embedded.Byte, Obj2.Embedded.Byte);
			Assert.AreEqual(Obj.Embedded.Short, Obj2.Embedded.Short);
			Assert.AreEqual(Obj.Embedded.Int, Obj2.Embedded.Int);
			Assert.AreEqual(Obj.EmbeddedNull, Obj2.EmbeddedNull);
			Assert.AreEqual(Obj.MultipleEmbedded[0].Byte, Obj2.MultipleEmbedded[0].Byte);
			Assert.AreEqual(Obj.MultipleEmbedded[0].Short, Obj2.MultipleEmbedded[0].Short);
			Assert.AreEqual(Obj.MultipleEmbedded[0].Int, Obj2.MultipleEmbedded[0].Int);
			Assert.AreEqual(Obj.MultipleEmbedded[1].Byte, Obj2.MultipleEmbedded[1].Byte);
			Assert.AreEqual(Obj.MultipleEmbedded[1].Short, Obj2.MultipleEmbedded[1].Short);
			Assert.AreEqual(Obj.MultipleEmbedded[1].Int, Obj2.MultipleEmbedded[1].Int);
			Assert.AreEqual(Obj.MultipleEmbedded[2].Byte, Obj2.MultipleEmbedded[2].Byte);
			Assert.AreEqual(Obj.MultipleEmbedded[2].Short, Obj2.MultipleEmbedded[2].Short);
			Assert.AreEqual(Obj.MultipleEmbedded[2].Int, Obj2.MultipleEmbedded[2].Int);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[0].Byte, Obj2.MultipleEmbeddedNullable[0].Byte);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[0].Short, Obj2.MultipleEmbeddedNullable[0].Short);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[0].Int, Obj2.MultipleEmbeddedNullable[0].Int);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[1], Obj2.MultipleEmbeddedNullable[1]);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[2].Byte, Obj2.MultipleEmbeddedNullable[2].Byte);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[2].Short, Obj2.MultipleEmbeddedNullable[2].Short);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[2].Int, Obj2.MultipleEmbeddedNullable[2].Int);
			Assert.AreEqual(Obj.MultipleEmbeddedNull, Obj2.MultipleEmbeddedNull);
		}

		[Test]
		public void Test_09_ObjectIdString()
		{
			ObjectIdString Obj = new ObjectIdString();

			Obj.Value = 0x12345678;

			Assert.IsTrue(string.IsNullOrEmpty(Obj.ObjectId));

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(ObjectIdString));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(string.IsNullOrEmpty(Obj.ObjectId));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			ObjectIdString Obj2 = (ObjectIdString)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		[Test]
		public void Test_10_ObjectIdByteArray()
		{
			ObjectIdByteArray Obj = new ObjectIdByteArray();

			Obj.Value = 0x12345678;

			Assert.IsNull(Obj.ObjectId);

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(ObjectIdByteArray));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsNotNull(Obj.ObjectId);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			ObjectIdByteArray Obj2 = (ObjectIdByteArray)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		[Test]
		public void Test_11_LocalTypeName()
		{
			LocalNameSubclass1 Obj1 = new LocalNameSubclass1();

			Obj1.Name = "Obj1";
			Obj1.Value = 0x12345678;

			LocalNameSubclass2 Obj2 = new LocalNameSubclass2();

			Obj2.Name = "Obj2";
			Obj2.Value = "Hello";

			Assert.IsTrue(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsTrue(Obj2.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S1 = this.provider.GetObjectSerializer(typeof(LocalNameSubclass1));
			IObjectSerializer S2 = this.provider.GetObjectSerializer(typeof(LocalNameSubclass2));
			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(LocalNameBase));
			BinarySerializer Writer1 = new BinarySerializer(Encoding.UTF8, true);
			BinarySerializer Writer2 = new BinarySerializer(Encoding.UTF8, true);

			S1.Serialize(Writer1, false, false, Obj1);
			S2.Serialize(Writer2, false, false, Obj2);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			BinaryDeserializer Reader1 = new BinaryDeserializer(Encoding.UTF8, Data1, true);
			BinaryDeserializer Reader2 = new BinaryDeserializer(Encoding.UTF8, Data2, true);

			LocalNameSubclass1 Obj12 = (LocalNameSubclass1)S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			LocalNameSubclass2 Obj22 = (LocalNameSubclass2)S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj1.Name, Obj12.Name);
			Assert.AreEqual(Obj1.Value, Obj12.Value);
			Assert.AreEqual(Obj1.ObjectId, Obj12.ObjectId);
			Assert.AreEqual(Obj2.Name, Obj22.Name);
			Assert.AreEqual(Obj2.Value, Obj22.Value);
			Assert.AreEqual(Obj2.ObjectId, Obj22.ObjectId);
		}

		[Test]
		public void Test_12_FullTypeName()
		{
			FullNameSubclass1 Obj1 = new FullNameSubclass1();

			Obj1.Name = "Obj1";
			Obj1.Value = 0x12345678;

			FullNameSubclass2 Obj2 = new FullNameSubclass2();

			Obj2.Name = "Obj2";
			Obj2.Value = "Hello";

			Assert.IsTrue(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsTrue(Obj2.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S1 = this.provider.GetObjectSerializer(typeof(FullNameSubclass1));
			IObjectSerializer S2 = this.provider.GetObjectSerializer(typeof(FullNameSubclass2));
			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(FullNameBase));
			BinarySerializer Writer1 = new BinarySerializer(Encoding.UTF8, true);
			BinarySerializer Writer2 = new BinarySerializer(Encoding.UTF8, true);

			S1.Serialize(Writer1, false, false, Obj1);
			S2.Serialize(Writer2, false, false, Obj2);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			BinaryDeserializer Reader1 = new BinaryDeserializer(Encoding.UTF8, Data1, true);
			BinaryDeserializer Reader2 = new BinaryDeserializer(Encoding.UTF8, Data2, true);

			FullNameSubclass1 Obj12 = (FullNameSubclass1)S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			FullNameSubclass2 Obj22 = (FullNameSubclass2)S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj1.Name, Obj12.Name);
			Assert.AreEqual(Obj1.Value, Obj12.Value);
			Assert.AreEqual(Obj1.ObjectId, Obj12.ObjectId);
			Assert.AreEqual(Obj2.Name, Obj22.Name);
			Assert.AreEqual(Obj2.Value, Obj22.Value);
			Assert.AreEqual(Obj2.ObjectId, Obj22.ObjectId);
		}

		[Test]
		public void Test_13_CollectionTest()
		{
			CollectionTest Obj = new CollectionTest();

			Obj.S1 = "Today, there will be a lot of ☀.";
			Obj.S2 = "Hello world.";
			Obj.S3 = "Testing, testing...";

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(CollectionTest));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data, true);

			CollectionTest Obj2 = (CollectionTest)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.S1, Obj2.S1);
			Assert.AreEqual(Obj.S2, Obj2.S2);
			Assert.AreEqual(Obj.S3, Obj2.S3);
		}

		// TODO: Objects, by reference, nullable (incl. null strings, arrays)
		// TODO: Generic object reader/writer (with no type knowledge, for batch operations). If type not found when reading: Return generic object.
		// TODO: Binary length (to skip block)
	}
}
