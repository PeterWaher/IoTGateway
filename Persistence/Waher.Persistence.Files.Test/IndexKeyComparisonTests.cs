using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Storage;

namespace Waher.Persistence.Files.Test
{
	[TestClass]
	public class IndexKeyComparisonTests
	{
		private FilesProvider provider;

		[ClassInitialize]
		public void ClassInitialize()
		{
			this.provider = new FilesProvider("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true, true);
		}

		[ClassCleanup]
		public void ClassCleanup()
		{
			this.provider.Dispose();
			this.provider = null;
		}

		[TestMethod]
		public void Test_01_NumericalFields()
		{
			object[,] Values = this.GetNumberValues();
			int x, y;
			int Rows = Values.GetLength(0);
			int Columns = Values.GetLength(1);

			for (y = 0; y < Rows; y++)
			{
				for (x = 0; x < Columns; x++)
				{
					BinarySerializer xWriter = new BinarySerializer(string.Empty, Encoding.UTF8);
					BinarySerializer yWriter = new BinarySerializer(string.Empty, Encoding.UTF8);

					object xValue = Values[y, x];
					object yValue = Values[x, y];
					int? Comparison = Searching.Comparison.Compare(xValue, yValue);

					Assert.IsTrue(Comparison.HasValue);
					Assert.AreEqual(Math.Sign(x.CompareTo(y)), Math.Sign(Comparison.Value), "x: " + x.ToString() + ", y: " + y.ToString());
				}
			}
		}

		[TestMethod]
		public void Test_02_Serialized_NumericalFields()
		{
			object[,] Values = this.GetNumberValues();
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

		private object[,] GetNumberValues()
		{
			return new object[,]
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
		}

	}
}
