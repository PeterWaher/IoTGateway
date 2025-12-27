namespace Waher.Content.Zip
{
	/// <summary>
	/// Static class for computing CRC-32 checksums.
	/// </summary>
	public static class Crc32
	{
		private static readonly uint[] table = CreateTable();

		private static uint[] CreateTable()
		{
			uint[] Result = new uint[256];

			for (uint i = 0; i < 256; i++)
			{
				uint c = i;

				for (int k = 0; k < 8; k++)
					c = (c & 1) != 0 ? 0xedb88320u ^ (c >> 1) : c >> 1;

				Result[i] = c;
			}

			return Result;
		}

		/// <summary>
		/// Computes the CRC-32 of a byte array 
		/// (polynomial 0xEDB88320, init = 0xFFFFFFFF, xor out = 0xFFFFFFFF).
		/// </summary>
		/// <param name="Data"></param>
		/// <returns>CRC-32 of data.</returns>
		public static uint Compute(byte[] Data)
		{
			uint Crc = 0xffffffff;

			foreach (byte b in Data)
				Crc = table[(byte)(Crc ^ b)] ^ (Crc >> 8);

			return ~Crc;
		}

		/// <summary>
		/// Updates an existing CRC value with one byte (standard CRC-32 algorithm).
		/// </summary>
		/// <param name="Crc">Current CRC value.</param>
		/// <param name="Value">Additional byte value.</param>
		/// <returns>Updated CRC Value.</returns>
		public static uint Update(uint Crc, byte Value)
		{
			return table[(byte)(Crc ^ Value)] ^ (Crc >> 8);
		}

	}
}
