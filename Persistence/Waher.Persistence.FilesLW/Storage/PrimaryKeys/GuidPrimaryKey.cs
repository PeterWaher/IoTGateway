using System;

namespace Waher.Persistence.Files.Storage.PrimaryKeys
{
	/// <summary>
	/// Represents GUID primary keys
	/// </summary>
	public class GuidPrimaryKey : IPrimaryKey
	{
		private Guid guid;
		private bool empty;

		/// <summary>
		/// Represents GUID primary keys
		/// </summary>
		public GuidPrimaryKey()
		{
			this.guid = Guid.Empty;
			this.empty = true;
		}

		/// <summary>
		/// Represents GUID primary keys
		/// </summary>
		/// <param name="Id">Primary key</param>
		public GuidPrimaryKey(Guid Id)
		{
			this.guid = Id;
			this.empty = Id == Guid.Empty;
		}

		/// <summary>
		/// Represents GUID primary keys
		/// </summary>
		/// <param name="Id">Primary key</param>
		public GuidPrimaryKey(string Id)
		{
			this.guid = new Guid(Id);
			this.empty = this.guid == Guid.Empty;
		}

		/// <summary>
		/// Represents GUID primary keys
		/// </summary>
		/// <param name="Id">Primary key</param>
		public GuidPrimaryKey(byte[] Id)
		{
			this.guid = new Guid(Id);
			this.empty = this.guid == Guid.Empty;
		}

		/// <summary>
		/// Represents GUID primary keys
		/// </summary>
		/// <param name="Bin">Binary data</param>
		/// <param name="Offset">Offset into buffer where primary key is stored.</param>
		/// <param name="Length">Length of primary key.</param>
		public GuidPrimaryKey(byte[] Bin, int Offset, int Length)
		{
			this.guid = new Guid(Bin.AsSpan(Offset, Length));
			this.empty = this.guid == Guid.Empty;
		}

		/// <summary>
		/// If the primary key is empty.
		/// </summary>
		public bool Empty => this.empty;

		/// <summary>
		/// Primary key value
		/// </summary>
		public Guid Value
		{
			get => this.guid;
			set
			{
				this.guid = value;
				this.empty = this.guid == Guid.Empty;
			}
		}

		/// <summary>
		/// Primary key value
		/// </summary>
		public Guid Guid => this.guid;

		/// <summary>
		/// Corresponding Object ID
		/// </summary>
		public object ObjectId => this.guid;

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(Guid Id)
		{
			this.guid = Id;
			this.empty = Id == Guid.Empty;
			return true;
		}

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(string Id)
		{
			if (Guid.TryParse(Id, out Guid Parsed))
			{
				this.guid = Parsed;
				this.empty = Parsed == Guid.Empty;
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// Represents a primary key
		/// </summary>
		/// <param name="Id">Primary key</param>
		/// <returns>If successful.</returns>
		public bool Set(byte[] Id)
		{
			try
			{
				this.guid = new Guid(Id);
				this.empty = this.guid == Guid.Empty;
				return true;
			}
			catch (Exception)
			{
				return false;
			}
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
			try
			{
				this.guid = new Guid(Bin.AsSpan(Offset, Length));
				this.empty = this.guid == Guid.Empty;

				return true;
			}
			catch (Exception)
			{
				return false;
			}
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
			PrimaryKey.guid = this.guid;
			PrimaryKey.empty = this.empty;
			return true;
		}

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		/// <returns>If successful.</returns>
		public bool CopyTo(StringPrimaryKey PrimaryKey)
		{
			if (this.empty)
				PrimaryKey.Clear();
			else
				PrimaryKey.Value = this.guid.ToString();

			return true;
		}

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		/// <returns>If successful.</returns>
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
			this.guid = Guid.Empty;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.guid.ToString();
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
			if (Other is GuidPrimaryKey Typed)
				return this.guid.CompareTo(Typed.guid);
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
				return Searching.Comparison.Increment(ref this.guid);
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
				return Searching.Comparison.Decrement(ref this.guid);
		}

		/// <summary>
		/// Clones the primary key object.
		/// </summary>
		/// <returns>Cloned primary key.</returns>
		public IPrimaryKey Clone()
		{
			if (this.empty)
				return new GuidPrimaryKey();
			else
				return new GuidPrimaryKey(this.guid);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is GuidPrimaryKey Typed && this.guid.Equals(Typed.guid);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return this.guid.GetHashCode();
		}
	}
}
