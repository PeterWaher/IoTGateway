using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class NullableEncrypted
	{
		[ObjectId]
		public Guid ObjectId;

		[Encrypted]
		public bool? Boolean1;

		[Encrypted(100)]
		public bool? Boolean2;

		[Encrypted]
		public byte? Byte;

		[Encrypted(100)]
		public short? Short;

		[Encrypted]
		public int? Int;

		[Encrypted(100)]
		public long? Long;

		[Encrypted]
		public sbyte? SByte;

		[Encrypted(100)]
		public ushort? UShort;

		[Encrypted]
		public uint? UInt;

		[Encrypted(100)]
		public ulong? ULong;

		[Encrypted]
		public char? Char;

		[Encrypted(100)]
		public decimal? Decimal;

		[Encrypted]
		public double? Double;

		[Encrypted(100)]
		public float? Single;

		[Encrypted]
		public string String;

		[Encrypted(100)]
		public DateTime? DateTime;

		[Encrypted]
		public TimeSpan? TimeSpan;

		[Encrypted(100)]
		public Guid? Guid;

		[Encrypted]
		public NormalEnum? NormalEnum;

		[Encrypted(100)]
		public FlagsEnum? FlagsEnum;

		public NullableEncrypted()
		{
		}
	}
}
