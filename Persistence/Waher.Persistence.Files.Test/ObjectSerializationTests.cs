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
			this.provider = new FilesProvider();
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

			Obj.Byte = 15;
			Obj.Short = -1234;
			Obj.Int = -23456789;
			Obj.Long = -345456456456456345;
			Obj.SByte = -45;
			Obj.UShort = 23456;
			Obj.UInt = 334534564;
			Obj.ULong = 4345345345345345;

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Simple));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8);

			S.Serialize(Writer, false, true, Obj);

			byte[] Data = Writer.GetSerialization();

			BinaryDeserializer Reader = new BinaryDeserializer(Encoding.UTF8, Data);

			Simple Obj2 = (Simple)S.Deserialize(Reader, null, false);

			Assert.AreEqual(Obj.Byte, Obj2.Byte);
			Assert.AreEqual(Obj.Short, Obj2.Short);
			Assert.AreEqual(Obj.Int, Obj2.Int);
			Assert.AreEqual(Obj.Long, Obj2.Long);
			Assert.AreEqual(Obj.SByte, Obj2.SByte);
			Assert.AreEqual(Obj.UShort, Obj2.UShort);
			Assert.AreEqual(Obj.UInt, Obj2.UInt);
			Assert.AreEqual(Obj.ULong, Obj2.ULong);
		}

		// TODO: Simple types
		// TODO: Boolean
		// TODO: Char
		// TODO: Decimal
		// TODO: Double
		// TODO: Single
		// TODO: String
		// TODO: Enum
		// TODO: DateTime
		// TODO: TimeStamp
		// TODO: Guid
		// TODO: Object IDs
		// TODO: Embedded Arrays (value elements, nullable elements)
		// TODO: Embedded objects (nullable)
		// TODO: Nullable<T>
		// TODO: Objects, by reference, nullable
		// TODO: Generic object reader/writer (with no type knowledge, for batch operations). If type not found when reading: Return generic object.
		// TODO: Type name serialization
		// TODO: Binary length (to skip block)
		// TODO: Default values.
		// TODO: Different Object ID field types (Guid, string, byte[])
	}
}
