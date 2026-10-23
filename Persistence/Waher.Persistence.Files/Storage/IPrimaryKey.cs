using System;
using Waher.Persistence.Files.Storage.PrimaryKeys;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// Interface for primary keys.
	/// </summary>
	public interface IPrimaryKey : IComparable<IPrimaryKey>
	{
		/// <summary>
		/// If the primary key is empty.
		/// </summary>
		bool Empty { get; }

		/// <summary>
		/// Corresponding Object ID
		/// </summary>
		object ObjectId { get; }

		/// <summary>
		/// The primary key, as a GUID.
		/// </summary>
		Guid Guid { get; }

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		void Set(Guid Id);

		/// <summary>
		/// Sets a new primary key.
		/// </summary>
		/// <param name="Id">Primary key</param>
		void Set(string Id);

		/// <summary>
		/// Represents a primary key
		/// </summary>
		/// <param name="Id">Primary key</param>
		void Set(byte[] Id);

		/// <summary>
		/// Represents a primary key
		/// </summary>
		/// <param name="Bin">Binary data</param>
		/// <param name="Offset">Offset into buffer where primary key is stored.</param>
		/// <param name="Length">Length of primary key.</param>
		void Set(byte[] Bin, int Offset, int Length);

		/// <summary>
		/// Sets the value of the primary key to the value of another primary key object.
		/// </summary>
		/// <param name="PrimaryKey"></param>
		void Set(IPrimaryKey PrimaryKey);

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		void CopyTo(GuidPrimaryKey PrimaryKey);

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		void CopyTo(StringPrimaryKey PrimaryKey);

		/// <summary>
		/// Copyes the value of the primary key to another primery key object.
		/// </summary>
		/// <param name="PrimaryKey">Recipient of the primary key value.</param>
		void CopyTo(BinaryPrimaryKey PrimaryKey);

		/// <summary>
		/// Clears the primary key.
		/// </summary>
		void Clear();

		/// <summary>
		/// Clones the primary key object.
		/// </summary>
		/// <returns>Cloned primary key.</returns>
		IPrimaryKey Clone();

		/// <summary>
		/// Increments the primary key to the next possible value.
		/// </summary>
		/// <returns>If the value could be incremented.</returns>
		bool Increment();

		/// <summary>
		/// Increments the primary key to the next possible value.
		/// </summary>
		/// <returns>If the value could be decremented.</returns>
		bool Decrement();
	}
}
