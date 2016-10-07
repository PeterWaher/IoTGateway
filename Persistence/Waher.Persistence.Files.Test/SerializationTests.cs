using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Test
{
	public enum NormalEnum
	{
		Option1,
		Option2,
		Option3,
		Option4
	}

	public enum FlagsEnum
	{
		Option1 = 1,
		Option2 = 2,
		Option3 = 4,
		Option4 = 8
	}

	[TestFixture]
	public class SerializationTests
	{
		[Test]
		public void Test_01_Serialization()
		{
			BinarySerializer Serializer = new BinarySerializer(Encoding.UTF8);

			Serializer.Write(true);
			Serializer.Write(false);
			Serializer.Write((byte)25);
			Serializer.Write((short)1234);
			Serializer.Write((short)-1234);
			Serializer.Write((int)12345678);
			Serializer.Write((int)-12345678);
			Serializer.Write((long)1234567890123456789);
			Serializer.Write((long)-1234567890123456789);
			Serializer.Write((sbyte)-45);
			Serializer.Write((ushort)1234);
			Serializer.Write((uint)12345678);
			Serializer.Write((ulong)1234567890123456789);
			Serializer.Write((decimal)1234567890.123456789);
			Serializer.Write(1234567890.123456789);
			Serializer.Write(1234567890.123456789f);
			Serializer.Write(new DateTime(2016, 10, 06, 20, 32, 0));
			Serializer.Write(new TimeSpan(1, 2, 3, 4, 5));
			Serializer.Write('☀');
			Serializer.Write(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
			Serializer.Write("Today, there will be a lot of ☀.");
			Serializer.WriteVariableLengthUInt64(0);
			Serializer.WriteVariableLengthUInt64(100);
			Serializer.WriteVariableLengthUInt64(10000);
			Serializer.WriteVariableLengthUInt64(1000000);
			Serializer.WriteVariableLengthUInt64(100000000);
			Serializer.WriteVariableLengthUInt64(10000000000);
			Serializer.WriteVariableLengthUInt64(1000000000000);
			Serializer.WriteVariableLengthUInt64(100000000000000);
			Serializer.WriteVariableLengthUInt64(10000000000000000);
			Serializer.WriteBit(false);
			Serializer.WriteBit(true);
			Serializer.WriteBits(10, 4);
			Serializer.WriteBits(100, 7);
			Serializer.WriteBits(1000, 10);
			Serializer.WriteBits(10000, 14);
			Serializer.Write(NormalEnum.Option2);
			Serializer.Write(FlagsEnum.Option2 | FlagsEnum.Option4);

			byte[] Data = Serializer.GetSerialization();
			BinaryDeserializer Deserializer = new BinaryDeserializer(Encoding.UTF8, Data);

			Assert.AreEqual(true, Deserializer.ReadBoolean());
			Assert.AreEqual(false, Deserializer.ReadBoolean());
			Assert.AreEqual((byte)25, Deserializer.ReadByte());
			Assert.AreEqual((short)1234, Deserializer.ReadInt16());
			Assert.AreEqual((short)-1234, Deserializer.ReadInt16());
			Assert.AreEqual((int)12345678, Deserializer.ReadInt32());
			Assert.AreEqual((int)-12345678, Deserializer.ReadInt32());
			Assert.AreEqual((long)1234567890123456789, Deserializer.ReadInt64());
			Assert.AreEqual((long)-1234567890123456789, Deserializer.ReadInt64());
			Assert.AreEqual((sbyte)-45, Deserializer.ReadSByte());
			Assert.AreEqual((ushort)1234, Deserializer.ReadUInt16());
			Assert.AreEqual((uint)12345678, Deserializer.ReadUInt32());
			Assert.AreEqual((ulong)1234567890123456789, Deserializer.ReadUInt64());
			Assert.AreEqual((decimal)1234567890.123456789, Deserializer.ReadDecimal());
			Assert.AreEqual(1234567890.123456789, Deserializer.ReadDouble());
			Assert.AreEqual(1234567890.123456789f, Deserializer.ReadSingle());
			Assert.AreEqual(new DateTime(2016, 10, 06, 20, 32, 0), Deserializer.ReadDateTime());
			Assert.AreEqual(new TimeSpan(1, 2, 3, 4, 5), Deserializer.ReadTimeSpan());
			Assert.AreEqual('☀', Deserializer.ReadChar());
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, Deserializer.ReadByteArray());
			Assert.AreEqual("Today, there will be a lot of ☀.", Deserializer.ReadString());
			Assert.AreEqual(0, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(100, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(10000, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(1000000, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(100000000, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(10000000000, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(1000000000000, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(100000000000000, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(10000000000000000, Deserializer.ReadVariableLengthUInt64());
			Assert.AreEqual(false, Deserializer.ReadBit());
			Assert.AreEqual(true, Deserializer.ReadBit());
			Assert.AreEqual(10, Deserializer.ReadBits(4));
			Assert.AreEqual(100, Deserializer.ReadBits(7));
			Assert.AreEqual(1000, Deserializer.ReadBits(10));
			Assert.AreEqual(10000, Deserializer.ReadBits(14));
			Assert.AreEqual(NormalEnum.Option2, Deserializer.ReadEnum(typeof(NormalEnum)));
			Assert.AreEqual(FlagsEnum.Option2 | FlagsEnum.Option4, Deserializer.ReadEnum(typeof(FlagsEnum)));
		}
	}
}
