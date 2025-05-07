using System.IO;

namespace Waher.Networking.HTTP.HTTP2
{
	/// <summary>
	/// Serializes binary data
	/// </summary>
	public class BinaryWriter
	{
		private readonly MemoryStream ms;

		/// <summary>
		/// Serializes binary data
		/// </summary>
		public BinaryWriter()
		{
			this.ms = new MemoryStream();
		}

		/// <summary>
		/// Serializes binary data
		/// </summary>
		/// <param name="Capacity">Initial capacity</param>
		public BinaryWriter(int Capacity)
		{
			this.ms = new MemoryStream(Capacity);
		}

		/// <summary>
		/// Returns a byte array of written data.
		/// </summary>
		/// <returns>Byte array</returns>
		public byte[] ToArray()
		{
			return this.ms.ToArray();
		}

		/// <summary>
		/// Writes a byte value.
		/// </summary>
		public void WriteByte(byte Value)
		{
			this.ms.WriteByte(Value);
		}

		/// <summary>
		/// Writes a 16-bit unsigned value.
		/// </summary>
		public void WriteUInt16(ushort Value)
		{
			this.ms.WriteByte((byte)(Value >> 8));
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Writes a 32-bit unsigned value.
		/// </summary>
		public void WriteUInt32(uint Value)
		{
			this.ms.WriteByte((byte)(Value >> 24));
			this.ms.WriteByte((byte)(Value >> 16));
			this.ms.WriteByte((byte)(Value >> 8));
			this.ms.WriteByte((byte)Value);
		}

		/// <summary>
		/// Writes a key-value pair.
		/// </summary>
		/// <param name="Key">16-bit key.</param>
		/// <param name="Value">32-bit value.</param>
		public void WriteKeyValue(ushort Key, uint Value)
		{
			this.WriteUInt16(Key);
			this.WriteUInt32(Value);
		}
	}
}
