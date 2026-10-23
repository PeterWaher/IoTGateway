using System;
using System.Text;

namespace Waher.Persistence.Files.Storage.PrimaryKeys
{
	/// <summary>
	/// Represents binary primary keys
	/// </summary>
	public class BinaryPrimaryKey : IPrimaryKey
	{
		private readonly IndexRecords records;
		private byte[] bin;
		private int len, maxLen;
		private bool empty;

		/// <summary>
		/// Represents binary primary keys
		/// </summary>
		/// <param name="Records">Index records.</param>
		public BinaryPrimaryKey(IndexRecords Records)
		{
			this.records = Records;
			this.bin = Array.Empty<byte>();
			this.len = this.maxLen = 0;
			this.empty = true;
		}

		/// <summary>
		/// Represents binary primary keys
		/// </summary>
		/// <param name="Records">Index records.</param>
		/// <param name="Id">Primary key</param>
		public BinaryPrimaryKey(IndexRecords Records, Guid Id)
		{
			this.records = Records;
			this.bin = Id.ToByteArray();
			this.len = this.maxLen = this.bin.Length;
			this.empty = false;
		}

		/// <summary>
		/// Represents binary primary keys
		/// </summary>
		/// <param name="Records">Index records.</param>
		/// <param name="Id">Primary key</param>
		public BinaryPrimaryKey(IndexRecords Records, string Id)
		{
			this.records = Records;
			this.bin = Encoding.UTF8.GetBytes(Id);
			this.len = this.maxLen = this.bin.Length;
			this.empty = false;
		}

		/// <summary>
		/// Represents binary primary keys
		/// </summary>
		/// <param name="Records">Index records.</param>
		/// <param name="Id">Primary key</param>
		public BinaryPrimaryKey(IndexRecords Records, byte[] Id)
		{
			this.records = Records;
			this.bin = (byte[])Id.Clone();
			this.len = this.maxLen = this.bin.Length;
			this.empty = false;
		}

		/// <summary>
		/// Represents binary primary keys
		/// </summary>
		/// <param name="Records">Index records.</param>
		/// <param name="Bin">Binary data</param>
		/// <param name="Offset">Offset into buffer where primary key is stored.</param>
		/// <param name="Length">Length of primary key.</param>
		public BinaryPrimaryKey(IndexRecords Records, byte[] Bin, int Offset, int Length)
		{
			this.records = Records;
			this.bin = new byte[Length];
			Buffer.BlockCopy(Bin, Offset, this.bin, 0, Length);
			this.len = this.maxLen = Length;
			this.empty = Length > 0;
		}

		/// <summary>
		/// If the primary key is empty.
		/// </summary>
		public bool Empty => this.empty;

		/// <summary>
		/// Primary key value
		/// </summary>
		public Guid Guid => new Guid(this.bin.AsSpan(0, this.len));

		/// <summary>
		/// Length of key
		/// </summary>
		public int Length => this.len;

		/// <summary>
		/// Byte array representing the key.
		/// </summary>
		public byte[] Binary => this.bin;

		/// <summary>
		/// Corresponding Object ID
		/// </summary>
		public object ObjectId
		{
			get
			{
				if (this.len == this.maxLen)
					return this.bin.Clone();
				else
				{
					byte[] Result = new byte[this.len];
					Buffer.BlockCopy(this.bin, 0, Result, 0, this.len);
					return Result;
				}
			}
		}

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(Guid Id)
		{
			return false;
		}

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(string Id)
		{
			return false;
		}

		/// <summary>
		/// Represents a primary key
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(byte[] Id)
		{
			this.len = Id.Length;
			if (this.maxLen < this.len)
			{
				this.bin = (byte[])Id.Clone();
				this.maxLen = this.len;
			}
			else
				Buffer.BlockCopy(Id, 0, this.bin, 0, this.len);

			this.empty = false;

			return true;
		}

		/// <summary>
		/// Represents a primary key
		/// </summary>
		/// <param name="Bin">Binary data</param>
		/// <param name="Offset">Offset into buffer where primary key is stored.</param>
		/// <param name="Length">Length of primary key.</param>
		public bool Set(byte[] Bin, int Offset, int Length)
		{
			this.len = Length;

			if (this.len == 0)
				this.empty = true;
			else
			{
				if (this.maxLen < Length)
				{
					this.bin = new byte[Length];
					this.maxLen = Length;
				}

				Buffer.BlockCopy(Bin, Offset, this.bin, 0, Length);
				this.empty = false;
			}

			return true;
		}

		/// <summary>
		/// Sets the value of the primary key to the value of another primary key object.
		/// </summary>
		/// <param name="PrimaryKey"></param>
		/// <returns>If successful.</returns>
		public bool Set(IPrimaryKey PrimaryKey)
		{
			return PrimaryKey.CopyTo(this);
		}

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		/// <returns>If successful.</returns>
		public bool CopyTo(GuidPrimaryKey PrimaryKey)
		{
			return false;
		}

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		/// <returns>If successful.</returns>
		public bool CopyTo(StringPrimaryKey PrimaryKey)
		{
			return false;
		}

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		/// <returns>If successful.</returns>
		public bool CopyTo(BinaryPrimaryKey PrimaryKey)
		{
			if (this.empty)
			{
				PrimaryKey.Clear();
				return true;
			}
			else
				return PrimaryKey.Set(this.bin, 0, this.len);
		}

		/// <summary>
		/// Clears the primary key.
		/// </summary>
		public void Clear()
		{
			this.empty = true;
			this.len = 0;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Convert.ToBase64String(this.bin, 0, this.len);
		}

		/// <summary>
		/// Compares the primary key to another.
		/// </summary>
		/// <param name="Other">Other primary key.</param>
		/// <returns>Comparison result:
		/// 0 = equal, 
		/// &lt;0 = key is less than the other key, 
		/// &gt;0 = the key is greater than the other key
		/// </returns>
		/// <exception cref="NotImplementedException"></exception>
		public int CompareTo(IPrimaryKey Other)
		{
			if (Other is BinaryPrimaryKey Typed)
				return this.records.Compare(this.bin, this.len, Typed.bin, Typed.len);
			else
				return this.GetType().FullName.CompareTo(Other.GetType().FullName);
		}

		/// <summary>
		/// Increments the primary key to the next possible value.
		/// </summary>
		/// <returns>If the value could be incremented.</returns>
		public bool Increment()
		{
			return false;
		}

		/// <summary>
		/// Increments the primary key to the next possible value.
		/// </summary>
		public bool Decrement()
		{
			return false;
		}

		/// <summary>
		/// Clones the primary key object.
		/// </summary>
		/// <returns>Cloned primary key.</returns>
		public IPrimaryKey Clone()
		{
			if (this.empty)
				return new BinaryPrimaryKey(this.records);
			else
				return new BinaryPrimaryKey(this.records, this.bin, 0, this.len);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is BinaryPrimaryKey Typed))
				return false;

			if (this.len != Typed.len)
				return false;

			for (int i = 0; i < this.len; i++)
			{
				if (this.bin[i] != Typed.bin[i])
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = 0;

			for (int i = 0; i < this.len; i++)
				Result ^= Result << 5 ^ this.bin[i];

			return Result;
		}
	}
}
