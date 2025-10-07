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
	public class DefaultEncrypted
	{
		[ObjectId]
		public Guid ObjectId;

		[Encrypted]
		[DefaultValue(true)]
		public bool Boolean1 = true;

		[Encrypted(100)]
		[DefaultValue(false)]
		public bool Boolean2 = false;

		[Encrypted]
		[DefaultValue(10)]
		public byte Byte = 10;

		[Encrypted(100)]
		[DefaultValue(10)]
		public short Short = 10;

		[Encrypted]
		[DefaultValue(10)]
		public int Int = 10;

		[Encrypted(100)]
		[DefaultValue(10)]
		public long Long = 10;

		[Encrypted]
		[DefaultValue(10)]
		public sbyte SByte = 10;

		[Encrypted(100)]
		[DefaultValue(10)]
		public ushort UShort = 10;

		[Encrypted]
		[DefaultValue(10)]
		public uint UInt = 10;

		[Encrypted(100)]
		[DefaultValue(10)]
		public ulong ULong = 10;

		[Encrypted]
		[DefaultValue('x')]
		public char Char = 'x';

		[Encrypted(100)]
		[DefaultValue(10)]
		public decimal Decimal = 10;

		[Encrypted]
		[DefaultValue(10)]
		public double Double = 10;

		[Encrypted(100)]
		[DefaultValue(10)]
		public float Single = 10;

		[Encrypted]
		[DefaultValueStringEmpty]
		public string String = string.Empty;

		[Encrypted(100)]
		[DefaultValueDateTimeMinValue]
		public DateTime DateTime = DateTime.MinValue;

		[Encrypted]
		[DefaultValueTimeSpanMinValue]
		public TimeSpan TimeSpan = TimeSpan.MinValue;

		[Encrypted(100)]
		[DefaultValueGuidEmpty]
		public Guid Guid = Guid.Empty;

		[Encrypted]
		[DefaultValue(NormalEnum.Option1)]
		public NormalEnum NormalEnum = NormalEnum.Option1;

		[Encrypted(100)]
		[DefaultValue(FlagsEnum.Option1 | FlagsEnum.Option2)]
		public FlagsEnum FlagsEnum = FlagsEnum.Option1 | FlagsEnum.Option2;

		[Encrypted]
		[DefaultValueNull]
		public string String2 = null;

		public DefaultEncrypted()
		{
		}
	}
}
