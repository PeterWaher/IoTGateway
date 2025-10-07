using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[TypeName(TypeNameSerialization.FullName)]
	public class SimpleEncrypted
	{
		[ObjectId]
		public Guid ObjectId;

		[Encrypted(100)]
		public bool Boolean1;

		[Encrypted]
		public bool Boolean2;

		[Encrypted(100)]
		public byte Byte;

		[Encrypted]
		public short Short;

		[Encrypted(100)]
		public int Int;

		[Encrypted]
		public long Long;

		[Encrypted(100)]
		public sbyte SByte;

		[Encrypted]
		public ushort UShort;

		[Encrypted(100)]
		public uint UInt;

		[Encrypted]
		public ulong ULong;

		[Encrypted(100)]
		public char Char;

		[Encrypted]
		public decimal Decimal;

		[Encrypted(100)]
		public double Double;

		[Encrypted]
		public float Single;

		[Encrypted(100)]
		public string String;

		[Encrypted]
		public string ShortString;

		[Encrypted(100)]
		public DateTime DateTime;

		[Encrypted]
		public DateTimeOffset DateTimeOffset;

		[Encrypted(100)]
		public TimeSpan TimeSpan;

		[Encrypted]
		public Guid Guid;

		[Encrypted(100)]
		public NormalEnum NormalEnum;

		[Encrypted]
		public FlagsEnum FlagsEnum;

		[Encrypted(100)]
		public CaseInsensitiveString CIString;

		public SimpleEncrypted()
		{
		}

		public override string ToString()
		{
			return this.ObjectId.ToString();
		}
	}
}
