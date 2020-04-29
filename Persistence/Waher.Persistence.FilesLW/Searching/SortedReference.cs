using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Sorted object reference.
	/// </summary>
	/// <typeparam name="T">Type of object being sorted.</typeparam>
	internal class SortedReference<T> : IComparable
	{
		private readonly byte[] key;
		private readonly IComparer<byte[]> comparer;
		private readonly T value;
		private readonly IObjectSerializer serializer;
		private readonly Guid id;

		/// <summary>
		/// Sorted object reference.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Comparer">Comparer</param>
		/// <param name="Value">Object value.</param>
		/// <param name="Serializer">Serializer used.</param>
		/// <param name="ObjectId">Object ID</param>
		public SortedReference(byte[] Key, IComparer<byte[]> Comparer, 
			T Value, IObjectSerializer Serializer, Guid ObjectId)
		{
			this.key = Key;
			this.comparer = Comparer;
			this.value = Value;
			this.serializer = Serializer;
			this.id = ObjectId;
		}

		public int CompareTo(object obj)
		{
			SortedReference<T> Rec = (SortedReference<T>)obj;
			return this.comparer.Compare(this.key, Rec.key);
		}

		/// <summary>
		/// Object value.
		/// </summary>
		public T Value => this.value;

		/// <summary>
		/// Serializer used.
		/// </summary>
		public IObjectSerializer Serializer => this.serializer;

		/// <summary>
		/// Object ID
		/// </summary>
		public Guid ObjectId => this.id;

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.id.ToString();
		}
	}
}
