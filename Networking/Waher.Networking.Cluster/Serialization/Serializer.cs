using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Waher.Networking.Cluster.Serialization
{
	/// <summary>
	/// Cluster serializer
	/// </summary>
	public class Serializer : IDisposable
	{
		private MemoryStream ms;

		/// <summary>
		/// Cluster serializer
		/// </summary>
		public Serializer()
		{
			this.ms = new MemoryStream();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.ms?.Dispose();
			this.ms = null;
		}

		/// <summary>
		/// Returns the binary output.
		/// </summary>
		/// <returns>Binary output.</returns>
		public byte[] ToArray()
		{
			return this.ms.ToArray();
		}

		/// <summary>
		/// Writes a byte to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteByte(byte Value)
		{
			this.ms.WriteByte(Value);
		}

		/// <summary>
		/// Writes a variable-length unsigned integer to the output.
		/// </summary>
		/// <param name="Value">Value</param>
		public void WriteVarUInt64(ulong Value)
		{
			byte b;

			do
			{
				b = (byte)(Value & 0x7f);
				Value >>= 7;
				if (Value != 0)
					b |= 0x80;

				this.ms.WriteByte(b);
			}
			while (Value != 0);
		}

		/// <summary>
		/// Writes a name to the output.
		/// </summary>
		/// <param name="Name">Name</param>
		public void WriteName(string Name)
		{
			byte[] Bin = Encoding.UTF8.GetBytes(Name);

			this.WriteVarUInt64((ulong)Bin.Length);
			this.ms.Write(Bin, 0, Bin.Length);
		}
	}
}
