using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Serialization;

#if !LW
using Waher.Persistence.Files.Test.Classes;

namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
using Waher.Persistence.FilesLW.Test.Classes;

namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class DBFilesObjectSerializationTests
	{
		private static FilesProvider provider;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext Context)
		{
			DBFilesBTreeTests.DeleteFiles();

#if LW
			provider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000);
#else
			provider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true, true);
#endif
			await provider.GetFile("Default");
			await provider.GetFile("Test");
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (provider != null)
			{
				provider.Dispose();
				provider = null;
			}
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_01_SimpleObject()
		{
			Simple Obj = new Simple()
			{
				Boolean1 = true,
				Boolean2 = false,
				Byte = 15,
				Short = -1234,
				Int = -23456789,
				Long = -345456456456456345,
				SByte = -45,
				UShort = 23456,
				UInt = 334534564,
				ULong = 4345345345345345,
				Char = '☀',
				Decimal = 12345.6789M,
				Double = 12345.6789,
				Single = 12345.6789f,
				String = "Today, there will be a lot of ☀.",
				DateTime = DateTime.Now,
				DateTimeOffset = DateTimeOffset.Now,
				Guid = Guid.NewGuid(),
				NormalEnum = NormalEnum.Option3,
				FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4,
				CIString = "Hello World!"
			};

			Obj.TimeSpan = Obj.DateTime.TimeOfDay;

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Simple));
			object Value;

			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean1", Obj));
			AssertEx.Same(Obj.Boolean1, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean2", Obj));
			AssertEx.Same(Obj.Boolean2, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Byte", Obj));
			AssertEx.Same(Obj.Byte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("SByte", Obj));
			AssertEx.Same(Obj.SByte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Char", Obj));
			AssertEx.Same(Obj.Char, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Decimal", Obj));
			AssertEx.Same(Obj.Decimal, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Double", Obj));
			AssertEx.Same(Obj.Double, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Single", Obj));
			AssertEx.Same(Obj.Single, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("String", Obj));
			AssertEx.Same(Obj.String, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("DateTime", Obj));
			AssertEx.Same(Obj.DateTime, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("DateTimeOffset", Obj));
			AssertEx.Same(Obj.DateTimeOffset, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("TimeSpan", Obj));
			AssertEx.Same(Obj.TimeSpan, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Guid", Obj));
			AssertEx.Same(Obj.Guid, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("NormalEnum", Obj));
			AssertEx.Same(Obj.NormalEnum, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("FlagsEnum", Obj));
			AssertEx.Same(Obj.FlagsEnum, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("CIString", Obj));
			AssertEx.Same(Obj.CIString, Value);

			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			Console.Out.WriteLine();
			Console.Out.WriteLine();

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			Simple Obj2 = (Simple)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Simple)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		internal static void AssertEqual(Simple Obj, Simple Obj2)
		{
			AssertEx.Same(Obj.Boolean1, Obj2.Boolean1);
			AssertEx.Same(Obj.Boolean2, Obj2.Boolean2);
			AssertEx.Same(Obj.Byte, Obj2.Byte);
			AssertEx.Same(Obj.Short, Obj2.Short);
			AssertEx.Same(Obj.Int, Obj2.Int);
			AssertEx.Same(Obj.Long, Obj2.Long);
			AssertEx.Same(Obj.SByte, Obj2.SByte);
			AssertEx.Same(Obj.UShort, Obj2.UShort);
			AssertEx.Same(Obj.UInt, Obj2.UInt);
			AssertEx.Same(Obj.ULong, Obj2.ULong);
			AssertEx.Same(Obj.Char, Obj2.Char);
			AssertEx.Same(Obj.Decimal, Obj2.Decimal);
			AssertEx.Same(Obj.Double, Obj2.Double);
			AssertEx.Same(Obj.Single, Obj2.Single);
			AssertEx.Same(Obj.String, Obj2.String);
			AssertEx.Same(Obj.DateTime, Obj2.DateTime);
			AssertEx.Same(Obj.DateTimeOffset, Obj2.DateTimeOffset);
			AssertEx.Same(Obj.TimeSpan, Obj2.TimeSpan);
			AssertEx.Same(Obj.Guid, Obj2.Guid);
			AssertEx.Same(Obj.NormalEnum, Obj2.NormalEnum);
			AssertEx.Same(Obj.FlagsEnum, Obj2.FlagsEnum);
			AssertEx.Same(Obj.CIString, Obj2.CIString);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		internal static void AssertEqual(Simple Obj, GenericObject GenObj)
		{
			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean1, GenObj["Boolean1"]);
			AssertEx.Same(Obj.Boolean2, GenObj["Boolean2"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(Obj.String, GenObj["String"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.DateTimeOffset, GenObj["DateTimeOffset"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			AssertEx.Same((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);
			AssertEx.Same(Obj.CIString, GenObj["CIString"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
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

		private void AssertBinaryLength(byte[] Data, IDeserializer Reader)
		{
			Reader.Restart(Data, 0);

			Reader.ReadGuid();
			ulong Len = Reader.ReadVariableLengthUInt64();

			AssertEx.Same(Data.Length - 16 - this.VariableULongLen(Len), Len);
		}

		private int VariableULongLen(ulong Len)
		{
			int NrBytes = 0;

			do
			{
				NrBytes++;
				Len >>= 7;
			}
			while (Len > 0);

			return NrBytes;
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_02_Nullable1()
		{
			Classes.Nullable Obj = new Classes.Nullable()
			{
				Boolean1 = true,
				Byte = 15,
				Int = -23456789,
				SByte = -45,
				UInt = 334534564,
				Char = '☀',
				Double = 12345.6789,
				String = "Today, there will be a lot of ☀.",
				TimeSpan = DateTime.Now.TimeOfDay,
				NormalEnum = NormalEnum.Option3
			};


			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Classes.Nullable));
			object Value;
			
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean1", Obj));
			AssertEx.Same(Obj.Boolean1, Value);
			Value = await S.TryGetFieldValue("Boolean2", Obj);
			AssertEx.Same(Obj.Boolean2, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Byte", Obj));
			AssertEx.Same(Obj.Byte, Value);
			Value = await S.TryGetFieldValue("Short", Obj);
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Value = await S.TryGetFieldValue("Long", Obj);
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("SByte", Obj));
			AssertEx.Same(Obj.SByte, Value);
			Value = await S.TryGetFieldValue("UShort", Obj);
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Value = await S.TryGetFieldValue("ULong", Obj);
			AssertEx.Same(Obj.ULong, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Char", Obj));
			AssertEx.Same(Obj.Char, Value);
			Value = await S.TryGetFieldValue("Decimal", Obj);
			AssertEx.Same(Obj.Decimal, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Double", Obj));
			AssertEx.Same(Obj.Double, Value);
			Value = await S.TryGetFieldValue("Single", Obj);
			AssertEx.Same(Obj.Single, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("String", Obj));
			AssertEx.Same(Obj.String, Value);
			Value = await S.TryGetFieldValue("DateTime", Obj);
			AssertEx.Same(Obj.DateTime, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("TimeSpan", Obj));
			AssertEx.Same(Obj.TimeSpan, Value);
			Value = await S.TryGetFieldValue("Guid", Obj);
			AssertEx.Same(Obj.Guid, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("NormalEnum", Obj));
			AssertEx.Same(Obj.NormalEnum, Value);
			Value = await S.TryGetFieldValue("FlagsEnum", Obj);
			AssertEx.Same(Obj.FlagsEnum, Value);

			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			Classes.Nullable Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean1, GenObj["Boolean1"]);
			AssertEx.Same(Obj.Boolean2, GenObj["Boolean2"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(Obj.String, GenObj["String"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			AssertEx.Same(null, GenObj["FlagsEnum"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(Classes.Nullable Obj, Classes.Nullable Obj2)
		{
			AssertEx.Same(Obj.Boolean1, Obj2.Boolean1);
			AssertEx.Same(Obj.Boolean2, Obj2.Boolean2);
			AssertEx.Same(Obj.Byte, Obj2.Byte);
			AssertEx.Same(Obj.Short, Obj2.Short);
			AssertEx.Same(Obj.Int, Obj2.Int);
			AssertEx.Same(Obj.Long, Obj2.Long);
			AssertEx.Same(Obj.SByte, Obj2.SByte);
			AssertEx.Same(Obj.UShort, Obj2.UShort);
			AssertEx.Same(Obj.UInt, Obj2.UInt);
			AssertEx.Same(Obj.ULong, Obj2.ULong);
			AssertEx.Same(Obj.Char, Obj2.Char);
			AssertEx.Same(Obj.Decimal, Obj2.Decimal);
			AssertEx.Same(Obj.Double, Obj2.Double);
			AssertEx.Same(Obj.Single, Obj2.Single);
			AssertEx.Same(Obj.String, Obj2.String);
			AssertEx.Same(Obj.DateTime, Obj2.DateTime);
			AssertEx.Same(Obj.TimeSpan, Obj2.TimeSpan);
			AssertEx.Same(Obj.Guid, Obj2.Guid);
			AssertEx.Same(Obj.NormalEnum, Obj2.NormalEnum);
			AssertEx.Same(Obj.FlagsEnum, Obj2.FlagsEnum);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_03_Nullable2()
		{
			Classes.Nullable Obj = new Classes.Nullable()
			{
				Boolean2 = false,
				Short = -1234,
				Long = -345456456456456345,
				UShort = 23456,
				ULong = 4345345345345345,
				Decimal = 12345.6789M,
				Single = 12345.6789f,
				DateTime = DateTime.Now,
				Guid = Guid.NewGuid(),
				FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Classes.Nullable));
			object Value;

			Value = await S.TryGetFieldValue("Boolean1", Obj);
			AssertEx.Same(Obj.Boolean1, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean2", Obj));
			AssertEx.Same(Obj.Boolean2, Value);
			Value = await S.TryGetFieldValue("Byte", Obj);
			AssertEx.Same(Obj.Byte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Value = await S.TryGetFieldValue("Int", Obj);
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Value = await S.TryGetFieldValue("SByte", Obj);
			AssertEx.Same(Obj.SByte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Value = await S.TryGetFieldValue("UInt", Obj);
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);
			Value = await S.TryGetFieldValue("Char", Obj);
			AssertEx.Same(Obj.Char, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Decimal", Obj));
			AssertEx.Same(Obj.Decimal, Value);
			Value = await S.TryGetFieldValue("Double", Obj);
			AssertEx.Same(Obj.Double, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Single", Obj));
			AssertEx.Same(Obj.Single, Value);
			Value = await S.TryGetFieldValue("String", Obj);
			AssertEx.Same(Obj.String, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("DateTime", Obj));
			AssertEx.Same(Obj.DateTime, Value);
			Value = await S.TryGetFieldValue("TimeSpan", Obj);
			AssertEx.Same(Obj.TimeSpan, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Guid", Obj));
			AssertEx.Same(Obj.Guid, Value);
			Value = await S.TryGetFieldValue("NormalEnum", Obj);
			AssertEx.Same(Obj.NormalEnum, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("FlagsEnum", Obj));
			AssertEx.Same(Obj.FlagsEnum, Value);

			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			Classes.Nullable Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean1, GenObj["Boolean1"]);
			AssertEx.Same(Obj.Boolean2, GenObj["Boolean2"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(Obj.String, GenObj["String"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(null, GenObj["NormalEnum"]);
			AssertEx.Same((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_04_Default1()
		{
			Default Obj = new Default()
			{
				Short = -1234,
				Long = -345456456456456345,
				UShort = 23456,
				ULong = 4345345345345345,
				Decimal = 12345.6789M,
				Single = 12345.6789f,
				DateTime = DateTime.Now,
				Guid = Guid.NewGuid(),
				FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4
			};


			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Default));
			object Value;
			
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean1", Obj));
			AssertEx.Same(Obj.Boolean1, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean2", Obj));
			AssertEx.Same(Obj.Boolean2, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Byte", Obj));
			AssertEx.Same(Obj.Byte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("SByte", Obj));
			AssertEx.Same(Obj.SByte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Char", Obj));
			AssertEx.Same(Obj.Char, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Decimal", Obj));
			AssertEx.Same(Obj.Decimal, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Double", Obj));
			AssertEx.Same(Obj.Double, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Single", Obj));
			AssertEx.Same(Obj.Single, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("String", Obj));
			AssertEx.Same(Obj.String, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("DateTime", Obj));
			AssertEx.Same(Obj.DateTime, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("TimeSpan", Obj));
			AssertEx.Same(Obj.TimeSpan, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Guid", Obj));
			AssertEx.Same(Obj.Guid, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("NormalEnum", Obj));
			AssertEx.Same(Obj.NormalEnum, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("FlagsEnum", Obj));
			AssertEx.Same(Obj.FlagsEnum, Value);

			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			Default Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(null, GenObj["Boolean1"]);
			AssertEx.Same(null, GenObj["Boolean2"]);
			AssertEx.Same(null, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(null, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(null, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(null, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(null, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(null, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(null, GenObj["String"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(null, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(null, GenObj["NormalEnum"]);
			AssertEx.Same((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);
			AssertEx.Same(null, GenObj["String2"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		internal static void AssertEqual(Default Obj, Default Obj2)
		{
			AssertEx.Same(Obj.Boolean1, Obj2.Boolean1);
			AssertEx.Same(Obj.Boolean2, Obj2.Boolean2);
			AssertEx.Same(Obj.Byte, Obj2.Byte);
			AssertEx.Same(Obj.Short, Obj2.Short);
			AssertEx.Same(Obj.Int, Obj2.Int);
			AssertEx.Same(Obj.Long, Obj2.Long);
			AssertEx.Same(Obj.SByte, Obj2.SByte);
			AssertEx.Same(Obj.UShort, Obj2.UShort);
			AssertEx.Same(Obj.UInt, Obj2.UInt);
			AssertEx.Same(Obj.ULong, Obj2.ULong);
			AssertEx.Same(Obj.Char, Obj2.Char);
			AssertEx.Same(Obj.Decimal, Obj2.Decimal);
			AssertEx.Same(Obj.Double, Obj2.Double);
			AssertEx.Same(Obj.Single, Obj2.Single);
			AssertEx.Same(Obj.String, Obj2.String);
			AssertEx.Same(Obj.DateTime, Obj2.DateTime);
			AssertEx.Same(Obj.TimeSpan, Obj2.TimeSpan);
			AssertEx.Same(Obj.Guid, Obj2.Guid);
			AssertEx.Same(Obj.NormalEnum, Obj2.NormalEnum);
			AssertEx.Same(Obj.FlagsEnum, Obj2.FlagsEnum);
			AssertEx.Same(Obj.String2, Obj2.String2);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_05_Default2()
		{
			Default Obj = new Default()
			{
				Boolean1 = false,
				Boolean2 = true,
				Byte = 15,
				Int = -23456789,
				SByte = -45,
				UInt = 334534564,
				Char = '☀',
				Double = 12345.6789,
				String = "Today, there will be a lot of ☀.",
				TimeSpan = DateTime.Now.TimeOfDay,
				NormalEnum = NormalEnum.Option3,
				String2 = "Hello"
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Default));
			object Value;

			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean1", Obj));
			AssertEx.Same(Obj.Boolean1, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean2", Obj));
			AssertEx.Same(Obj.Boolean2, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Byte", Obj));
			AssertEx.Same(Obj.Byte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("SByte", Obj));
			AssertEx.Same(Obj.SByte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Char", Obj));
			AssertEx.Same(Obj.Char, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Decimal", Obj));
			AssertEx.Same(Obj.Decimal, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Double", Obj));
			AssertEx.Same(Obj.Double, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Single", Obj));
			AssertEx.Same(Obj.Single, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("String", Obj));
			AssertEx.Same(Obj.String, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("DateTime", Obj));
			AssertEx.Same(Obj.DateTime, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("TimeSpan", Obj));
			AssertEx.Same(Obj.TimeSpan, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Guid", Obj));
			AssertEx.Same(Obj.Guid, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("NormalEnum", Obj));
			AssertEx.Same(Obj.NormalEnum, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("FlagsEnum", Obj));
			AssertEx.Same(Obj.FlagsEnum, Value);

			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			Default Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean1, GenObj["Boolean1"]);
			AssertEx.Same(Obj.Boolean2, GenObj["Boolean2"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(null, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(null, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(null, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(null, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(null, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(null, GenObj["Single"]);
			AssertEx.Same(Obj.String, GenObj["String"]);
			AssertEx.Same(null, GenObj["DateTime"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(null, GenObj["Guid"]);
			AssertEx.Same(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			AssertEx.Same(null, GenObj["FlagsEnum"]);
			AssertEx.Same(Obj.String2, GenObj["String2"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_06_SimpleArrays()
		{
			SimpleArrays Obj = new SimpleArrays()
			{
				Boolean = new bool[] { true, false },
				Byte = new byte[] { 1, 2, 3 },
				Short = new short[] { 1, 2, 3 },
				Int = new int[] { 1, 2, 3 },
				Long = new long[] { 1, 2, 3 },
				SByte = new sbyte[] { 1, 2, 3 },
				UShort = new ushort[] { 1, 2, 3 },
				UInt = new uint[] { 1, 2, 3 },
				ULong = new ulong[] { 1, 2, 3 },
				Char = new char[] { 'a', 'b', 'c', '☀' },
				Decimal = new decimal[] { 1, 2, 3 },
				Double = new double[] { 1, 2, 3 },
				Single = new float[] { 1, 2, 3 },
				String = new string[] { "a", "b", "c", "Today, there will be a lot of ☀." },
				DateTime = new DateTime[] { DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue },
				TimeSpan = new TimeSpan[] { DateTime.Now.TimeOfDay, TimeSpan.Zero },
				Guid = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
				NormalEnum = new NormalEnum[] { NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4 },
				FlagsEnum = new FlagsEnum[] { FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3 },
				CIStrings = new CaseInsensitiveString[] { "a", "b", "c", "Today, there will be a lot of ☀." }
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(SimpleArrays));
			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			SimpleArrays Obj2 = (SimpleArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean, GenObj["Boolean"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(Obj.String, GenObj["String"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(new string[] { Obj.NormalEnum[0].ToString(), Obj.NormalEnum[1].ToString(), Obj.NormalEnum[2].ToString() }, GenObj["NormalEnum"]);
			AssertEx.Same(new int[] { (int)Obj.FlagsEnum[0], (int)Obj.FlagsEnum[1] }, GenObj["FlagsEnum"]);
			AssertEx.Same(Obj.CIStrings, GenObj["CIStrings"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (SimpleArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(SimpleArrays Obj, SimpleArrays Obj2)
		{
			AssertEx.Same(Obj.Boolean, Obj2.Boolean);
			AssertEx.Same(Obj.Byte, Obj2.Byte);
			AssertEx.Same(Obj.Short, Obj2.Short);
			AssertEx.Same(Obj.Int, Obj2.Int);
			AssertEx.Same(Obj.Long, Obj2.Long);
			AssertEx.Same(Obj.SByte, Obj2.SByte);
			AssertEx.Same(Obj.UShort, Obj2.UShort);
			AssertEx.Same(Obj.UInt, Obj2.UInt);
			AssertEx.Same(Obj.ULong, Obj2.ULong);
			AssertEx.Same(Obj.Char, Obj2.Char);
			AssertEx.Same(Obj.Decimal, Obj2.Decimal);
			AssertEx.Same(Obj.Double, Obj2.Double);
			AssertEx.Same(Obj.Single, Obj2.Single);
			AssertEx.Same(Obj.String, Obj2.String);
			AssertEx.Same(Obj.DateTime, Obj2.DateTime);
			AssertEx.Same(Obj.TimeSpan, Obj2.TimeSpan);
			AssertEx.Same(Obj.Guid, Obj2.Guid);
			AssertEx.Same(Obj.NormalEnum, Obj2.NormalEnum);
			AssertEx.Same(Obj.FlagsEnum, Obj2.FlagsEnum);
			AssertEx.Same(Obj.CIStrings, Obj2.CIStrings);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_07_NullableArrays()
		{
			NullableArrays Obj = new NullableArrays()
			{
				Boolean = new bool?[] { true, null, false },
				Byte = new byte?[] { 1, null, 3 },
				Short = new short?[] { 1, null, 3 },
				Int = new int?[] { 1, null, 3 },
				Long = new long?[] { 1, null, 3 },
				SByte = new sbyte?[] { 1, null, 3 },
				UShort = new ushort?[] { 1, null, 3 },
				UInt = new uint?[] { 1, null, 3 },
				ULong = new ulong?[] { 1, null, 3 },
				Char = new char?[] { 'a', 'b', null, '☀' },
				Decimal = new decimal?[] { 1, null, 3 },
				Double = new double?[] { 1, null, 3 },
				Single = new float?[] { 1, null, 3 },
				DateTime = new DateTime?[] { DateTime.Now, null, DateTime.MinValue, DateTime.MaxValue },
				TimeSpan = new TimeSpan?[] { DateTime.Now.TimeOfDay, null, TimeSpan.Zero },
				Guid = new Guid?[] { Guid.NewGuid(), null, Guid.NewGuid() },
				NormalEnum = new NormalEnum?[] { NormalEnum.Option3, null, NormalEnum.Option4 },
				FlagsEnum = new FlagsEnum?[] { FlagsEnum.Option1 | FlagsEnum.Option4, null, FlagsEnum.Option3 }
			};


			IObjectSerializer S = await provider.GetObjectSerializer(typeof(NullableArrays));
			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			NullableArrays Obj2 = (NullableArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean, GenObj["Boolean"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(new string[] { Obj.NormalEnum[0].ToString(), null, Obj.NormalEnum[2].ToString() }, GenObj["NormalEnum"]);
			AssertEx.Same(new int?[] { (int)Obj.FlagsEnum[0], null, (int)Obj.FlagsEnum[2] }, GenObj["FlagsEnum"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (NullableArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(NullableArrays Obj, NullableArrays Obj2)
		{
			AssertEx.Same(Obj.Boolean, Obj2.Boolean);
			AssertEx.Same(Obj.Byte, Obj2.Byte);
			AssertEx.Same(Obj.Short, Obj2.Short);
			AssertEx.Same(Obj.Int, Obj2.Int);
			AssertEx.Same(Obj.Long, Obj2.Long);
			AssertEx.Same(Obj.SByte, Obj2.SByte);
			AssertEx.Same(Obj.UShort, Obj2.UShort);
			AssertEx.Same(Obj.UInt, Obj2.UInt);
			AssertEx.Same(Obj.ULong, Obj2.ULong);
			AssertEx.Same(Obj.Char, Obj2.Char);
			AssertEx.Same(Obj.Decimal, Obj2.Decimal);
			AssertEx.Same(Obj.Double, Obj2.Double);
			AssertEx.Same(Obj.Single, Obj2.Single);
			AssertEx.Same(Obj.DateTime, Obj2.DateTime);
			AssertEx.Same(Obj.TimeSpan, Obj2.TimeSpan);
			AssertEx.Same(Obj.Guid, Obj2.Guid);
			AssertEx.Same(Obj.NormalEnum, Obj2.NormalEnum);
			AssertEx.Same(Obj.FlagsEnum, Obj2.FlagsEnum);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_08_Embedded()
		{
			Container Obj = new Container()
			{
				Embedded = new Embedded()
				{
					Byte = 10,
					Short = 1000,
					Int = 10000000
				},
				EmbeddedNull = null,
				MultipleEmbedded = new Embedded[] 
				{
					new Embedded()
					{
						Byte = 20,
						Short = 2000,
						Int = 20000000
					},
					new Embedded()
					{
						Byte = 30,
						Short = 3000,
						Int = 30000000
					},
					new Embedded()
					{
						Byte = 40,
						Short = 4000,
						Int = 40000000
					}
				},
				MultipleEmbeddedNullable = new Embedded[] 
				{
					new Embedded()
					{
						Byte = 20,
						Short = 2000,
						Int = 20000000
					},
					null,
					new Embedded()
					{
						Byte = 40,
						Short = 4000,
						Int = 40000000
					}
				},
				MultipleEmbeddedNull = null
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Container));
			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			Container Obj2 = (Container)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Embedded.Byte, ((GenericObject)GenObj["Embedded"])["Byte"]);
			AssertEx.Same(Obj.Embedded.Short, ((GenericObject)GenObj["Embedded"])["Short"]);
			AssertEx.Same(Obj.Embedded.Int, ((GenericObject)GenObj["Embedded"])["Int"]);
			AssertEx.Same(Obj.EmbeddedNull, GenObj["EmbeddedNull"]);
			AssertEx.Same(Obj.MultipleEmbedded[0].Byte, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0))["Byte"]);
			AssertEx.Same(Obj.MultipleEmbedded[0].Short, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0))["Short"]);
			AssertEx.Same(Obj.MultipleEmbedded[0].Int, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0))["Int"]);
			AssertEx.Same(Obj.MultipleEmbedded[1].Byte, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1))["Byte"]);
			AssertEx.Same(Obj.MultipleEmbedded[1].Short, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1))["Short"]);
			AssertEx.Same(Obj.MultipleEmbedded[1].Int, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1))["Int"]);
			AssertEx.Same(Obj.MultipleEmbedded[2].Byte, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2))["Byte"]);
			AssertEx.Same(Obj.MultipleEmbedded[2].Short, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2))["Short"]);
			AssertEx.Same(Obj.MultipleEmbedded[2].Int, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2))["Int"]);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[0].Byte, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0))["Byte"]);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[0].Short, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0))["Short"]);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[0].Int, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0))["Int"]);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[1], ((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(1));
			AssertEx.Same(Obj.MultipleEmbeddedNullable[2].Byte, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2))["Byte"]);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[2].Short, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2))["Short"]);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[2].Int, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2))["Int"]);
			AssertEx.Same(Obj.MultipleEmbeddedNull, GenObj["MultipleEmbeddedNull"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Container)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(Container Obj, Container Obj2)
		{
			AssertEx.Same(Obj.Embedded.Byte, Obj2.Embedded.Byte);
			AssertEx.Same(Obj.Embedded.Short, Obj2.Embedded.Short);
			AssertEx.Same(Obj.Embedded.Int, Obj2.Embedded.Int);
			AssertEx.Same(Obj.EmbeddedNull, Obj2.EmbeddedNull);
			AssertEx.Same(Obj.MultipleEmbedded[0].Byte, Obj2.MultipleEmbedded[0].Byte);
			AssertEx.Same(Obj.MultipleEmbedded[0].Short, Obj2.MultipleEmbedded[0].Short);
			AssertEx.Same(Obj.MultipleEmbedded[0].Int, Obj2.MultipleEmbedded[0].Int);
			AssertEx.Same(Obj.MultipleEmbedded[1].Byte, Obj2.MultipleEmbedded[1].Byte);
			AssertEx.Same(Obj.MultipleEmbedded[1].Short, Obj2.MultipleEmbedded[1].Short);
			AssertEx.Same(Obj.MultipleEmbedded[1].Int, Obj2.MultipleEmbedded[1].Int);
			AssertEx.Same(Obj.MultipleEmbedded[2].Byte, Obj2.MultipleEmbedded[2].Byte);
			AssertEx.Same(Obj.MultipleEmbedded[2].Short, Obj2.MultipleEmbedded[2].Short);
			AssertEx.Same(Obj.MultipleEmbedded[2].Int, Obj2.MultipleEmbedded[2].Int);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[0].Byte, Obj2.MultipleEmbeddedNullable[0].Byte);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[0].Short, Obj2.MultipleEmbeddedNullable[0].Short);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[0].Int, Obj2.MultipleEmbeddedNullable[0].Int);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[1], Obj2.MultipleEmbeddedNullable[1]);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[2].Byte, Obj2.MultipleEmbeddedNullable[2].Byte);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[2].Short, Obj2.MultipleEmbeddedNullable[2].Short);
			AssertEx.Same(Obj.MultipleEmbeddedNullable[2].Int, Obj2.MultipleEmbeddedNullable[2].Int);
			AssertEx.Same(Obj.MultipleEmbeddedNull, Obj2.MultipleEmbeddedNull);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_09_ObjectIdString()
		{
			ObjectIdString Obj = new ObjectIdString()
			{
				Value = 0x12345678
			};

			Assert.IsTrue(string.IsNullOrEmpty(Obj.ObjectId));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ObjectIdString));
			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(string.IsNullOrEmpty(Obj.ObjectId));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			ObjectIdString Obj2 = (ObjectIdString)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Value, GenObj["Value"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId.ToString());

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (ObjectIdString)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(ObjectIdString Obj, ObjectIdString Obj2)
		{
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_10_ObjectIdByteArray()
		{
			ObjectIdByteArray Obj = new ObjectIdByteArray()
			{
				Value = 0x12345678
			};

			Assert.IsNull(Obj.ObjectId);

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ObjectIdByteArray));
			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			Assert.IsNotNull(Obj.ObjectId);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			ObjectIdByteArray Obj2 = (ObjectIdByteArray)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Value, GenObj["Value"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId.ToByteArray());

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (ObjectIdByteArray)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(ObjectIdByteArray Obj, ObjectIdByteArray Obj2)
		{
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_11_LocalTypeName()
		{
			LocalNameSubclass1 Obj1 = new LocalNameSubclass1()
			{
				Name = "Obj1",
				Value = 0x12345678
			};

			LocalNameSubclass2 Obj2 = new LocalNameSubclass2()
			{
				Name = "Obj2",
				Value = "Hello"
			};

			Assert.IsTrue(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsTrue(Obj2.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S1 = await provider.GetObjectSerializer(typeof(LocalNameSubclass1));
			IObjectSerializer S2 = await provider.GetObjectSerializer(typeof(LocalNameSubclass2));
			IObjectSerializer S = await provider.GetObjectSerializer(typeof(LocalNameBase));
			ISerializer Writer1 = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);
			ISerializer Writer2 = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S1.Serialize(Writer1, false, false, Obj1);
			await S2.Serialize(Writer2, false, false, Obj2);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			IDeserializer Reader1 = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data1, uint.MaxValue), Console.Out);
			IDeserializer Reader2 = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data2, uint.MaxValue), Console.Out);

			LocalNameSubclass1 Obj12 = (LocalNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			LocalNameSubclass2 Obj22 = (LocalNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);

			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj1 = (GenericObject)await GS.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			GenericObject GenObj2 = (GenericObject)await GS.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(Obj1.Name, GenObj1["Name"]);
			AssertEx.Same(Obj1.Value, GenObj1["Value"]);
			AssertEx.Same(Obj1.ObjectId, GenObj1.ObjectId);
			AssertEx.Same(Obj2.Name, GenObj2["Name"]);
			AssertEx.Same(Obj2.Value, GenObj2["Value"]);
			AssertEx.Same(Obj2.ObjectId, GenObj2.ObjectId);

			Writer1.Restart();
			Writer2.Restart();

			await GS.Serialize(Writer1, false, false, GenObj1);
			await GS.Serialize(Writer2, false, false, GenObj2);

			Data1 = Writer1.GetSerialization();
			Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			Obj12 = (LocalNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			Obj22 = (LocalNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);
			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);
		}

		private void AssertEqual(LocalNameSubclass1 Obj, LocalNameSubclass1 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		private void AssertEqual(LocalNameSubclass2 Obj, LocalNameSubclass2 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_12_FullTypeName()
		{
			FullNameSubclass1 Obj1 = new FullNameSubclass1()
			{
				Name = "Obj1",
				Value = 0x12345678
			};

			FullNameSubclass2 Obj2 = new FullNameSubclass2()
			{
				Name = "Obj2",
				Value = "Hello"
			};

			Assert.IsTrue(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsTrue(Obj2.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S1 = await provider.GetObjectSerializer(typeof(FullNameSubclass1));
			IObjectSerializer S2 = await provider.GetObjectSerializer(typeof(FullNameSubclass2));
			IObjectSerializer S = await provider.GetObjectSerializer(typeof(FullNameBase));
			ISerializer Writer1 = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);
			ISerializer Writer2 = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S1.Serialize(Writer1, false, false, Obj1);
			await S2.Serialize(Writer2, false, false, Obj2);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			IDeserializer Reader1 = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data1, uint.MaxValue), Console.Out);
			IDeserializer Reader2 = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data2, uint.MaxValue), Console.Out);

			FullNameSubclass1 Obj12 = (FullNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			FullNameSubclass2 Obj22 = (FullNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);

			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj1 = (GenericObject)await GS.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			GenericObject GenObj2 = (GenericObject)await GS.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(Obj1.Name, GenObj1["Name"]);
			AssertEx.Same(Obj1.Value, GenObj1["Value"]);
			AssertEx.Same(Obj1.ObjectId, GenObj1.ObjectId);
			AssertEx.Same(Obj2.Name, GenObj2["Name"]);
			AssertEx.Same(Obj2.Value, GenObj2["Value"]);
			AssertEx.Same(Obj2.ObjectId, GenObj2.ObjectId);

			Writer1.Restart();
			Writer2.Restart();

			await GS.Serialize(Writer1, false, false, GenObj1);
			await GS.Serialize(Writer2, false, false, GenObj2);

			Data1 = Writer1.GetSerialization();
			Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			Obj12 = (FullNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			Obj22 = (FullNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);
			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);
		}

		private void AssertEqual(FullNameSubclass1 Obj, FullNameSubclass1 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		private void AssertEqual(FullNameSubclass2 Obj, FullNameSubclass2 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_13_CollectionTest()
		{
			CollectionTest Obj = new CollectionTest()
			{
				S1 = "Today, there will be a lot of ☀.",
				S2 = "Hello world.",
				S3 = "Testing, testing..."
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(CollectionTest));
			ISerializer Writer = new DebugSerializer(new BinarySerializer(await ((ObjectSerializer)S).CollectionName(Obj), Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(await ((ObjectSerializer)S).CollectionName(Obj), Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			CollectionTest Obj2 = (CollectionTest)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Test");
			AssertEx.Same(Obj.S1, GenObj["S1"]);
			AssertEx.Same(Obj.S2, GenObj["S2"]);
			AssertEx.Same(Obj.S3, GenObj["S3"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (CollectionTest)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(CollectionTest Obj, CollectionTest Obj2)
		{
			AssertEx.Same(Obj.S1, Obj2.S1);
			AssertEx.Same(Obj.S2, Obj2.S2);
			AssertEx.Same(Obj.S3, Obj2.S3);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_14_ArraysOfArrays()
		{
			ArraysOfArrays Obj = new ArraysOfArrays()
			{
				Boolean = new bool[][] { new bool[] { true, false }, new bool[] { false, true } },
				Byte = new byte[][] { new byte[] { 1, 2, 3 }, new byte[] { 2, 3, 4 } },
				Short = new short[][] { new short[] { 1, 2, 3 }, new short[] { 2, 3, 4 } },
				Int = new int[][] { new int[] { 1, 2, 3 }, new int[] { 2, 3, 4 } },
				Long = new long[][] { new long[] { 1, 2, 3 }, new long[] { 2, 3, 4 } },
				SByte = new sbyte[][] { new sbyte[] { 1, 2, 3 }, new sbyte[] { 2, 3, 4 } },
				UShort = new ushort[][] { new ushort[] { 1, 2, 3 }, new ushort[] { 2, 3, 4 } },
				UInt = new uint[][] { new uint[] { 1, 2, 3 }, new uint[] { 2, 3, 4 } },
				ULong = new ulong[][] { new ulong[] { 1, 2, 3 }, new ulong[] { 2, 3, 4 } },
				Char = new char[][] { new char[] { 'a', 'b', 'c', '☀' }, new char[] { 'a', 'b', 'c' } },
				Decimal = new decimal[][] { new decimal[] { 1, 2, 3 }, new decimal[] { 2, 3, 4 } },
				Double = new double[][] { new double[] { 1, 2, 3 }, new double[] { 2, 3, 4 } },
				Single = new float[][] { new float[] { 1, 2, 3 }, new float[] { 2, 3, 4 } },
				String = new string[][] { new string[] { "a", "b", "c", "Today, there will be a lot of ☀." }, new string[] { "a", "b", "c" } },
				CIStrings = new CaseInsensitiveString[][] { new CaseInsensitiveString[] { "a", "b", "c", "Today, there will be a lot of ☀." }, new CaseInsensitiveString[] { "a", "b", "c" } },
				DateTime = new DateTime[][] { new DateTime[] { DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue }, new DateTime[] { DateTime.MinValue, DateTime.MaxValue } },
				TimeSpan = new TimeSpan[][] { new TimeSpan[] { DateTime.Now.TimeOfDay, TimeSpan.Zero }, new TimeSpan[] { TimeSpan.MinValue, TimeSpan.MaxValue } },
				Guid = new Guid[][] { new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }, new Guid[] { Guid.NewGuid(), Guid.NewGuid() } },
				NormalEnum = new NormalEnum[][] { new NormalEnum[] { NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4 }, new NormalEnum[] { NormalEnum.Option1, NormalEnum.Option2 } },
				FlagsEnum = new FlagsEnum[][] { new FlagsEnum[] { FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3 }, new FlagsEnum[] { FlagsEnum.Option2, FlagsEnum.Option3 } }
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ArraysOfArrays));
			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			ArraysOfArrays Obj2 = (ArraysOfArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Boolean, GenObj["Boolean"]);
			AssertEx.Same(Obj.Byte, GenObj["Byte"]);
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.SByte, GenObj["SByte"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.Char, GenObj["Char"]);
			AssertEx.Same(Obj.Decimal, GenObj["Decimal"]);
			AssertEx.Same(Obj.Double, GenObj["Double"]);
			AssertEx.Same(Obj.Single, GenObj["Single"]);
			AssertEx.Same(Obj.String, GenObj["String"]);
			AssertEx.Same(Obj.CIStrings, GenObj["CIStrings"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(new string[][] { new string[] { Obj.NormalEnum[0][0].ToString(), Obj.NormalEnum[0][1].ToString(), Obj.NormalEnum[0][2].ToString() }, new string[] { Obj.NormalEnum[1][0].ToString(), Obj.NormalEnum[1][1].ToString() } }, GenObj["NormalEnum"]);
			AssertEx.Same(new int[][] { new int[] { (int)Obj.FlagsEnum[0][0], (int)Obj.FlagsEnum[0][1] }, new int[] { (int)Obj.FlagsEnum[1][0], (int)Obj.FlagsEnum[1][1] } }, GenObj["FlagsEnum"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (ArraysOfArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_15_ObsoleteMethod()
		{
			ObsoleteMethod Obj = new ObsoleteMethod()
			{
				Boolean1 = true,
				Boolean2 = false,
				Byte = 15,
				Short = -1234,
				Int = -23456789,
				Long = -345456456456456345,
				SByte = -45,
				UShort = 23456,
				UInt = 334534564,
				ULong = 4345345345345345,
				Char = '☀',
				Decimal = 12345.6789M,
				Double = 12345.6789,
				Single = 12345.6789f,
				String = "Today, there will be a lot of ☀.",
				DateTime = DateTime.Now,
				DateTimeOffset = DateTimeOffset.Now,
				Guid = Guid.NewGuid(),
				NormalEnum = NormalEnum.Option3,
				FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4,
				CIString = "Hello World!"
			};

			Obj.TimeSpan = Obj.DateTime.TimeOfDay;

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ObsoleteMethod));
			object Value;

			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean1", Obj));
			AssertEx.Same(Obj.Boolean1, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Boolean2", Obj));
			AssertEx.Same(Obj.Boolean2, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Byte", Obj));
			AssertEx.Same(Obj.Byte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("SByte", Obj));
			AssertEx.Same(Obj.SByte, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Char", Obj));
			AssertEx.Same(Obj.Char, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Decimal", Obj));
			AssertEx.Same(Obj.Decimal, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Double", Obj));
			AssertEx.Same(Obj.Double, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Single", Obj));
			AssertEx.Same(Obj.Single, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("String", Obj));
			AssertEx.Same(Obj.String, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("DateTime", Obj));
			AssertEx.Same(Obj.DateTime, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("DateTimeOffset", Obj));
			AssertEx.Same(Obj.DateTimeOffset, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("TimeSpan", Obj));
			AssertEx.Same(Obj.TimeSpan, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Guid", Obj));
			AssertEx.Same(Obj.Guid, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("NormalEnum", Obj));
			AssertEx.Same(Obj.NormalEnum, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("FlagsEnum", Obj));
			AssertEx.Same(Obj.FlagsEnum, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("CIString", Obj));
			AssertEx.Same(Obj.CIString, Value);

			ISerializer Writer = new DebugSerializer(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), Console.Out);

			await S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			IDeserializer Reader = new DebugDeserializer(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), Console.Out);

			ObsoleteMethod Obj2 = (ObsoleteMethod)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (ObsoleteMethod)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(ArraysOfArrays Obj, ArraysOfArrays Obj2)
		{
			AssertEx.Same(Obj.Boolean, Obj2.Boolean);
			AssertEx.Same(Obj.Byte, Obj2.Byte);
			AssertEx.Same(Obj.Short, Obj2.Short);
			AssertEx.Same(Obj.Int, Obj2.Int);
			AssertEx.Same(Obj.Long, Obj2.Long);
			AssertEx.Same(Obj.SByte, Obj2.SByte);
			AssertEx.Same(Obj.UShort, Obj2.UShort);
			AssertEx.Same(Obj.UInt, Obj2.UInt);
			AssertEx.Same(Obj.ULong, Obj2.ULong);
			AssertEx.Same(Obj.Char, Obj2.Char);
			AssertEx.Same(Obj.Decimal, Obj2.Decimal);
			AssertEx.Same(Obj.Double, Obj2.Double);
			AssertEx.Same(Obj.Single, Obj2.Single);
			AssertEx.Same(Obj.String, Obj2.String);
			AssertEx.Same(Obj.CIStrings, Obj2.CIStrings);
			AssertEx.Same(Obj.DateTime, Obj2.DateTime);
			AssertEx.Same(Obj.TimeSpan, Obj2.TimeSpan);
			AssertEx.Same(Obj.Guid, Obj2.Guid);
			AssertEx.Same(Obj.NormalEnum, Obj2.NormalEnum);
			AssertEx.Same(Obj.FlagsEnum, Obj2.FlagsEnum);
		}

		// TODO: Objects, by reference, nullable (incl. null strings, arrays)
		// TODO: Multidimensional arrays
	}
}
