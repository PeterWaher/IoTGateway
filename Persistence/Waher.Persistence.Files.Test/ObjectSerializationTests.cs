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

			Obj.Byte = 1;
			Obj.Short = -1;
			Obj.Int = -2;
			Obj.Long = -3;
			Obj.SByte = -4;
			Obj.UShort = 2;
			Obj.UInt = 3;
			Obj.ULong = 4;

			IObjectSerializer S = this.provider.GetObjectSerializer(typeof(Simple));
			BinarySerializer Writer = new BinarySerializer(Encoding.UTF8);

			S.Serialize(Writer, false, true, Obj);

			byte[] Data = Writer.GetSerialization();
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
