using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Files.Serialization;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	public enum NormalEnum
	{
		Option1,
		Option2,
		Option3,
		Option4
	}

	[Flags]
	public enum FlagsEnum
	{
		Option1 = 1,
		Option2 = 2,
		Option3 = 4,
		Option4 = 8
	}

	[TestClass]
	public class BinarySerializationTests
	{
		[TestMethod]
		public void Test_01_Serialization()
		{
			BinarySerializer Serializer = new BinarySerializer(string.Empty, Encoding.UTF8, true);

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
			BinaryDeserializer Deserializer = new BinaryDeserializer(string.Empty, Encoding.UTF8, Data, true);

			AssertEx.Same(true, Deserializer.ReadBoolean());
			AssertEx.Same(false, Deserializer.ReadBoolean());
			AssertEx.Same((byte)25, Deserializer.ReadByte());
			AssertEx.Same((short)1234, Deserializer.ReadInt16());
			AssertEx.Same((short)-1234, Deserializer.ReadInt16());
			AssertEx.Same((int)12345678, Deserializer.ReadInt32());
			AssertEx.Same((int)-12345678, Deserializer.ReadInt32());
			AssertEx.Same((long)1234567890123456789, Deserializer.ReadInt64());
			AssertEx.Same((long)-1234567890123456789, Deserializer.ReadInt64());
			AssertEx.Same((sbyte)-45, Deserializer.ReadSByte());
			AssertEx.Same((ushort)1234, Deserializer.ReadUInt16());
			AssertEx.Same((uint)12345678, Deserializer.ReadUInt32());
			AssertEx.Same((ulong)1234567890123456789, Deserializer.ReadUInt64());
			AssertEx.Same((decimal)1234567890.123456789, Deserializer.ReadDecimal());
			AssertEx.Same(1234567890.123456789, Deserializer.ReadDouble());
			AssertEx.Same(1234567890.123456789f, Deserializer.ReadSingle());
			AssertEx.Same(new DateTime(2016, 10, 06, 20, 32, 0), Deserializer.ReadDateTime());
			AssertEx.Same(new TimeSpan(1, 2, 3, 4, 5), Deserializer.ReadTimeSpan());
			AssertEx.Same('☀', Deserializer.ReadChar());
			AssertEx.Same(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, Deserializer.ReadByteArray());
			AssertEx.Same("Today, there will be a lot of ☀.", Deserializer.ReadString());
			AssertEx.Same(0, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(100, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(10000, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(1000000, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(100000000, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(10000000000, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(1000000000000, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(100000000000000, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(10000000000000000, Deserializer.ReadVariableLengthUInt64());
			AssertEx.Same(false, Deserializer.ReadBit());
			AssertEx.Same(true, Deserializer.ReadBit());
			AssertEx.Same(10, Deserializer.ReadBits(4));
			AssertEx.Same(100, Deserializer.ReadBits(7));
			AssertEx.Same(1000, Deserializer.ReadBits(10));
			AssertEx.Same(10000, Deserializer.ReadBits(14));
			AssertEx.Same(NormalEnum.Option2, Deserializer.ReadEnum(typeof(NormalEnum)));
			AssertEx.Same(FlagsEnum.Option2 | FlagsEnum.Option4, Deserializer.ReadEnum(typeof(FlagsEnum)));
		}

	}
}
