using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Serialization;
using Waher.Persistence.Files.Serialization;

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
	public class ObjectSerializationTests
	{
		internal const string FileName = "Data\\Test.btree";
		internal const string BlobFileName = "Data\\Test.blob";
		internal const string NamesFileName = "Data\\Test.names";

		private FilesProvider provider;
		private ObjectBTreeFile file1;
		private ObjectBTreeFile file2;

		[ClassInitialize]
		public async void ClassInitialize()
		{
			if (File.Exists(BTreeTests.MasterFileName))
				File.Delete(BTreeTests.MasterFileName);

			if (File.Exists(BTreeTests.FileName))
				File.Delete(BTreeTests.FileName);

			if (File.Exists(BTreeTests.BlobFileName))
				File.Delete(BTreeTests.BlobFileName);

			if (File.Exists(BTreeTests.NamesFileName))
				File.Delete(BTreeTests.NamesFileName);

			if (File.Exists(FileName))
				File.Delete(FileName);

			if (File.Exists(BlobFileName))
				File.Delete(BlobFileName);

			if (File.Exists(NamesFileName))
				File.Delete(NamesFileName);

#if !LW
			this.provider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true, true);
#else
			this.provider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
#endif
			this.file1 = await this.provider.GetFile("Default");
			this.file2 = await this.provider.GetFile("Test");
		}

		[ClassCleanup]
		public void ClassCleanup()
		{
			if (this.provider != null)
			{
				this.provider.Dispose();
				this.provider = null;
			}

			this.file1 = null;
			this.file2 = null;
		}

		[TestMethod]
		public void Test_01_SimpleObject()
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
				Guid = Guid.NewGuid(),
				NormalEnum = NormalEnum.Option3,
				FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option4
			};

			Obj.TimeSpan = Obj.DateTime.TimeOfDay;

			Assert.IsTrue(Obj.ObjectId.Equals(Guid.Empty));

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Simple));
			
			Assert.IsTrue(S.TryGetFieldValue("Boolean1", Obj, out object Value));
			Assert.AreEqual(Obj.Boolean1, Value);
			Assert.IsTrue(S.TryGetFieldValue("Boolean2", Obj, out Value));
			Assert.AreEqual(Obj.Boolean2, Value);
			Assert.IsTrue(S.TryGetFieldValue("Byte", Obj, out Value));
			Assert.AreEqual(Obj.Byte, Value);
			Assert.IsTrue(S.TryGetFieldValue("Short", Obj, out Value));
			Assert.AreEqual(Obj.Short, Value);
			Assert.IsTrue(S.TryGetFieldValue("Int", Obj, out Value));
			Assert.AreEqual(Obj.Int, Value);
			Assert.IsTrue(S.TryGetFieldValue("Long", Obj, out Value));
			Assert.AreEqual(Obj.Long, Value);
			Assert.IsTrue(S.TryGetFieldValue("SByte", Obj, out Value));
			Assert.AreEqual(Obj.SByte, Value);
			Assert.IsTrue(S.TryGetFieldValue("UShort", Obj, out Value));
			Assert.AreEqual(Obj.UShort, Value);
			Assert.IsTrue(S.TryGetFieldValue("UInt", Obj, out Value));
			Assert.AreEqual(Obj.UInt, Value);
			Assert.IsTrue(S.TryGetFieldValue("ULong", Obj, out Value));
			Assert.AreEqual(Obj.ULong, Value);
			Assert.IsTrue(S.TryGetFieldValue("Char", Obj, out Value));
			Assert.AreEqual(Obj.Char, Value);
			Assert.IsTrue(S.TryGetFieldValue("Decimal", Obj, out Value));
			Assert.AreEqual(Obj.Decimal, Value);
			Assert.IsTrue(S.TryGetFieldValue("Double", Obj, out Value));
			Assert.AreEqual(Obj.Double, Value);
			Assert.IsTrue(S.TryGetFieldValue("Single", Obj, out Value));
			Assert.AreEqual(Obj.Single, Value);
			Assert.IsTrue(S.TryGetFieldValue("String", Obj, out Value));
			Assert.AreEqual(Obj.String, Value);
			Assert.IsTrue(S.TryGetFieldValue("DateTime", Obj, out Value));
			Assert.AreEqual(Obj.DateTime, Value);
			Assert.IsTrue(S.TryGetFieldValue("TimeSpan", Obj, out Value));
			Assert.AreEqual(Obj.TimeSpan, Value);
			Assert.IsTrue(S.TryGetFieldValue("Guid", Obj, out Value));
			Assert.AreEqual(Obj.Guid, Value);
			Assert.IsTrue(S.TryGetFieldValue("NormalEnum", Obj, out Value));
			Assert.AreEqual(Obj.NormalEnum, Value);
			Assert.IsTrue(S.TryGetFieldValue("FlagsEnum", Obj, out Value));
			Assert.AreEqual(Obj.FlagsEnum, Value);

			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			Simple Obj2 = (Simple)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, GenObj);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Simple)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		internal static void AssertEqual(Simple Obj, Simple Obj2)
		{
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

		internal static void AssertEqual(Simple Obj, GenericObject GenObj)
		{
			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean1, GenObj["Boolean1"]);
			Assert.AreEqual(Obj.Boolean2, GenObj["Boolean2"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(Obj.String, GenObj["String"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			Assert.AreEqual((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);
			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId);
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

		private void AssertBinaryLength(byte[] Data, BinaryDeserializer Reader)
		{
			Reader.Restart(Data, 0);

			Guid ObjectId = Reader.ReadGuid();
			ulong Len = Reader.ReadVariableLengthUInt64();

			Assert.AreEqual(Data.Length - 16 - this.VariableULongLen(Len), Len);
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
		public void Test_02_Nullable1()
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


			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Classes.Nullable));
			
			Assert.IsTrue(S.TryGetFieldValue("Boolean1", Obj, out object Value));
			Assert.AreEqual(Obj.Boolean1, Value);
			Assert.IsTrue(S.TryGetFieldValue("Boolean2", Obj, out Value));
			Assert.AreEqual(Obj.Boolean2, Value);
			Assert.IsTrue(S.TryGetFieldValue("Byte", Obj, out Value));
			Assert.AreEqual(Obj.Byte, Value);
			Assert.IsTrue(S.TryGetFieldValue("Short", Obj, out Value));
			Assert.AreEqual(Obj.Short, Value);
			Assert.IsTrue(S.TryGetFieldValue("Int", Obj, out Value));
			Assert.AreEqual(Obj.Int, Value);
			Assert.IsTrue(S.TryGetFieldValue("Long", Obj, out Value));
			Assert.AreEqual(Obj.Long, Value);
			Assert.IsTrue(S.TryGetFieldValue("SByte", Obj, out Value));
			Assert.AreEqual(Obj.SByte, Value);
			Assert.IsTrue(S.TryGetFieldValue("UShort", Obj, out Value));
			Assert.AreEqual(Obj.UShort, Value);
			Assert.IsTrue(S.TryGetFieldValue("UInt", Obj, out Value));
			Assert.AreEqual(Obj.UInt, Value);
			Assert.IsTrue(S.TryGetFieldValue("ULong", Obj, out Value));
			Assert.AreEqual(Obj.ULong, Value);
			Assert.IsTrue(S.TryGetFieldValue("Char", Obj, out Value));
			Assert.AreEqual(Obj.Char, Value);
			Assert.IsTrue(S.TryGetFieldValue("Decimal", Obj, out Value));
			Assert.AreEqual(Obj.Decimal, Value);
			Assert.IsTrue(S.TryGetFieldValue("Double", Obj, out Value));
			Assert.AreEqual(Obj.Double, Value);
			Assert.IsTrue(S.TryGetFieldValue("Single", Obj, out Value));
			Assert.AreEqual(Obj.Single, Value);
			Assert.IsTrue(S.TryGetFieldValue("String", Obj, out Value));
			Assert.AreEqual(Obj.String, Value);
			Assert.IsTrue(S.TryGetFieldValue("DateTime", Obj, out Value));
			Assert.AreEqual(Obj.DateTime, Value);
			Assert.IsTrue(S.TryGetFieldValue("TimeSpan", Obj, out Value));
			Assert.AreEqual(Obj.TimeSpan, Value);
			Assert.IsTrue(S.TryGetFieldValue("Guid", Obj, out Value));
			Assert.AreEqual(Obj.Guid, Value);
			Assert.IsTrue(S.TryGetFieldValue("NormalEnum", Obj, out Value));
			Assert.AreEqual(Obj.NormalEnum, Value);
			Assert.IsTrue(S.TryGetFieldValue("FlagsEnum", Obj, out Value));
			Assert.AreEqual(Obj.FlagsEnum, Value);

			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			Classes.Nullable Obj2 = (Classes.Nullable)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean1, GenObj["Boolean1"]);
			Assert.AreEqual(Obj.Boolean2, GenObj["Boolean2"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(Obj.String, GenObj["String"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			Assert.AreEqual(null, GenObj["FlagsEnum"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Classes.Nullable)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(Classes.Nullable Obj, Classes.Nullable Obj2)
		{
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

		[TestMethod]
		public void Test_03_Nullable2()
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

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Classes.Nullable));
			
			Assert.IsTrue(S.TryGetFieldValue("Boolean1", Obj, out object Value));
			Assert.AreEqual(Obj.Boolean1, Value);
			Assert.IsTrue(S.TryGetFieldValue("Boolean2", Obj, out Value));
			Assert.AreEqual(Obj.Boolean2, Value);
			Assert.IsTrue(S.TryGetFieldValue("Byte", Obj, out Value));
			Assert.AreEqual(Obj.Byte, Value);
			Assert.IsTrue(S.TryGetFieldValue("Short", Obj, out Value));
			Assert.AreEqual(Obj.Short, Value);
			Assert.IsTrue(S.TryGetFieldValue("Int", Obj, out Value));
			Assert.AreEqual(Obj.Int, Value);
			Assert.IsTrue(S.TryGetFieldValue("Long", Obj, out Value));
			Assert.AreEqual(Obj.Long, Value);
			Assert.IsTrue(S.TryGetFieldValue("SByte", Obj, out Value));
			Assert.AreEqual(Obj.SByte, Value);
			Assert.IsTrue(S.TryGetFieldValue("UShort", Obj, out Value));
			Assert.AreEqual(Obj.UShort, Value);
			Assert.IsTrue(S.TryGetFieldValue("UInt", Obj, out Value));
			Assert.AreEqual(Obj.UInt, Value);
			Assert.IsTrue(S.TryGetFieldValue("ULong", Obj, out Value));
			Assert.AreEqual(Obj.ULong, Value);
			Assert.IsTrue(S.TryGetFieldValue("Char", Obj, out Value));
			Assert.AreEqual(Obj.Char, Value);
			Assert.IsTrue(S.TryGetFieldValue("Decimal", Obj, out Value));
			Assert.AreEqual(Obj.Decimal, Value);
			Assert.IsTrue(S.TryGetFieldValue("Double", Obj, out Value));
			Assert.AreEqual(Obj.Double, Value);
			Assert.IsTrue(S.TryGetFieldValue("Single", Obj, out Value));
			Assert.AreEqual(Obj.Single, Value);
			Assert.IsTrue(S.TryGetFieldValue("String", Obj, out Value));
			Assert.AreEqual(Obj.String, Value);
			Assert.IsTrue(S.TryGetFieldValue("DateTime", Obj, out Value));
			Assert.AreEqual(Obj.DateTime, Value);
			Assert.IsTrue(S.TryGetFieldValue("TimeSpan", Obj, out Value));
			Assert.AreEqual(Obj.TimeSpan, Value);
			Assert.IsTrue(S.TryGetFieldValue("Guid", Obj, out Value));
			Assert.AreEqual(Obj.Guid, Value);
			Assert.IsTrue(S.TryGetFieldValue("NormalEnum", Obj, out Value));
			Assert.AreEqual(Obj.NormalEnum, Value);
			Assert.IsTrue(S.TryGetFieldValue("FlagsEnum", Obj, out Value));
			Assert.AreEqual(Obj.FlagsEnum, Value);

			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			Classes.Nullable Obj2 = (Classes.Nullable)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean1, GenObj["Boolean1"]);
			Assert.AreEqual(Obj.Boolean2, GenObj["Boolean2"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(Obj.String, GenObj["String"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(null, GenObj["NormalEnum"]);
			Assert.AreEqual((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Classes.Nullable)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		[TestMethod]
		public void Test_04_Default1()
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


			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Default));
			
			Assert.IsTrue(S.TryGetFieldValue("Boolean1", Obj, out object Value));
			Assert.AreEqual(Obj.Boolean1, Value);
			Assert.IsTrue(S.TryGetFieldValue("Boolean2", Obj, out Value));
			Assert.AreEqual(Obj.Boolean2, Value);
			Assert.IsTrue(S.TryGetFieldValue("Byte", Obj, out Value));
			Assert.AreEqual(Obj.Byte, Value);
			Assert.IsTrue(S.TryGetFieldValue("Short", Obj, out Value));
			Assert.AreEqual(Obj.Short, Value);
			Assert.IsTrue(S.TryGetFieldValue("Int", Obj, out Value));
			Assert.AreEqual(Obj.Int, Value);
			Assert.IsTrue(S.TryGetFieldValue("Long", Obj, out Value));
			Assert.AreEqual(Obj.Long, Value);
			Assert.IsTrue(S.TryGetFieldValue("SByte", Obj, out Value));
			Assert.AreEqual(Obj.SByte, Value);
			Assert.IsTrue(S.TryGetFieldValue("UShort", Obj, out Value));
			Assert.AreEqual(Obj.UShort, Value);
			Assert.IsTrue(S.TryGetFieldValue("UInt", Obj, out Value));
			Assert.AreEqual(Obj.UInt, Value);
			Assert.IsTrue(S.TryGetFieldValue("ULong", Obj, out Value));
			Assert.AreEqual(Obj.ULong, Value);
			Assert.IsTrue(S.TryGetFieldValue("Char", Obj, out Value));
			Assert.AreEqual(Obj.Char, Value);
			Assert.IsTrue(S.TryGetFieldValue("Decimal", Obj, out Value));
			Assert.AreEqual(Obj.Decimal, Value);
			Assert.IsTrue(S.TryGetFieldValue("Double", Obj, out Value));
			Assert.AreEqual(Obj.Double, Value);
			Assert.IsTrue(S.TryGetFieldValue("Single", Obj, out Value));
			Assert.AreEqual(Obj.Single, Value);
			Assert.IsTrue(S.TryGetFieldValue("String", Obj, out Value));
			Assert.AreEqual(Obj.String, Value);
			Assert.IsTrue(S.TryGetFieldValue("DateTime", Obj, out Value));
			Assert.AreEqual(Obj.DateTime, Value);
			Assert.IsTrue(S.TryGetFieldValue("TimeSpan", Obj, out Value));
			Assert.AreEqual(Obj.TimeSpan, Value);
			Assert.IsTrue(S.TryGetFieldValue("Guid", Obj, out Value));
			Assert.AreEqual(Obj.Guid, Value);
			Assert.IsTrue(S.TryGetFieldValue("NormalEnum", Obj, out Value));
			Assert.AreEqual(Obj.NormalEnum, Value);
			Assert.IsTrue(S.TryGetFieldValue("FlagsEnum", Obj, out Value));
			Assert.AreEqual(Obj.FlagsEnum, Value);

			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			Default Obj2 = (Default)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(null, GenObj["Boolean1"]);
			Assert.AreEqual(null, GenObj["Boolean2"]);
			Assert.AreEqual(null, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(null, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(null, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(null, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(null, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(null, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(null, GenObj["String"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(null, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(null, GenObj["NormalEnum"]);
			Assert.AreEqual((int)Obj.FlagsEnum, GenObj["FlagsEnum"]);
			Assert.AreEqual(null, GenObj["String2"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Default)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		internal static void AssertEqual(Default Obj, Default Obj2)
		{
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

		[TestMethod]
		public void Test_05_Default2()
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

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Default));
			
			Assert.IsTrue(S.TryGetFieldValue("Boolean1", Obj, out object Value));
			Assert.AreEqual(Obj.Boolean1, Value);
			Assert.IsTrue(S.TryGetFieldValue("Boolean2", Obj, out Value));
			Assert.AreEqual(Obj.Boolean2, Value);
			Assert.IsTrue(S.TryGetFieldValue("Byte", Obj, out Value));
			Assert.AreEqual(Obj.Byte, Value);
			Assert.IsTrue(S.TryGetFieldValue("Short", Obj, out Value));
			Assert.AreEqual(Obj.Short, Value);
			Assert.IsTrue(S.TryGetFieldValue("Int", Obj, out Value));
			Assert.AreEqual(Obj.Int, Value);
			Assert.IsTrue(S.TryGetFieldValue("Long", Obj, out Value));
			Assert.AreEqual(Obj.Long, Value);
			Assert.IsTrue(S.TryGetFieldValue("SByte", Obj, out Value));
			Assert.AreEqual(Obj.SByte, Value);
			Assert.IsTrue(S.TryGetFieldValue("UShort", Obj, out Value));
			Assert.AreEqual(Obj.UShort, Value);
			Assert.IsTrue(S.TryGetFieldValue("UInt", Obj, out Value));
			Assert.AreEqual(Obj.UInt, Value);
			Assert.IsTrue(S.TryGetFieldValue("ULong", Obj, out Value));
			Assert.AreEqual(Obj.ULong, Value);
			Assert.IsTrue(S.TryGetFieldValue("Char", Obj, out Value));
			Assert.AreEqual(Obj.Char, Value);
			Assert.IsTrue(S.TryGetFieldValue("Decimal", Obj, out Value));
			Assert.AreEqual(Obj.Decimal, Value);
			Assert.IsTrue(S.TryGetFieldValue("Double", Obj, out Value));
			Assert.AreEqual(Obj.Double, Value);
			Assert.IsTrue(S.TryGetFieldValue("Single", Obj, out Value));
			Assert.AreEqual(Obj.Single, Value);
			Assert.IsTrue(S.TryGetFieldValue("String", Obj, out Value));
			Assert.AreEqual(Obj.String, Value);
			Assert.IsTrue(S.TryGetFieldValue("DateTime", Obj, out Value));
			Assert.AreEqual(Obj.DateTime, Value);
			Assert.IsTrue(S.TryGetFieldValue("TimeSpan", Obj, out Value));
			Assert.AreEqual(Obj.TimeSpan, Value);
			Assert.IsTrue(S.TryGetFieldValue("Guid", Obj, out Value));
			Assert.AreEqual(Obj.Guid, Value);
			Assert.IsTrue(S.TryGetFieldValue("NormalEnum", Obj, out Value));
			Assert.AreEqual(Obj.NormalEnum, Value);
			Assert.IsTrue(S.TryGetFieldValue("FlagsEnum", Obj, out Value));
			Assert.AreEqual(Obj.FlagsEnum, Value);

			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			Default Obj2 = (Default)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean1, GenObj["Boolean1"]);
			Assert.AreEqual(Obj.Boolean2, GenObj["Boolean2"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(null, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(null, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(null, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(null, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(null, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(null, GenObj["Single"]);
			Assert.AreEqual(Obj.String, GenObj["String"]);
			Assert.AreEqual(null, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(null, GenObj["Guid"]);
			Assert.AreEqual(Obj.NormalEnum.ToString(), GenObj["NormalEnum"]);
			Assert.AreEqual(null, GenObj["FlagsEnum"]);
			Assert.AreEqual(Obj.String2, GenObj["String2"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Default)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		[TestMethod]
		public void Test_06_SimpleArrays()
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
				FlagsEnum = new FlagsEnum[] { FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3 }
			};

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(SimpleArrays));
			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			SimpleArrays Obj2 = (SimpleArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean, GenObj["Boolean"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(Obj.String, GenObj["String"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(new string[] { Obj.NormalEnum[0].ToString(), Obj.NormalEnum[1].ToString(), Obj.NormalEnum[2].ToString() }, GenObj["NormalEnum"]);
			Assert.AreEqual(new int[] { (int)Obj.FlagsEnum[0], (int)Obj.FlagsEnum[1] }, GenObj["FlagsEnum"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (SimpleArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(SimpleArrays Obj, SimpleArrays Obj2)
		{
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

		[TestMethod]
		public void Test_07_NullableArrays()
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


			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(NullableArrays));
			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			NullableArrays Obj2 = (NullableArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean, GenObj["Boolean"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(new string[] { Obj.NormalEnum[0].ToString(), null, Obj.NormalEnum[2].ToString() }, GenObj["NormalEnum"]);
			Assert.AreEqual(new int?[] { (int)Obj.FlagsEnum[0], null, (int)Obj.FlagsEnum[2] }, GenObj["FlagsEnum"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (NullableArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(NullableArrays Obj, NullableArrays Obj2)
		{
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

		[TestMethod]
		public void Test_08_Embedded()
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

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Container));
			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(Obj.ObjectId.Equals(Guid.Empty));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			Container Obj2 = (Container)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Embedded.Byte, ((GenericObject)GenObj["Embedded"])["Byte"]);
			Assert.AreEqual(Obj.Embedded.Short, ((GenericObject)GenObj["Embedded"])["Short"]);
			Assert.AreEqual(Obj.Embedded.Int, ((GenericObject)GenObj["Embedded"])["Int"]);
			Assert.AreEqual(Obj.EmbeddedNull, GenObj["EmbeddedNull"]);
			Assert.AreEqual(Obj.MultipleEmbedded[0].Byte, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0))["Byte"]);
			Assert.AreEqual(Obj.MultipleEmbedded[0].Short, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0))["Short"]);
			Assert.AreEqual(Obj.MultipleEmbedded[0].Int, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(0))["Int"]);
			Assert.AreEqual(Obj.MultipleEmbedded[1].Byte, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1))["Byte"]);
			Assert.AreEqual(Obj.MultipleEmbedded[1].Short, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1))["Short"]);
			Assert.AreEqual(Obj.MultipleEmbedded[1].Int, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(1))["Int"]);
			Assert.AreEqual(Obj.MultipleEmbedded[2].Byte, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2))["Byte"]);
			Assert.AreEqual(Obj.MultipleEmbedded[2].Short, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2))["Short"]);
			Assert.AreEqual(Obj.MultipleEmbedded[2].Int, ((GenericObject)((Array)GenObj["MultipleEmbedded"]).GetValue(2))["Int"]);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[0].Byte, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0))["Byte"]);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[0].Short, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0))["Short"]);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[0].Int, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(0))["Int"]);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[1], ((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(1));
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[2].Byte, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2))["Byte"]);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[2].Short, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2))["Short"]);
			Assert.AreEqual(Obj.MultipleEmbeddedNullable[2].Int, ((GenericObject)((Array)GenObj["MultipleEmbeddedNullable"]).GetValue(2))["Int"]);
			Assert.AreEqual(Obj.MultipleEmbeddedNull, GenObj["MultipleEmbeddedNull"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (Container)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(Container Obj, Container Obj2)
		{
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

		[TestMethod]
		public void Test_09_ObjectIdString()
		{
			ObjectIdString Obj = new ObjectIdString()
			{
				Value = 0x12345678
			};

			Assert.IsTrue(string.IsNullOrEmpty(Obj.ObjectId));

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(ObjectIdString));
			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsFalse(string.IsNullOrEmpty(Obj.ObjectId));

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			ObjectIdString Obj2 = (ObjectIdString)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Value, GenObj["Value"]);
			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId.ToString());

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (ObjectIdString)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(ObjectIdString Obj, ObjectIdString Obj2)
		{
			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public void Test_10_ObjectIdByteArray()
		{
			ObjectIdByteArray Obj = new ObjectIdByteArray()
			{
				Value = 0x12345678
			};

			Assert.IsNull(Obj.ObjectId);

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(ObjectIdByteArray));
			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			Assert.IsNotNull(Obj.ObjectId);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			ObjectIdByteArray Obj2 = (ObjectIdByteArray)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Value, GenObj["Value"]);
			Assert.AreEqual(Obj.ObjectId, GenObj.ObjectId.ToByteArray());

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (ObjectIdByteArray)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(ObjectIdByteArray Obj, ObjectIdByteArray Obj2)
		{
			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public void Test_11_LocalTypeName()
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

			IObjectSerializer S1 = this.provider.GetObjectSerializer(typeof(LocalNameSubclass1));
			IObjectSerializer S2 = this.provider.GetObjectSerializer(typeof(LocalNameSubclass2));
			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(LocalNameBase));
			BinarySerializer Writer1 = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);
			BinarySerializer Writer2 = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S1.Serialize(Writer1, false, false, Obj1);
			S2.Serialize(Writer2, false, false, Obj2);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			BinaryDeserializer Reader1 = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data1, true);
			BinaryDeserializer Reader2 = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data2, true);

			LocalNameSubclass1 Obj12 = (LocalNameSubclass1)S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			LocalNameSubclass2 Obj22 = (LocalNameSubclass2)S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);

			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj1 = (GenericObject)GS.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			GenericObject GenObj2 = (GenericObject)GS.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj1.Name, GenObj1["Name"]);
			Assert.AreEqual(Obj1.Value, GenObj1["Value"]);
			Assert.AreEqual(Obj1.ObjectId, GenObj1.ObjectId);
			Assert.AreEqual(Obj2.Name, GenObj2["Name"]);
			Assert.AreEqual(Obj2.Value, GenObj2["Value"]);
			Assert.AreEqual(Obj2.ObjectId, GenObj2.ObjectId);

			Writer1.Restart();
			Writer2.Restart();

			GS.Serialize(Writer1, false, false, GenObj1);
			GS.Serialize(Writer2, false, false, GenObj2);

			Data1 = Writer1.GetSerialization();
			Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			Obj12 = (LocalNameSubclass1)S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			Obj22 = (LocalNameSubclass2)S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);
			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);
		}

		private void AssertEqual(LocalNameSubclass1 Obj, LocalNameSubclass1 Obj2)
		{
			Assert.AreEqual(Obj.Name, Obj2.Name);
			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		private void AssertEqual(LocalNameSubclass2 Obj, LocalNameSubclass2 Obj2)
		{
			Assert.AreEqual(Obj.Name, Obj2.Name);
			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public void Test_12_FullTypeName()
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

			IObjectSerializer S1 = this.provider.GetObjectSerializer(typeof(FullNameSubclass1));
			IObjectSerializer S2 = this.provider.GetObjectSerializer(typeof(FullNameSubclass2));
			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(FullNameBase));
			BinarySerializer Writer1 = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);
			BinarySerializer Writer2 = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S1.Serialize(Writer1, false, false, Obj1);
			S2.Serialize(Writer2, false, false, Obj2);

			Assert.IsFalse(Obj1.ObjectId.Equals(Guid.Empty));
			Assert.IsFalse(Obj2.ObjectId.Equals(Guid.Empty));

			byte[] Data1 = Writer1.GetSerialization();
			byte[] Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			BinaryDeserializer Reader1 = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data1, true);
			BinaryDeserializer Reader2 = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data2, true);

			FullNameSubclass1 Obj12 = (FullNameSubclass1)S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			FullNameSubclass2 Obj22 = (FullNameSubclass2)S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);

			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj1 = (GenericObject)GS.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			GenericObject GenObj2 = (GenericObject)GS.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj1.Name, GenObj1["Name"]);
			Assert.AreEqual(Obj1.Value, GenObj1["Value"]);
			Assert.AreEqual(Obj1.ObjectId, GenObj1.ObjectId);
			Assert.AreEqual(Obj2.Name, GenObj2["Name"]);
			Assert.AreEqual(Obj2.Value, GenObj2["Value"]);
			Assert.AreEqual(Obj2.ObjectId, GenObj2.ObjectId);

			Writer1.Restart();
			Writer2.Restart();

			GS.Serialize(Writer1, false, false, GenObj1);
			GS.Serialize(Writer2, false, false, GenObj2);

			Data1 = Writer1.GetSerialization();
			Data2 = Writer2.GetSerialization();
			this.WriteData(Data1);
			this.WriteData(Data2);

			Reader1.Restart(Data1, 0);
			Reader2.Restart(Data2, 0);
			Obj12 = (FullNameSubclass1)S.Deserialize(Reader1, ObjectSerializer.TYPE_OBJECT, false);
			Obj22 = (FullNameSubclass2)S.Deserialize(Reader2, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj1, Obj12);
			this.AssertEqual(Obj2, Obj22);
			this.AssertBinaryLength(Data1, Reader1);
			this.AssertBinaryLength(Data2, Reader2);
		}

		private void AssertEqual(FullNameSubclass1 Obj, FullNameSubclass1 Obj2)
		{
			Assert.AreEqual(Obj.Name, Obj2.Name);
			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		private void AssertEqual(FullNameSubclass2 Obj, FullNameSubclass2 Obj2)
		{
			Assert.AreEqual(Obj.Name, Obj2.Name);
			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
		}

		[TestMethod]
		public void Test_13_CollectionTest()
		{
			CollectionTest Obj = new CollectionTest()
			{
				S1 = "Today, there will be a lot of ☀.",
				S2 = "Hello world.",
				S3 = "Testing, testing..."
			};

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(CollectionTest));
			BinarySerializer Writer = new BinarySerializer(((ObjectSerializer)S).CollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(((ObjectSerializer)S).CollectionName, Encoding.UTF8, Data, true);

			CollectionTest Obj2 = (CollectionTest)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Test");
			Assert.AreEqual(Obj.S1, GenObj["S1"]);
			Assert.AreEqual(Obj.S2, GenObj["S2"]);
			Assert.AreEqual(Obj.S3, GenObj["S3"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (CollectionTest)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(CollectionTest Obj, CollectionTest Obj2)
		{
			Assert.AreEqual(Obj.S1, Obj2.S1);
			Assert.AreEqual(Obj.S2, Obj2.S2);
			Assert.AreEqual(Obj.S3, Obj2.S3);
		}

		[TestMethod]
		public void Test_14_ArraysOfArrays()
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
				DateTime = new DateTime[][] { new DateTime[] { DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue }, new DateTime[] { DateTime.MinValue, DateTime.MaxValue } },
				TimeSpan = new TimeSpan[][] { new TimeSpan[] { DateTime.Now.TimeOfDay, TimeSpan.Zero }, new TimeSpan[] { TimeSpan.MinValue, TimeSpan.MaxValue } },
				Guid = new Guid[][] { new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }, new Guid[] { Guid.NewGuid(), Guid.NewGuid() } },
				NormalEnum = new NormalEnum[][] { new NormalEnum[] { NormalEnum.Option3, NormalEnum.Option1, NormalEnum.Option4 }, new NormalEnum[] { NormalEnum.Option1, NormalEnum.Option2 } },
				FlagsEnum = new FlagsEnum[][] { new FlagsEnum[] { FlagsEnum.Option1 | FlagsEnum.Option4, FlagsEnum.Option3 }, new FlagsEnum[] { FlagsEnum.Option2, FlagsEnum.Option3 } }
			};

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(ArraysOfArrays));
			BinarySerializer Writer = new BinarySerializer(this.provider.DefaultCollectionName, Encoding.UTF8, true);

			S.Serialize(Writer, false, false, Obj);

			byte[] Data = Writer.GetSerialization();
			this.WriteData(Data);

			BinaryDeserializer Reader = new BinaryDeserializer(this.provider.DefaultCollectionName, Encoding.UTF8, Data, true);

			ArraysOfArrays Obj2 = (ArraysOfArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);

			Reader.Restart(Data, 0);
			GenericObjectSerializer GS = new GenericObjectSerializer(this.provider);
			GenericObject GenObj = (GenericObject)GS.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(GenObj.CollectionName, "Default");
			Assert.AreEqual(Obj.Boolean, GenObj["Boolean"]);
			Assert.AreEqual(Obj.Byte, GenObj["Byte"]);
			Assert.AreEqual(Obj.Short, GenObj["Short"]);
			Assert.AreEqual(Obj.Int, GenObj["Int"]);
			Assert.AreEqual(Obj.Long, GenObj["Long"]);
			Assert.AreEqual(Obj.SByte, GenObj["SByte"]);
			Assert.AreEqual(Obj.UShort, GenObj["UShort"]);
			Assert.AreEqual(Obj.UInt, GenObj["UInt"]);
			Assert.AreEqual(Obj.ULong, GenObj["ULong"]);
			Assert.AreEqual(Obj.Char, GenObj["Char"]);
			Assert.AreEqual(Obj.Decimal, GenObj["Decimal"]);
			Assert.AreEqual(Obj.Double, GenObj["Double"]);
			Assert.AreEqual(Obj.Single, GenObj["Single"]);
			Assert.AreEqual(Obj.String, GenObj["String"]);
			Assert.AreEqual(Obj.DateTime, GenObj["DateTime"]);
			Assert.AreEqual(Obj.TimeSpan, GenObj["TimeSpan"]);
			Assert.AreEqual(Obj.Guid, GenObj["Guid"]);
			Assert.AreEqual(new string[][] { new string[] { Obj.NormalEnum[0][0].ToString(), Obj.NormalEnum[0][1].ToString(), Obj.NormalEnum[0][2].ToString() }, new string[] { Obj.NormalEnum[1][0].ToString(), Obj.NormalEnum[1][1].ToString() } }, GenObj["NormalEnum"]);
			Assert.AreEqual(new int[][] { new int[] { (int)Obj.FlagsEnum[0][0], (int)Obj.FlagsEnum[0][1] }, new int[] { (int)Obj.FlagsEnum[1][0], (int)Obj.FlagsEnum[1][1] } }, GenObj["FlagsEnum"]);

			Writer.Restart();

			GS.Serialize(Writer, false, false, GenObj);

			Data = Writer.GetSerialization();
			this.WriteData(Data);

			Reader.Restart(Data, 0);
			Obj2 = (ArraysOfArrays)S.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			this.AssertEqual(Obj, Obj2);
			this.AssertBinaryLength(Data, Reader);
		}

		private void AssertEqual(ArraysOfArrays Obj, ArraysOfArrays Obj2)
		{
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

		// TODO: Objects, by reference, nullable (incl. null strings, arrays)
		// TODO: Multidimensional arrays
	}
}
