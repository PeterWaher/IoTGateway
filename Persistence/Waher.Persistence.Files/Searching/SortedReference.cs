using System;
using Waher.Persistence.Files.Storage;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Sorted object reference.
	/// </summary>
	/// <typeparam name="T">Type of object being sorted.</typeparam>
	internal class SortedReference<T> : IComparable
	{
		private readonly IPrimaryKey key;
		private readonly T value;
		private readonly IObjectSerializer serializer;
		private readonly IPrimaryKey currentPrimaryKey;

		/// <summary>
		/// Sorted object reference.
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Object value.</param>
		/// <param name="Serializer">Serializer used.</param>
		/// <param name="CurrentPrimaryKey">Current Primary key</param>
		public SortedReference(IPrimaryKey Key, T Value, IObjectSerializer Serializer, 
			IPrimaryKey CurrentPrimaryKey)
		{
			this.key = Key;
			this.value = Value;
			this.serializer = Serializer;
			this.currentPrimaryKey = CurrentPrimaryKey;
		}

		public int CompareTo(object obj)
		{
			if (obj is SortedReference<T> Rec)
				return this.key.CompareTo(Rec.key);
			else
				return this.GetType().FullName.CompareTo(obj.GetType().FullName);
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
		/// Primary key
		/// </summary>
		public IPrimaryKey PrimaryKey => this.currentPrimaryKey;

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.currentPrimaryKey.ToString();
		}
	}
}
