using System;
using System.Text;

namespace Waher.Persistence.Files.Storage.PrimaryKeys
{
	/// <summary>
	/// Represents string primary keys
	/// </summary>
	public class StringPrimaryKey : IPrimaryKey
	{
		private string id;
		private bool empty;

		/// <summary>
		/// Represents string primary keys
		/// </summary>
		public StringPrimaryKey()
		{
			this.id = string.Empty;
			this.empty = true;
		}

		/// <summary>
		/// Represents string primary keys
		/// </summary>
		/// <param name="Id">Primary key</param>
		public StringPrimaryKey(Guid Id)
		{
			this.id = Id.ToString();
			this.empty = false;
		}

		/// <summary>
		/// Represents string primary keys
		/// </summary>
		/// <param name="Id">Primary key</param>
		public StringPrimaryKey(string Id)
		{
			this.id = Id;
			this.empty = !string.IsNullOrEmpty(Id);
		}

		/// <summary>
		/// Represents string primary keys
		/// </summary>
		/// <param name="Id">Primary key</param>
		public StringPrimaryKey(byte[] Id)
		{
			this.id = Encoding.UTF8.GetString(Id);
			this.empty = false;
		}

		/// <summary>
		/// Represents string primary keys
		/// </summary>
		/// <param name="Bin">Binary data</param>
		/// <param name="Offset">Offset into buffer where primary key is stored.</param>
		/// <param name="Length">Length of primary key.</param>
		public StringPrimaryKey(byte[] Bin, int Offset, int Length)
		{
			this.id = Encoding.UTF8.GetString(Bin, Offset, Length);
			this.empty = false;
		}

		/// <summary>
		/// If the primary key is empty.
		/// </summary>
		public bool Empty => this.empty;

		/// <summary>
		/// Corresponding Object ID
		/// </summary>
		public object ObjectId => this.id;

		/// <summary>
		/// Primary key value
		/// </summary>
		public string Value
		{
			get => this.id;
			set
			{
				this.id = value;
				this.empty = !string.IsNullOrEmpty(this.id);
			}
		}

		/// <summary>
		/// Primary key value
		/// </summary>
		public Guid Guid => new Guid(this.id);

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(Guid Id)
		{
			this.id = Id.ToString();
			this.empty = false;
			return true;
		}

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(string Id)
		{
			this.id = Id;
			this.empty = !string.IsNullOrEmpty(Id);
			return true;
		}

		/// <summary>
		/// Represents a primary key
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(byte[] Id)
		{
			this.id = Encoding.UTF8.GetString(Id);
			this.empty = false;
			return true;
		}

		/// <summary>
		/// Represents a primary key
		/// </summary>
		/// <param name="Bin">Binary data</param>
		/// <param name="Offset">Offset into buffer where primary key is stored.</param>
		/// <param name="Length">Length of primary key.</param>
		/// <returns>If successful.</returns>
		public bool Set(byte[] Bin, int Offset, int Length)
		{
			this.id = Encoding.UTF8.GetString(Bin, Offset, Length);
			this.empty = Length > 0;
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
			PrimaryKey.Value = new Guid(this.id);
			return true;
		}

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		public bool CopyTo(StringPrimaryKey PrimaryKey)
		{
			PrimaryKey.id = this.id;
			PrimaryKey.empty = this.empty;
			return true;
		}

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		public bool CopyTo(BinaryPrimaryKey PrimaryKey)
		{
			return false;
		}

		/// <summary>
		/// Clears the primary key.
		/// </summary>
		public void Clear()
		{
			this.empty = true;
			this.id = string.Empty;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.id;
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
			if (Other is StringPrimaryKey Typed)
				return this.id.CompareTo(Typed.id);
			else
				return this.GetType().FullName.CompareTo(Other.GetType().FullName);
		}

		/// <summary>
		/// Increments the primary key to the next possible value.
		/// </summary>
		/// <returns>If the value could be incremented.</returns>
		public bool Increment()
		{
			if (this.empty)
				return false;
			else
				return Searching.Comparison.Increment(ref this.id);
		}

		/// <summary>
		/// Increments the primary key to the next possible value.
		/// </summary>
		/// <returns>If the value could be decremented.</returns>
		public bool Decrement()
		{
			if (this.empty)
				return false;
			else
				return Searching.Comparison.Decrement(ref this.id);
		}

		/// <summary>
		/// Clones the primary key object.
		/// </summary>
		/// <returns>Cloned primary key.</returns>
		public IPrimaryKey Clone()
		{
			if (this.empty)
				return new StringPrimaryKey();
			else
				return new StringPrimaryKey(this.id);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is StringPrimaryKey Typed && this.id.Equals(Typed.id);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.id.GetHashCode();
		}
	}
}
