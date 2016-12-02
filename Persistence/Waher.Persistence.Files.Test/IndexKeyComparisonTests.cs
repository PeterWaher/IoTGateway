using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
	public class IndexKeyComparisonTests
	{
		private FilesProvider provider;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			this.provider = new FilesProvider("Data", "Default", true);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			this.provider.Dispose();
			this.provider = null;
		}

		[Test]
		public void Test_01_NumericalFields()
		{
			object[,] Values = new object[,]
			{
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 },
				{ false, (byte)1, (short)2, (int)3, (long)4, (sbyte)5, (ushort)6, (uint)7, (ulong)8, (decimal)9, (double)10, (float)11 }
			};
			int x, y;
			int Rows = Values.GetLength(0);
			int Columns = Values.GetLength(1);
			IndexRecords Records = new IndexRecords(string.Empty, Encoding.UTF8, 1000, "Field");

			for (y = 0; y < Rows; y++)
			{
				for (x = 0; x < Columns; x++)
				{
					BinarySerializer xWriter = new BinarySerializer(string.Empty, Encoding.UTF8);
					BinarySerializer yWriter = new BinarySerializer(string.Empty, Encoding.UTF8);

					object xValue = Values[y, x];
					object yValue = Values[x, y];

					xWriter.WriteBit(true);
					Assert.IsTrue(Records.Serialize(xWriter, xValue));
					xWriter.Write(Guid.Empty);

					yWriter.WriteBit(true);
					Assert.IsTrue(Records.Serialize(yWriter, yValue));
					yWriter.Write(Guid.Empty);

					byte[] xBin = xWriter.GetSerialization();
					byte[] yBin = yWriter.GetSerialization();

					int Comparison = Records.Compare(xBin, yBin);

					Assert.AreEqual(Math.Sign(x.CompareTo(y)), Math.Sign(Comparison), "x: " + x.ToString() + ", y: " + y.ToString());
				}
			}
		}
	}
}
