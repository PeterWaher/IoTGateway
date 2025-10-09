using IdApp.Test.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Persistence.Serialization;
using Waher.Runtime.Console;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Security;


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
		public static async Task ClassCleanup()
		{
			await provider.DisposeAsync();
			provider = null;
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_01_SimpleObject()
		{
			Simple Obj = new()
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
				ShortString = "Hello",
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
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ShortString", Obj));
			AssertEx.Same(Obj.ShortString, Value);
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			Simple Obj2 = (Simple)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (Simple)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
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
			AssertEx.Same(Obj.ShortString, GenObj["ShortString"]);
			AssertEx.Same(Obj.DateTime, GenObj["DateTime"]);
			AssertEx.Same(Obj.DateTimeOffset, GenObj["DateTimeOffset"]);
			AssertEx.Same(Obj.TimeSpan, GenObj["TimeSpan"]);
			AssertEx.Same(Obj.Guid, GenObj["Guid"]);
			AssertEx.Same(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			AssertEx.Same((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);
			AssertEx.Same(Obj.CIString, GenObj["CIString"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
		}

		private static void WriteData(byte[] Data)
		{
			int i, c = Data.Length;

			for (i = 0; i < c; i++)
			{
				if ((i & 15) == 0)
					ConsoleOut.WriteLine();
				else
					ConsoleOut.Write(' ');

				ConsoleOut.Write(Data[i].ToString("x2"));
			}

			ConsoleOut.WriteLine();
		}

		private static void AssertBinaryLength(byte[] Data, DebugDeserializer Reader)
		{
			Reader.Restart(Data, 0);

			Reader.ReadGuid();
			ulong Len = Reader.ReadVariableLengthUInt64();

			AssertEx.Same(Data.Length - 16 - VariableULongLen(Len), Len);
		}

		private static int VariableULongLen(ulong Len)
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
			Classes.Nullable Obj = new()
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			Classes.Nullable Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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
			AssertEx.Same(Obj.NormalEnum?.ToString(), GenObj["NormalEnum"]);
			AssertEx.Same(Obj.FlagsEnum?.ToString(), GenObj["FlagsEnum"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(Classes.Nullable Obj, Classes.Nullable Obj2)
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
			Classes.Nullable Obj = new()
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			Classes.Nullable Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (Classes.Nullable)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_04_Default1()
		{
			Default Obj = new()
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			Default Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
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
			Default Obj = new()
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			Default Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (Default)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_06_SimpleArrays()
		{
			SimpleArrays Obj = new()
			{
				Boolean = [true, false],
				Byte = [1, 2, 3],
				Short = [1, 2, 3],
				Int = [1, 2, 3],
				Long = [1, 2, 3],
				SByte = [1, 2, 3],
				UShort = [1, 2, 3],
				UInt = [1, 2, 3],
				ULong = [1, 2, 3],
				Char = ['a', 'b', 'c', '☀'],
				Decimal = [1, 2, 3],
				Double = [1, 2, 3],
				Single = [1, 2, 3],
				String = ["a", "b", "c", "Today, there will be a lot of ☀."],
				DateTime = [DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue],
				TimeSpan = [DateTime.Now.TimeOfDay, TimeSpan.Zero],
				Guid = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
				NormalEnum = [NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4],
				FlagsEnum = [FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3],
				CIStrings = ["a", "b", "c", "Today, there will be a lot of ☀."]
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(SimpleArrays));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			SimpleArrays Obj2 = (SimpleArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			//Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			// Note: Enumerations are serialized as strings in the generic object, so serializations will not be equal.

			Reader.Restart(Data2, 0);
			Obj2 = (SimpleArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(SimpleArrays Obj, SimpleArrays Obj2)
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
			NullableArrays Obj = new()
			{
				Boolean = [true, null, false],
				Byte = [1, null, 3],
				Short = [1, null, 3],
				Int = [1, null, 3],
				Long = [1, null, 3],
				SByte = [1, null, 3],
				UShort = [1, null, 3],
				UInt = [1, null, 3],
				ULong = [1, null, 3],
				Char = ['a', 'b', null, '☀'],
				Decimal = [1, null, 3],
				Double = [1, null, 3],
				Single = [1, null, 3],
				DateTime = [DateTime.Now, null, DateTime.MinValue, DateTime.MaxValue],
				TimeSpan = [DateTime.Now.TimeOfDay, null, TimeSpan.Zero],
				Guid = [Guid.NewGuid(), null, Guid.NewGuid()],
				NormalEnum = [NormalEnum.Option3, null, NormalEnum.Option4],
				FlagsEnum = [FlagsEnum.Option1 | FlagsEnum.Option4, null, FlagsEnum.Option3]
			};


			IObjectSerializer S = await provider.GetObjectSerializer(typeof(NullableArrays));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			NullableArrays Obj2 = (NullableArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			//Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			// Note: Enumerations are serialized as strings in the generic object, so serializations will not be equal.

			Reader.Restart(Data2, 0);
			Obj2 = (NullableArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(NullableArrays Obj, NullableArrays Obj2)
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
			Container Obj = new()
			{
				Embedded = new()
				{
					Byte = 10,
					Short = 1000,
					Int = 10000000
				},
				EmbeddedNull = null,
				MultipleEmbedded =
				[
					new()
					{
						Byte = 20,
						Short = 2000,
						Int = 20000000
					},
					new()
					{
						Byte = 30,
						Short = 3000,
						Int = 30000000
					},
					new()
					{
						Byte = 40,
						Short = 4000,
						Int = 40000000
					}
				],
				MultipleEmbeddedNullable =
				[
					new()
					{
						Byte = 20,
						Short = 2000,
						Int = 20000000
					},
					null,
					new()
					{
						Byte = 40,
						Short = 4000,
						Int = 40000000
					}
				],
				MultipleEmbeddedNull = null
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Container));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			Container Obj2 = (Container)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (Container)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(Container Obj, Container Obj2)
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
			ObjectIdString Obj = new()
			{
				Value = 0x12345678
			};

			Assert.IsTrue(string.IsNullOrEmpty(Obj.ObjectId));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ObjectIdString));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(string.IsNullOrEmpty(Obj.ObjectId));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			ObjectIdString Obj2 = (ObjectIdString)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Value, GenObj["Value"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId.ToString());

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (ObjectIdString)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(ObjectIdString Obj, ObjectIdString Obj2)
		{
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_10_ObjectIdByteArray()
		{
			ObjectIdByteArray Obj = new()
			{
				Value = 0x12345678
			};

			Assert.IsNull(Obj.ObjectId);

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ObjectIdByteArray));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsNotNull(Obj.ObjectId);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			ObjectIdByteArray Obj2 = (ObjectIdByteArray)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Value, GenObj["Value"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId.ToByteArray());

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (ObjectIdByteArray)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(ObjectIdByteArray Obj, ObjectIdByteArray Obj2)
		{
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_11_LocalTypeName()
		{
			LocalNameSubclass1 Obj1 = new()
			{
				Name = "Obj1",
				Value = 0x12345678
			};

			LocalNameSubclass2 Obj2 = new()
			{
				Name = "Obj2",
				Value = "Hello"
			};

			Assert.IsTrue(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsTrue(Obj2.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S1 = await provider.GetObjectSerializer(typeof(LocalNameSubclass1));
			IObjectSerializer S2 = await provider.GetObjectSerializer(typeof(LocalNameSubclass2));
			IObjectSerializer S = await provider.GetObjectSerializer(typeof(LocalNameBase));
			DebugSerializer Writer1 = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);
			DebugSerializer Writer2 = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S1.Serialize(Writer1, false, false, Obj1, null);
			await S2.Serialize(Writer2, false, false, Obj2, null);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			WriteData(Data1);
			WriteData(Data2);

			DebugDeserializer Reader1 = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data1, uint.MaxValue), ConsoleOut.Writer);
			DebugDeserializer Reader2 = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data2, uint.MaxValue), ConsoleOut.Writer);

			LocalNameSubclass1 Obj12 = (LocalNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			LocalNameSubclass2 Obj22 = (LocalNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj1, Obj12);
			AssertEqual(Obj2, Obj22);

			AssertBinaryLength(Data1, Reader1);
			AssertBinaryLength(Data2, Reader2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer1, false, false, GenObj1, null);
			await GS.Serialize(Writer2, false, false, GenObj2, null);

			Data1 = Writer1.GetSerialization();
			Data2 = Writer2.GetSerialization();
			WriteData(Data1);
			WriteData(Data2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			Obj12 = (LocalNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			Obj22 = (LocalNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj1, Obj12);
			AssertEqual(Obj2, Obj22);
			AssertBinaryLength(Data1, Reader1);
			AssertBinaryLength(Data2, Reader2);
		}

		private static void AssertEqual(LocalNameSubclass1 Obj, LocalNameSubclass1 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		private static void AssertEqual(LocalNameSubclass2 Obj, LocalNameSubclass2 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_12_FullTypeName()
		{
			FullNameSubclass1 Obj1 = new()
			{
				Name = "Obj1",
				Value = 0x12345678
			};

			FullNameSubclass2 Obj2 = new()
			{
				Name = "Obj2",
				Value = "Hello"
			};

			Assert.IsTrue(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsTrue(Obj2.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S1 = await provider.GetObjectSerializer(typeof(FullNameSubclass1));
			IObjectSerializer S2 = await provider.GetObjectSerializer(typeof(FullNameSubclass2));
			IObjectSerializer S = await provider.GetObjectSerializer(typeof(FullNameBase));
			DebugSerializer Writer1 = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);
			DebugSerializer Writer2 = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S1.Serialize(Writer1, false, false, Obj1, null);
			await S2.Serialize(Writer2, false, false, Obj2, null);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			WriteData(Data1);
			WriteData(Data2);

			DebugDeserializer Reader1 = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data1, uint.MaxValue), ConsoleOut.Writer);
			DebugDeserializer Reader2 = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data2, uint.MaxValue), ConsoleOut.Writer);

			FullNameSubclass1 Obj12 = (FullNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			FullNameSubclass2 Obj22 = (FullNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj1, Obj12);
			AssertEqual(Obj2, Obj22);

			AssertBinaryLength(Data1, Reader1);
			AssertBinaryLength(Data2, Reader2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			GenericObjectSerializer GS = new(provider);
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

			await GS.Serialize(Writer1, false, false, GenObj1, null);
			await GS.Serialize(Writer2, false, false, GenObj2, null);

			Data1 = Writer1.GetSerialization();
			Data2 = Writer2.GetSerialization();
			WriteData(Data1);
			WriteData(Data2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			Obj12 = (FullNameSubclass1)await S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			Obj22 = (FullNameSubclass2)await S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj1, Obj12);
			AssertEqual(Obj2, Obj22);
			AssertBinaryLength(Data1, Reader1);
			AssertBinaryLength(Data2, Reader2);
		}

		private static void AssertEqual(FullNameSubclass1 Obj, FullNameSubclass1 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		private static void AssertEqual(FullNameSubclass2 Obj, FullNameSubclass2 Obj2)
		{
			AssertEx.Same(Obj.Name, Obj2.Name);
			AssertEx.Same(Obj.Value, Obj2.Value);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_13_CollectionTest()
		{
			CollectionTest Obj = new()
			{
				S1 = "Today, there will be a lot of ☀.",
				S2 = "Hello world.",
				S3 = "Testing, testing..."
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(CollectionTest));
			DebugSerializer Writer = new(new BinarySerializer(await ((ObjectSerializer)S).CollectionName(Obj), Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(await ((ObjectSerializer)S).CollectionName(Obj), Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			CollectionTest Obj2 = (CollectionTest)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Test");
			AssertEx.Same(Obj.S1, GenObj["S1"]);
			AssertEx.Same(Obj.S2, GenObj["S2"]);
			AssertEx.Same(Obj.S3, GenObj["S3"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (CollectionTest)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(CollectionTest Obj, CollectionTest Obj2)
		{
			AssertEx.Same(Obj.S1, Obj2.S1);
			AssertEx.Same(Obj.S2, Obj2.S2);
			AssertEx.Same(Obj.S3, Obj2.S3);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_14_ArraysOfArrays()
		{
			ArraysOfArrays Obj = new()
			{
				Boolean = [[true, false], [false, true]],
				Byte = [[1, 2, 3], [2, 3, 4]],
				Short = [[1, 2, 3], [2, 3, 4]],
				Int = [[1, 2, 3], [2, 3, 4]],
				Long = [[1, 2, 3], [2, 3, 4]],
				SByte = [[1, 2, 3], [2, 3, 4]],
				UShort = [[1, 2, 3], [2, 3, 4]],
				UInt = [[1, 2, 3], [2, 3, 4]],
				ULong = [[1, 2, 3], [2, 3, 4]],
				Char = [['a', 'b', 'c', '☀'], ['a', 'b', 'c']],
				Decimal = [[1, 2, 3], [2, 3, 4]],
				Double = [[1, 2, 3], [2, 3, 4]],
				Single = [[1, 2, 3], [2, 3, 4]],
				String = [["a", "b", "c", "Today, there will be a lot of ☀."], ["a", "b", "c"]],
				CIStrings = [["a", "b", "c", "Today, there will be a lot of ☀."], ["a", "b", "c"]],
				DateTime = [[DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue], [DateTime.MinValue, DateTime.MaxValue]],
				TimeSpan = [[DateTime.Now.TimeOfDay, TimeSpan.Zero], [TimeSpan.MinValue, TimeSpan.MaxValue]],
				Guid = [[Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]],
				NormalEnum = [[NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4], [NormalEnum.Option1, NormalEnum.Option2]],
				FlagsEnum = [[FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3], [FlagsEnum.Option2, FlagsEnum.Option3]]
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ArraysOfArrays));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			ArraysOfArrays Obj2 = (ArraysOfArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
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
			AssertEx.Same(new string[][] { [Obj.NormalEnum[0][0].ToString(), Obj.NormalEnum[0][1].ToString(), Obj.NormalEnum[0][2].ToString()], [Obj.NormalEnum[1][0].ToString(), Obj.NormalEnum[1][1].ToString()] }, GenObj["NormalEnum"]);
			AssertEx.Same(new int[][] { [(int)Obj.FlagsEnum[0][0], (int)Obj.FlagsEnum[0][1]], [(int)Obj.FlagsEnum[1][0], (int)Obj.FlagsEnum[1][1]] }, GenObj["FlagsEnum"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			//Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			// Note: Enumerations are serialized as strings in the generic object, so serializations will not be equal.

			Reader.Restart(Data2, 0);
			Obj2 = (ArraysOfArrays)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(ArraysOfArrays Obj, ArraysOfArrays Obj2)
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

		[TestMethod]
		public async Task DBFiles_ObjSerialization_15_ObsoleteMethod()
		{
			ObsoleteMethod Obj = new()
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			ObsoleteMethod Obj2 = (ObsoleteMethod)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (ObsoleteMethod)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_16_GenObject1()
		{
			GenObj1 Obj = new()
			{
				EmbeddedObj = new GenericObject(string.Empty, string.Empty, Guid.Empty,
					new KeyValuePair<string, object>("A", 10),
					new KeyValuePair<string, object>("B", "Hello"))
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(GenObj1));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);
			GenObj1 Obj2 = (GenObj1)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.IsNotNull(Obj2.EmbeddedObj);
			Assert.AreEqual(Obj.EmbeddedObj.Count, Obj2.EmbeddedObj.Count);
			Assert.IsTrue(Obj2.EmbeddedObj.TryGetFieldValue("A", out object A));
			Assert.IsTrue(Obj2.EmbeddedObj.TryGetFieldValue("B", out object B));
			Assert.AreEqual(10, A);
			Assert.AreEqual("Hello", B);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_17_GenObject2()
		{
			GenObj2 Obj = new()
			{
				EmbeddedObj = new Dictionary<string, object>()
				{
					{ "A", 10 },
					{ "B", "Hello" }
				}
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(GenObj2));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);
			GenObj2 Obj2 = (GenObj2)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.IsNotNull(Obj2.EmbeddedObj);
			Assert.IsTrue(Obj2.EmbeddedObj.TryGetValue("A", out object A));
			Assert.IsTrue(Obj2.EmbeddedObj.TryGetValue("B", out object B));
			Assert.AreEqual(10, A);
			Assert.AreEqual("Hello", B);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_18_GenObject3()
		{
			GenObj3 Obj = new()
			{
				EmbeddedObj = new Dictionary<string, IElement>()
				{
					{ "A", new DoubleNumber(10) },
					{ "B", new StringValue("Hello") }
				}
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(GenObj3));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);
			GenObj3 Obj2 = (GenObj3)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.IsNotNull(Obj2.EmbeddedObj);
			Assert.IsTrue(Obj2.EmbeddedObj.TryGetValue("A", out IElement A));
			Assert.IsTrue(Obj2.EmbeddedObj.TryGetValue("B", out IElement B));
			Assert.AreEqual(10.0, A.AssociatedObjectValue);
			Assert.AreEqual("Hello", B.AssociatedObjectValue);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_19_GenObject4()
		{
			GenObj4 Obj = new()
			{
				EmbeddedObj =
				[
					new("A", 10),
					new("B", "Hello")
				]
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(GenObj4));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);
			GenObj4 Obj2 = (GenObj4)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.IsNotNull(Obj2.EmbeddedObj);
			object A = null;
			object B = null;

			foreach (KeyValuePair<string, object> P in Obj2.EmbeddedObj)
			{
				switch (P.Key)
				{
					case "A":
						A = P.Value;
						break;

					case "B":
						B = P.Value;
						break;
				}
			}

			Assert.AreEqual(10, A);
			Assert.AreEqual("Hello", B);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_20_GenObject5()
		{
			GenObj5 Obj = new()
			{
				EmbeddedObj =
				[
					new("A", new DoubleNumber(10)),
					new("B", new StringValue("Hello"))
				]
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(GenObj5));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);
			GenObj5 Obj2 = (GenObj5)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.IsNotNull(Obj2.EmbeddedObj);
			object A = null;
			object B = null;

			foreach (KeyValuePair<string, IElement> P in Obj2.EmbeddedObj)
			{
				switch (P.Key)
				{
					case "A":
						A = P.Value.AssociatedObjectValue;
						break;

					case "B":
						B = P.Value.AssociatedObjectValue;
						break;
				}
			}

			Assert.AreEqual(10.0, A);
			Assert.AreEqual("Hello", B);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_21_Structs()
		{
			Structs Obj = new()
			{
				Duration = new Content.Duration(true, 1, 2, 3, 4, 5, 6)
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(Structs));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);
			Structs Obj2 = (Structs)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.IsNotNull(Obj2.Duration);
			Assert.AreEqual(true, Obj2.Duration.Negation);
			Assert.AreEqual(1, Obj2.Duration.Years);
			Assert.AreEqual(2, Obj2.Duration.Months);
			Assert.AreEqual(3, Obj2.Duration.Days);
			Assert.AreEqual(4, Obj2.Duration.Hours);
			Assert.AreEqual(5, Obj2.Duration.Minutes);
			Assert.AreEqual(6, Obj2.Duration.Seconds);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_22_VarLimits_Min()
		{
			VarLenIntegers Obj = new()
			{
				Short = GeneratedObjectSerializerBase.Int16VarSizeMinLimit + 1,
				Int = GeneratedObjectSerializerBase.Int32VarSizeMinLimit + 1,
				Long = GeneratedObjectSerializerBase.Int64VarSizeMinLimit + 1,
				UShort = 0,
				UInt = 0,
				ULong = 0
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(VarLenIntegers));
			object Value;

			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			VarLenIntegers Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_23_VarLimits_Max()
		{
			VarLenIntegers Obj = new()
			{
				Short = GeneratedObjectSerializerBase.Int16VarSizeMaxLimit - 1,
				Int = GeneratedObjectSerializerBase.Int32VarSizeMaxLimit - 1,
				Long = GeneratedObjectSerializerBase.Int64VarSizeMaxLimit - 1,
				UShort = GeneratedObjectSerializerBase.UInt16VarSizeLimit - 1,
				UInt = GeneratedObjectSerializerBase.UInt32VarSizeLimit - 1,
				ULong = GeneratedObjectSerializerBase.UInt64VarSizeLimit - 1
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(VarLenIntegers));
			object Value;

			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			VarLenIntegers Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_24_FixedLimits_Min()
		{
			VarLenIntegers Obj = new()
			{
				Short = GeneratedObjectSerializerBase.Int16VarSizeMinLimit,
				Int = GeneratedObjectSerializerBase.Int32VarSizeMinLimit,
				Long = GeneratedObjectSerializerBase.Int64VarSizeMinLimit,
				UShort = 0,
				UInt = 0,
				ULong = 0
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(VarLenIntegers));
			object Value;

			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			VarLenIntegers Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_25_FixedLimits_Max()
		{
			VarLenIntegers Obj = new()
			{
				Short = GeneratedObjectSerializerBase.Int16VarSizeMaxLimit,
				Int = GeneratedObjectSerializerBase.Int32VarSizeMaxLimit,
				Long = GeneratedObjectSerializerBase.Int64VarSizeMaxLimit,
				UShort = GeneratedObjectSerializerBase.UInt16VarSizeLimit,
				UInt = GeneratedObjectSerializerBase.UInt32VarSizeLimit,
				ULong = GeneratedObjectSerializerBase.UInt64VarSizeLimit
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(VarLenIntegers));
			object Value;

			Assert.IsNotNull(Value = await S.TryGetFieldValue("Short", Obj));
			AssertEx.Same(Obj.Short, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Int", Obj));
			AssertEx.Same(Obj.Int, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("Long", Obj));
			AssertEx.Same(Obj.Long, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UShort", Obj));
			AssertEx.Same(Obj.UShort, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("UInt", Obj));
			AssertEx.Same(Obj.UInt, Value);
			Assert.IsNotNull(Value = await S.TryGetFieldValue("ULong", Obj));
			AssertEx.Same(Obj.ULong, Value);

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			VarLenIntegers Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (VarLenIntegers)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		internal static void AssertEqual(VarLenIntegers Obj, VarLenIntegers Obj2)
		{
			AssertEx.Same(Obj.Short, Obj2.Short);
			AssertEx.Same(Obj.Int, Obj2.Int);
			AssertEx.Same(Obj.Long, Obj2.Long);
			AssertEx.Same(Obj.UShort, Obj2.UShort);
			AssertEx.Same(Obj.UInt, Obj2.UInt);
			AssertEx.Same(Obj.ULong, Obj2.ULong);
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
		}

		internal static void AssertEqual(VarLenIntegers Obj, GenericObject GenObj)
		{
			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.Short, GenObj["Short"]);
			AssertEx.Same(Obj.Int, GenObj["Int"]);
			AssertEx.Same(Obj.Long, GenObj["Long"]);
			AssertEx.Same(Obj.UShort, GenObj["UShort"]);
			AssertEx.Same(Obj.UInt, GenObj["UInt"]);
			AssertEx.Same(Obj.ULong, GenObj["ULong"]);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_26_Nested()
		{
			NestedClass Obj = CreateNestedTestClass();
			IObjectSerializer S = await provider.GetObjectSerializer(typeof(NestedClass));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			NestedClass Obj2 = (NestedClass)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (NestedClass)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static NestedClass CreateNestedTestClass()
		{
			return new NestedClass()
			{
				ObjectId = Guid.NewGuid(),
				UI8 = byte.MaxValue,
				UI16 = ushort.MaxValue,
				UI32 = uint.MaxValue,
				UI64 = ulong.MaxValue,
				I8 = sbyte.MaxValue,
				I16 = short.MaxValue,
				I32 = int.MaxValue,
				I64 = long.MaxValue,
				S = "Kilroy was here",
				Cis = new CaseInsensitiveString("Hello WORLD"),
				Ch = 'a',
				Bin = Encoding.UTF8.GetBytes("Hello World"),
				Id = Guid.NewGuid(),
				TS = TimeSpan.FromHours(12),
				D = Duration.Parse("P1Y2M3DT4H5M6S"),
				Null = null,
				B = true,
				Fl = float.MaxValue,
				Db = double.MaxValue,
				Dc = decimal.MaxValue,
				E = EventType.Notice,
				TP = DateTime.Now,
				TPO = DateTimeOffset.Now,
				A = ["Kilroy", "was", "here"],
				Nested = new NestedClass()
				{
					ObjectId = Guid.NewGuid(),
					UI8 = byte.MinValue,
					UI16 = ushort.MinValue,
					UI32 = uint.MinValue,
					UI64 = ulong.MinValue,
					I8 = sbyte.MinValue,
					I16 = short.MinValue,
					I32 = int.MinValue,
					I64 = long.MinValue,
					S = "Kilroy was here",
					Cis = new CaseInsensitiveString("Hello WORLD"),
					Ch = 'a',
					Bin = Encoding.UTF8.GetBytes("Hello World"),
					Id = Guid.NewGuid(),
					TS = TimeSpan.FromHours(12),
					D = Duration.Parse("P1Y2M3DT4H5M6S"),
					Null = null,
					B = true,
					Fl = float.MinValue,
					Db = double.MinValue,
					Dc = decimal.MinValue,
					E = EventType.Notice,
					TP = DateTime.Now,
					TPO = DateTimeOffset.Now,
					A = ["Kilroy", "was", "here"],
					Nested = null,
				},
			};
		}
		internal static void AssertEqual(NestedClass Obj, NestedClass Obj2)
		{
			if ((Obj is null) ^ (Obj2 is null))
				Assert.Fail("Object null check fail.");

			if (Obj is null)
				return;

			//AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
			AssertEx.Same(Obj.UI8, Obj2.UI8);
			AssertEx.Same(Obj.UI16, Obj2.UI16);
			AssertEx.Same(Obj.UI32, Obj2.UI32);
			AssertEx.Same(Obj.UI64, Obj2.UI64);
			AssertEx.Same(Obj.I8, Obj2.I8);
			AssertEx.Same(Obj.I16, Obj2.I16);
			AssertEx.Same(Obj.I32, Obj2.I32);
			AssertEx.Same(Obj.I64, Obj2.I64);
			AssertEx.Same(Obj.S, Obj2.S);
			AssertEx.Same(Obj.Cis, Obj2.Cis);
			AssertEx.Same(Obj.Ch, Obj2.Ch);
			AssertEx.Same(Obj.Bin, Obj2.Bin);
			AssertEx.Same(Obj.Id, Obj2.Id);
			AssertEx.Same(Obj.TS, Obj2.TS);
			AssertEx.Same(Obj.D, Obj2.D);
			AssertEx.Same(Obj.Null, Obj2.Null);
			AssertEx.Same(Obj.B, Obj2.B);
			AssertEx.Same(Obj.Fl, Obj2.Fl);
			AssertEx.Same(Obj.Db, Obj2.Db);
			AssertEx.Same(Obj.Dc, Obj2.Dc);
			AssertEx.Same(Obj.E, Obj2.E);
			AssertEx.Same(Obj.TP, Obj2.TP);
			AssertEx.Same(Obj.TPO, Obj2.TPO);
			AssertEx.Same(Obj.A, Obj2.A);

			AssertEqual(Obj.Nested, Obj2.Nested);
		}

		internal static void AssertEqual(NestedClass Obj, GenericObject GenObj)
		{
			if ((Obj is null) ^ (GenObj is null))
				Assert.Fail("Object null check fail.");

			if (Obj is null)
				return;

			//AssertEx.Same(GenObj.CollectionName, "Default");
			//AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEx.Same(Obj.UI8, GenObj["UI8"]);
			AssertEx.Same(Obj.UI16, GenObj["UI16"]);
			AssertEx.Same(Obj.UI32, GenObj["UI32"]);
			AssertEx.Same(Obj.UI64, GenObj["UI64"]);
			AssertEx.Same(Obj.I8, GenObj["I8"]);
			AssertEx.Same(Obj.I16, GenObj["I16"]);
			AssertEx.Same(Obj.I32, GenObj["I32"]);
			AssertEx.Same(Obj.I64, GenObj["I64"]);
			AssertEx.Same(Obj.S, GenObj["S"]);
			AssertEx.Same(Obj.Cis, GenObj["Cis"]);
			AssertEx.Same(Obj.Ch, GenObj["Ch"]);
			AssertEx.Same(Obj.Bin, GenObj["Bin"]);
			AssertEx.Same(Obj.Id, GenObj["Id"]);
			AssertEx.Same(Obj.TS, GenObj["TS"]);
			//AssertEx.Same(Obj.D, GenObj["D"]);
			AssertEx.Same(Obj.Null, GenObj["Null"]);
			AssertEx.Same(Obj.B, GenObj["B"]);
			AssertEx.Same(Obj.Fl, GenObj["Fl"]);
			AssertEx.Same(Obj.Db, GenObj["Db"]);
			AssertEx.Same(Obj.Dc, GenObj["Dc"]);
			//AssertEx.Same(Obj.E, GenObj["E"]);
			AssertEx.Same(Obj.TP, GenObj["TP"]);
			AssertEx.Same(Obj.TPO, GenObj["TPO"]);
			AssertEx.Same(Obj.A, GenObj["A"]);

			AssertEqual(Obj.Nested, GenObj["Nested"] as GenericObject);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_27_Binary()
		{
			DockerBlob Obj = new()
			{
				ObjectId = Guid.NewGuid().ToString(),
				Function = HashFunction.SHA256,
				Digest =
				[
					0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
					10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
					20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
					30, 31
				],
				FileName = "Some file",
				AccountName = "Some user"
			};

			ObjectSerializer S = (ObjectSerializer)await provider.GetObjectSerializer(typeof(DockerBlob));
			DebugSerializer Writer = new(new BinarySerializer(await S.CollectionName(Obj), Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(await S.CollectionName(Obj), Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			DockerBlob Obj2 = (DockerBlob)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Convert.ToBase64String(Obj.Digest), Convert.ToBase64String(Obj2.Digest));
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId.ToString());
			Assert.AreEqual(Obj.Function.ToString(), GenObj["Function"]);
			Assert.AreEqual(Convert.ToBase64String(Obj.Digest), Convert.ToBase64String((byte[])GenObj["Digest"]));
			Assert.AreEqual(Obj.FileName, GenObj["FileName"]);
			Assert.AreEqual(Obj.AccountName, GenObj["AccountName"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (DockerBlob)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Convert.ToBase64String(Obj.Digest), Convert.ToBase64String(Obj2.Digest));
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_28_BinaryNull()
		{
			DockerBlob Obj = new()
			{
				ObjectId = Guid.NewGuid().ToString(),
				FileName = "Some file",
				AccountName = "Some user"
			};

			ObjectSerializer S = (ObjectSerializer)await provider.GetObjectSerializer(typeof(DockerBlob));
			DebugSerializer Writer = new(new BinarySerializer(await S.CollectionName(Obj), Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(await S.CollectionName(Obj), Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			DockerBlob Obj2 = (DockerBlob)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Obj.Digest, Obj2.Digest);
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId.ToString());
			Assert.AreEqual(Obj.Function.ToString(), GenObj["Function"]);
			Assert.AreEqual(Obj.Digest, GenObj["Digest"]);
			Assert.AreEqual(Obj.FileName, GenObj["FileName"]);
			Assert.AreEqual(Obj.AccountName, GenObj["AccountName"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (DockerBlob)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Obj.Digest, Obj2.Digest);
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_29_SimpleEncryptedObject()
		{
			SimpleEncrypted Obj = new()
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

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(SimpleEncrypted));
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			SimpleEncrypted Obj2 = (SimpleEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEncrypted(Obj, GenObj);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (SimpleEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		internal static void AssertEqual(SimpleEncrypted Obj, SimpleEncrypted Obj2)
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

		internal static void AssertEncrypted(SimpleEncrypted Obj, GenericObject GenObj)
		{
			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEncrypted(GenObj, "Boolean1", 100);
			AssertEncrypted(GenObj, "Boolean2", 0);
			AssertEncrypted(GenObj, "Byte", 100);
			AssertEncrypted(GenObj, "Short", 0);
			AssertEncrypted(GenObj, "Int", 100);
			AssertEncrypted(GenObj, "Long", 0);
			AssertEncrypted(GenObj, "SByte", 100);
			AssertEncrypted(GenObj, "UShort", 0);
			AssertEncrypted(GenObj, "UInt", 100);
			AssertEncrypted(GenObj, "ULong", 0);
			AssertEncrypted(GenObj, "Char", 100);
			AssertEncrypted(GenObj, "Decimal", 0);
			AssertEncrypted(GenObj, "Double", 100);
			AssertEncrypted(GenObj, "Single", 0);
			AssertEncrypted(GenObj, "String", 100);
			AssertEncrypted(GenObj, "ShortString", 0);
			AssertEncrypted(GenObj, "DateTime", 100);
			AssertEncrypted(GenObj, "DateTimeOffset", 0);
			AssertEncrypted(GenObj, "TimeSpan", 100);
			AssertEncrypted(GenObj, "Guid", 0);
			AssertEncrypted(GenObj, "NormalEnum", 100);
			AssertEncrypted(GenObj, "FlagsEnum", 0);
			AssertEncrypted(GenObj, "CIString", 100);
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
		}

		private static void AssertEncrypted(GenericObject GenObj, string Property, int MinLength)
		{
			Assert.IsTrue(GenObj[Property] is byte[] Encrypted &&
				Encrypted.Length >= MinLength);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_30_Nullable1Encrypted()
		{
			NullableEncrypted Obj = new()
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


			IObjectSerializer S = await provider.GetObjectSerializer(typeof(NullableEncrypted));
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			NullableEncrypted Obj2 = (NullableEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEncrypted(GenObj, "Boolean1", 0);
			AssertEncrypted(GenObj, "Boolean2", 100);
			AssertEncrypted(GenObj, "Byte", 0);
			AssertEncrypted(GenObj, "Short", 100);
			AssertEncrypted(GenObj, "Int", 0);
			AssertEncrypted(GenObj, "Long", 100);
			AssertEncrypted(GenObj, "SByte", 0);
			AssertEncrypted(GenObj, "UShort", 100);
			AssertEncrypted(GenObj, "UInt", 0);
			AssertEncrypted(GenObj, "ULong", 100);
			AssertEncrypted(GenObj, "Char", 0);
			AssertEncrypted(GenObj, "Decimal", 100);
			AssertEncrypted(GenObj, "Double", 0);
			AssertEncrypted(GenObj, "Single", 100);
			AssertEncrypted(GenObj, "String", 0);
			AssertEncrypted(GenObj, "DateTime", 100);
			AssertEncrypted(GenObj, "TimeSpan", 0);
			AssertEncrypted(GenObj, "Guid", 100);
			AssertEncrypted(GenObj, "NormalEnum", 0);
			AssertEncrypted(GenObj, "FlagsEnum", 100);
			
			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (NullableEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(NullableEncrypted Obj, NullableEncrypted Obj2)
		{
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
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
		public async Task DBFiles_ObjSerialization_31_Nullable2Encrypted()
		{
			NullableEncrypted Obj = new()
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

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(NullableEncrypted));
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			NullableEncrypted Obj2 = (NullableEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEncrypted(GenObj, "Boolean1", 0);
			AssertEncrypted(GenObj, "Boolean2", 100);
			AssertEncrypted(GenObj, "Byte", 0);
			AssertEncrypted(GenObj, "Short", 100);
			AssertEncrypted(GenObj, "Int", 0);
			AssertEncrypted(GenObj, "Long", 100);
			AssertEncrypted(GenObj, "SByte", 0);
			AssertEncrypted(GenObj, "UShort", 100);
			AssertEncrypted(GenObj, "UInt", 0);
			AssertEncrypted(GenObj, "ULong", 100);
			AssertEncrypted(GenObj, "Char", 0);
			AssertEncrypted(GenObj, "Decimal", 100);
			AssertEncrypted(GenObj, "Double", 0);
			AssertEncrypted(GenObj, "Single", 100);
			AssertEncrypted(GenObj, "String", 0);
			AssertEncrypted(GenObj, "DateTime", 100);
			AssertEncrypted(GenObj, "TimeSpan", 0);
			AssertEncrypted(GenObj, "Guid", 100);
			AssertEncrypted(GenObj, "NormalEnum", 0);
			AssertEncrypted(GenObj, "FlagsEnum", 100);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (NullableEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_32_Default1Encrypted()
		{
			DefaultEncrypted Obj = new()
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


			IObjectSerializer S = await provider.GetObjectSerializer(typeof(DefaultEncrypted));
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			DefaultEncrypted Obj2 = (DefaultEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEx.Same(null, GenObj["Boolean1"]);
			AssertEx.Same(null, GenObj["Boolean2"]);
			AssertEx.Same(null, GenObj["Byte"]);
			AssertEncrypted(GenObj, "Short", 100);
			AssertEx.Same(null, GenObj["Int"]);
			AssertEncrypted(GenObj, "Long", 100);
			AssertEx.Same(null, GenObj["SByte"]);
			AssertEncrypted(GenObj, "UShort", 100);
			AssertEx.Same(null, GenObj["UInt"]);
			AssertEncrypted(GenObj, "ULong", 100);
			AssertEx.Same(null, GenObj["Char"]);
			AssertEncrypted(GenObj, "Decimal", 100);
			AssertEx.Same(null, GenObj["Double"]);
			AssertEncrypted(GenObj, "Single", 100);
			AssertEx.Same(null, GenObj["String"]);
			AssertEncrypted(GenObj, "DateTime", 100);
			AssertEx.Same(null, GenObj["TimeSpan"]);
			AssertEncrypted(GenObj, "Guid", 100);
			AssertEx.Same(null, GenObj["NormalEnum"]);
			AssertEncrypted(GenObj, "FlagsEnum", 100);
			AssertEx.Same(null, GenObj["String2"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (DefaultEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		internal static void AssertEqual(DefaultEncrypted Obj, DefaultEncrypted Obj2)
		{
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
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
		public async Task DBFiles_ObjSerialization_33_Default2Encrypted()
		{
			DefaultEncrypted Obj = new()
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

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(DefaultEncrypted));
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

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			DefaultEncrypted Obj2 = (DefaultEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEncrypted(GenObj, "Boolean1", 0);
			AssertEncrypted(GenObj, "Boolean2", 100);
			AssertEncrypted(GenObj, "Byte", 0);
			AssertEx.Same(null, GenObj["Short"]);
			AssertEncrypted(GenObj, "Int", 0);
			AssertEx.Same(null, GenObj["Long"]);
			AssertEncrypted(GenObj, "SByte", 0);
			AssertEx.Same(null, GenObj["UShort"]);
			AssertEncrypted(GenObj, "UInt", 0);
			AssertEx.Same(null, GenObj["ULong"]);
			AssertEncrypted(GenObj, "Char", 0);
			AssertEx.Same(null, GenObj["Decimal"]);
			AssertEncrypted(GenObj, "Double", 0);
			AssertEx.Same(null, GenObj["Single"]);
			AssertEncrypted(GenObj, "String", 0);
			AssertEx.Same(null, GenObj["DateTime"]);
			AssertEncrypted(GenObj, "TimeSpan", 0);
			AssertEx.Same(null, GenObj["Guid"]);
			AssertEncrypted(GenObj, "NormalEnum", 0);
			AssertEx.Same(null, GenObj["FlagsEnum"]);
			AssertEncrypted(GenObj, "String2", 0);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (DefaultEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_34_SimpleArraysEncrypted()
		{
			SimpleArraysEncrypted Obj = new()
			{
				Boolean = [true, false],
				Byte = [1, 2, 3],
				Short = [1, 2, 3],
				Int = [1, 2, 3],
				Long = [1, 2, 3],
				SByte = [1, 2, 3],
				UShort = [1, 2, 3],
				UInt = [1, 2, 3],
				ULong = [1, 2, 3],
				Char = ['a', 'b', 'c', '☀'],
				Decimal = [1, 2, 3],
				Double = [1, 2, 3],
				Single = [1, 2, 3],
				String = ["a", "b", "c", "Today, there will be a lot of ☀."],
				DateTime = [DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue],
				TimeSpan = [DateTime.Now.TimeOfDay, TimeSpan.Zero],
				Guid = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
				NormalEnum = [NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4],
				FlagsEnum = [FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3],
				CIStrings = ["a", "b", "c", "Today, there will be a lot of ☀."]
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(SimpleArraysEncrypted));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			SimpleArraysEncrypted Obj2 = (SimpleArraysEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEncrypted(GenObj, "Boolean", 200);
			AssertEncrypted(GenObj, "Byte", 500);
			AssertEncrypted(GenObj, "Short", 200);
			AssertEncrypted(GenObj, "Int", 500);
			AssertEncrypted(GenObj, "Long", 200);
			AssertEncrypted(GenObj, "SByte", 500);
			AssertEncrypted(GenObj, "UShort", 200);
			AssertEncrypted(GenObj, "UInt", 500);
			AssertEncrypted(GenObj, "ULong", 200);
			AssertEncrypted(GenObj, "Char", 500);
			AssertEncrypted(GenObj, "Decimal", 200);
			AssertEncrypted(GenObj, "Double", 500);
			AssertEncrypted(GenObj, "Single", 200);
			AssertEncrypted(GenObj, "String", 500);
			AssertEncrypted(GenObj, "DateTime", 200);
			AssertEncrypted(GenObj, "TimeSpan", 500);
			AssertEncrypted(GenObj, "Guid", 200);
			AssertEncrypted(GenObj, "NormalEnum", 500);
			AssertEncrypted(GenObj, "FlagsEnum", 200);
			AssertEncrypted(GenObj, "CIStrings", 500);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (SimpleArraysEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(SimpleArraysEncrypted Obj, SimpleArraysEncrypted Obj2)
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
		public async Task DBFiles_ObjSerialization_35_NullableArraysEncrypted()
		{
			NullableArraysEncrypted Obj = new()
			{
				Boolean = [true, null, false],
				Byte = [1, null, 3],
				Short = [1, null, 3],
				Int = [1, null, 3],
				Long = [1, null, 3],
				SByte = [1, null, 3],
				UShort = [1, null, 3],
				UInt = [1, null, 3],
				ULong = [1, null, 3],
				Char = ['a', 'b', null, '☀'],
				Decimal = [1, null, 3],
				Double = [1, null, 3],
				Single = [1, null, 3],
				DateTime = [DateTime.Now, null, DateTime.MinValue, DateTime.MaxValue],
				TimeSpan = [DateTime.Now.TimeOfDay, null, TimeSpan.Zero],
				Guid = [Guid.NewGuid(), null, Guid.NewGuid()],
				NormalEnum = [NormalEnum.Option3, null, NormalEnum.Option4],
				FlagsEnum = [FlagsEnum.Option1 | FlagsEnum.Option4, null, FlagsEnum.Option3]
			};


			IObjectSerializer S = await provider.GetObjectSerializer(typeof(NullableArraysEncrypted));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			NullableArraysEncrypted Obj2 = (NullableArraysEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEncrypted(GenObj, "Boolean", 200);
			AssertEncrypted(GenObj, "Byte", 500);
			AssertEncrypted(GenObj, "Short", 200);
			AssertEncrypted(GenObj, "Int", 500);
			AssertEncrypted(GenObj, "Long", 200);
			AssertEncrypted(GenObj, "SByte", 500);
			AssertEncrypted(GenObj, "UShort", 200);
			AssertEncrypted(GenObj, "UInt", 500);
			AssertEncrypted(GenObj, "ULong", 200);
			AssertEncrypted(GenObj, "Char", 500);
			AssertEncrypted(GenObj, "Decimal", 200);
			AssertEncrypted(GenObj, "Double", 500);
			AssertEncrypted(GenObj, "Single", 200);
			AssertEncrypted(GenObj, "DateTime", 500);
			AssertEncrypted(GenObj, "TimeSpan", 200);
			AssertEncrypted(GenObj, "Guid", 500);
			AssertEncrypted(GenObj, "NormalEnum", 200);
			AssertEncrypted(GenObj, "FlagsEnum", 500);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (NullableArraysEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(NullableArraysEncrypted Obj, NullableArraysEncrypted Obj2)
		{
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
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
		public async Task DBFiles_ObjSerialization_36_EmbeddedEncrypted()
		{
			ContainerEncrypted Obj = new()
			{
				Embedded = new()
				{
					Byte = 10,
					Short = 1000,
					Int = 10000000
				},
				EmbeddedNull = null,
				MultipleEmbedded =
				[
					new()
					{
						Byte = 20,
						Short = 2000,
						Int = 20000000
					},
					new()
					{
						Byte = 30,
						Short = 3000,
						Int = 30000000
					},
					new()
					{
						Byte = 40,
						Short = 4000,
						Int = 40000000
					}
				],
				MultipleEmbeddedNullable =
				[
					new()
					{
						Byte = 20,
						Short = 2000,
						Int = 20000000
					},
					null,
					new()
					{
						Byte = 40,
						Short = 4000,
						Int = 40000000
					}
				],
				MultipleEmbeddedNull = null
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ContainerEncrypted));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			ContainerEncrypted Obj2 = (ContainerEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEx.Same(Obj.ObjectId, GenObj.ObjectId);
			AssertEncrypted((GenericObject)GenObj["Embedded"], "Byte", 0);
			AssertEncrypted((GenericObject)GenObj["Embedded"], "Short", 100);
			AssertEncrypted((GenericObject)GenObj["Embedded"], "Int", 0);
			AssertEx.Same(null, GenObj["EmbeddedNull"]);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0), "Byte", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0), "Short", 100);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0), "Int", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1), "Byte", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1), "Short", 100);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1), "Int", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2), "Byte", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2), "Short", 100);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2), "Int", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0), "Byte", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0), "Short", 100);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0), "Int", 0);
			AssertEx.Same(null, ((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(1));
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2), "Byte", 0);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2), "Short", 100);
			AssertEncrypted((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2), "Int", 0);
			AssertEx.Same(null, GenObj["MultipleEmbeddedNull"]);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (ContainerEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(ContainerEncrypted Obj, ContainerEncrypted Obj2)
		{
			AssertEx.Same(Obj.ObjectId, Obj2.ObjectId);
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
		public async Task DBFiles_ObjSerialization_37_ArraysOfArraysEncrypted()
		{
			ArraysOfArraysEncrypted Obj = new()
			{
				Boolean = [[true, false], [false, true]],
				Byte = [[1, 2, 3], [2, 3, 4]],
				Short = [[1, 2, 3], [2, 3, 4]],
				Int = [[1, 2, 3], [2, 3, 4]],
				Long = [[1, 2, 3], [2, 3, 4]],
				SByte = [[1, 2, 3], [2, 3, 4]],
				UShort = [[1, 2, 3], [2, 3, 4]],
				UInt = [[1, 2, 3], [2, 3, 4]],
				ULong = [[1, 2, 3], [2, 3, 4]],
				Char = [['a', 'b', 'c', '☀'], ['a', 'b', 'c']],
				Decimal = [[1, 2, 3], [2, 3, 4]],
				Double = [[1, 2, 3], [2, 3, 4]],
				Single = [[1, 2, 3], [2, 3, 4]],
				String = [["a", "b", "c", "Today, there will be a lot of ☀."], ["a", "b", "c"]],
				CIStrings = [["a", "b", "c", "Today, there will be a lot of ☀."], ["a", "b", "c"]],
				DateTime = [[DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue], [DateTime.MinValue, DateTime.MaxValue]],
				TimeSpan = [[DateTime.Now.TimeOfDay, TimeSpan.Zero], [TimeSpan.MinValue, TimeSpan.MaxValue]],
				Guid = [[Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]],
				NormalEnum = [[NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4], [NormalEnum.Option1, NormalEnum.Option2]],
				FlagsEnum = [[FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3], [FlagsEnum.Option2, FlagsEnum.Option3]]
			};

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(ArraysOfArraysEncrypted));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			ArraysOfArraysEncrypted Obj2 = (ArraysOfArraysEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEx.Same(GenObj.CollectionName, "Default");
			AssertEncrypted(GenObj, "Boolean", 200);
			AssertEncrypted(GenObj, "Byte", 500);
			AssertEncrypted(GenObj, "Short", 200);
			AssertEncrypted(GenObj, "Int", 500);
			AssertEncrypted(GenObj, "Long", 200);
			AssertEncrypted(GenObj, "SByte", 500);
			AssertEncrypted(GenObj, "UShort", 200);
			AssertEncrypted(GenObj, "UInt", 500);
			AssertEncrypted(GenObj, "ULong", 200);
			AssertEncrypted(GenObj, "Char", 500);
			AssertEncrypted(GenObj, "Decimal", 200);
			AssertEncrypted(GenObj, "Double", 500);
			AssertEncrypted(GenObj, "Single", 200);
			AssertEncrypted(GenObj, "String", 500);
			AssertEncrypted(GenObj, "DateTime", 200);
			AssertEncrypted(GenObj, "TimeSpan", 500);
			AssertEncrypted(GenObj, "Guid", 200);
			AssertEncrypted(GenObj, "NormalEnum", 500);
			AssertEncrypted(GenObj, "CIStrings", 200);
			AssertEncrypted(GenObj, "FlagsEnum", 500);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			//Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			// Note: Enumerations are serialized as strings in the generic object, so serializations will not be equal.

			Reader.Restart(Data2, 0);
			Obj2 = (ArraysOfArraysEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			AssertBinaryLength(Data2, Reader);
		}

		private static void AssertEqual(ArraysOfArraysEncrypted Obj, ArraysOfArraysEncrypted Obj2)
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

		[TestMethod]
		public async Task DBFiles_ObjSerialization_38_StructsEncrypted()
		{
			StructsEncrypted Obj = new()
			{
				Duration = new Content.Duration(true, 1, 2, 3, 4, 5, 6)
			};

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = await provider.GetObjectSerializer(typeof(StructsEncrypted));
			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);
			StructsEncrypted Obj2 = (StructsEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.IsNotNull(Obj2.Duration);
			Assert.AreEqual(true, Obj2.Duration.Negation);
			Assert.AreEqual(1, Obj2.Duration.Years);
			Assert.AreEqual(2, Obj2.Duration.Months);
			Assert.AreEqual(3, Obj2.Duration.Days);
			Assert.AreEqual(4, Obj2.Duration.Hours);
			Assert.AreEqual(5, Obj2.Duration.Minutes);
			Assert.AreEqual(6, Obj2.Duration.Seconds);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_39_BinaryEncrypted()
		{
			DockerBlobEncrypted Obj = new()
			{
				ObjectId = Guid.NewGuid().ToString(),
				Function = HashFunction.SHA256,
				Digest =
				[
					0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
					10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
					20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
					30, 31
				],
				FileName = "Some file",
				AccountName = "Some user"
			};

			ObjectSerializer S = (ObjectSerializer)await provider.GetObjectSerializer(typeof(DockerBlobEncrypted));
			DebugSerializer Writer = new(new BinarySerializer(await S.CollectionName(Obj), Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(await S.CollectionName(Obj), Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			DockerBlobEncrypted Obj2 = (DockerBlobEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Convert.ToBase64String(Obj.Digest), Convert.ToBase64String(Obj2.Digest));
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId.ToString());
			AssertEncrypted(GenObj, "Function", 100);
			AssertEncrypted(GenObj, "Digest", 200);
			AssertEncrypted(GenObj, "FileName", 100);
			AssertEncrypted(GenObj, "AccountName", 200);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (DockerBlobEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Convert.ToBase64String(Obj.Digest), Convert.ToBase64String(Obj2.Digest));
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data2, Reader);
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_40_BinaryNullEncrypted()
		{
			DockerBlobEncrypted Obj = new()
			{
				ObjectId = Guid.NewGuid().ToString(),
				FileName = "Some file",
				AccountName = "Some user"
			};

			ObjectSerializer S = (ObjectSerializer)await provider.GetObjectSerializer(typeof(DockerBlobEncrypted));
			DebugSerializer Writer = new(new BinarySerializer(await S.CollectionName(Obj), Encoding.UTF8), ConsoleOut.Writer);

			await S.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			WriteData(Data);

			ConsoleOut.WriteLine();
			ConsoleOut.WriteLine();

			DebugDeserializer Reader = new(new BinaryDeserializer(await S.CollectionName(Obj), Encoding.UTF8, Data, uint.MaxValue), ConsoleOut.Writer);

			DockerBlobEncrypted Obj2 = (DockerBlobEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Obj.Digest, Obj2.Digest);
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new(provider);
			GenericObject GenObj = (GenericObject)await GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId.ToString());
			AssertEncrypted(GenObj, "Function", 100);
			AssertEncrypted(GenObj, "Digest", 200);
			AssertEncrypted(GenObj, "FileName", 100);
			AssertEncrypted(GenObj, "AccountName", 200);

			Writer.Restart();

			await GS.Serialize(Writer, false, false, GenObj, null);

			byte[] Data2 = Writer.GetSerialization();
			WriteData(Data2);

			Assert.AreEqual(Data.Length, Data2.Length, "Generic Object serialization length does not match typed object serialization.");
			Reader.Restart(Data2, 0);
			Obj2 = (DockerBlobEncrypted)await S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Function, Obj2.Function);
			Assert.AreEqual(Obj.Digest, Obj2.Digest);
			Assert.AreEqual(Obj.FileName, Obj2.FileName);
			Assert.AreEqual(Obj.AccountName, Obj2.AccountName);

			AssertBinaryLength(Data2, Reader);
		}

		// TODO: Objects, by reference, nullable (incl. null strings, arrays)
		// TODO: Multidimensional arrays
	}
}
