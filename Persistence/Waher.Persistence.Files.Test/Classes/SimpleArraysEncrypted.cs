using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class SimpleArraysEncrypted
	{
		[ObjectId]
		public Guid ObjectId;

		[Encrypted(200)]
		public bool[] Boolean;

		[Encrypted(500)]
		public byte[] Byte;

		[Encrypted(200)]
		public short[] Short;

		[Encrypted(500)]
		public int[] Int;

		[Encrypted(200)]
		public long[] Long;

		[Encrypted(500)]
		public sbyte[] SByte;

		[Encrypted(200)]
		public ushort[] UShort;

		[Encrypted(500)]
		public uint[] UInt;

		[Encrypted(200)]
		public ulong[] ULong;

		[Encrypted(500)]
		public char[] Char;

		[Encrypted(200)]
		public decimal[] Decimal;

		[Encrypted(500)]
		public double[] Double;

		[Encrypted(200)]
		public float[] Single;

		[Encrypted(500)]
		public string[] String;

		[Encrypted(200)]
		public DateTime[] DateTime;

		[Encrypted(500)]
		public TimeSpan[] TimeSpan;

		[Encrypted(200)]
		public Guid[] Guid;

		[Encrypted(500)]
		public NormalEnum[] NormalEnum;

		[Encrypted(200)]
		public FlagsEnum[] FlagsEnum;

		[Encrypted(500)]
		public CaseInsensitiveString[] CIStrings;

		public SimpleArraysEncrypted()
		{
		}
	}
}
