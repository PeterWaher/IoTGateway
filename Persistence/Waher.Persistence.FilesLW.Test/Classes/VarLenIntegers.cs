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
	public class VarLenIntegers
	{
		[ObjectId]
		public Guid ObjectId;
		public short Short;
		public int Int;
		public long Long;
		public ushort UShort;
		public uint UInt;
		public ulong ULong;

		public VarLenIntegers()
		{
		}

		public override string ToString()
		{
			return this.ObjectId.ToString();
		}
	}
}
