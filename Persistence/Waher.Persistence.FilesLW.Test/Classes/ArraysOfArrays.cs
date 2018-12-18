using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class ArraysOfArrays
	{
		public bool[][] Boolean;
		public byte[][] Byte;
		public short[][] Short;
		public int[][] Int;
		public long[][] Long;
		public sbyte[][] SByte;
		public ushort[][] UShort;
		public uint[][] UInt;
		public ulong[][] ULong;
		public char[][] Char;
		public decimal[][] Decimal;
		public double[][] Double;
		public float[][] Single;
		public string[][] String;
		public DateTime[][] DateTime;
		public TimeSpan[][] TimeSpan;
		public Guid[][] Guid;
		public NormalEnum[][] NormalEnum;
		public CaseInsensitiveString[][] CIStrings;
		public FlagsEnum[][] FlagsEnum;
		public Embedded[][] MultipleEmbedded;
		public Embedded[][] MultipleEmbeddedNullable;
		public Embedded[][] MultipleEmbeddedNull;

		public ArraysOfArrays()
		{
		}
	}
}
