using System;
using System.Collections.Generic;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.Files.Storage
{
	/// <summary>
	/// Maintains a list of locks granted for the current operation.
	/// </summary>
	internal class NestedLocks
	{
		private readonly ObjectBTreeFile file;
		private int count;
		private Dictionary<ObjectBTreeFile, int> additionalLocks = null;

		/// <summary>
		/// Maintains a list of locks acquired for the current operation.
		/// </summary>
		/// <param name="File">Top-most file in nested operation.</param>
		/// <param name="WriteLock">true=Write lock, false=Read lock.</param>
		public NestedLocks(ObjectBTreeFile File, bool WriteLock)
		{
			this.file = File;
			this.count = WriteLock ? 1 : -1;
		}

		/// <summary>
		/// Creates an instance of <see cref="NestedLocks"/>, if the serializer in
		/// <paramref name="Serializer"/> indicates nested serialization is to be
		/// performed.
		/// </summary>
		/// <param name="File">Top-most file in nested operation.</param>
		/// <param name="WriteLock">true=Write lock, false=Read lock.</param>
		/// <param name="Serializer">Top-most serializer in nested operation.</param>
		public static NestedLocks CreateIfNested(ObjectBTreeFile File, bool WriteLock, IObjectSerializer Serializer)
		{
			if (Serializer is ObjectSerializer ObjectSerializer)
				return CreateIfNested(File, WriteLock, ObjectSerializer);
			else
				return null;
		}

		/// <summary>
		/// Creates an instance of <see cref="NestedLocks"/>, if the serializer in
		/// <paramref name="Serializer"/> indicates nested serialization is to be
		/// performed.
		/// </summary>
		/// <param name="File">Top-most file in nested operation.</param>
		/// <param name="WriteLock">true=Write lock, false=Read lock.</param>
		/// <param name="Serializer">Top-most serializer in nested operation.</param>
		public static NestedLocks CreateIfNested(ObjectBTreeFile File, bool WriteLock, ObjectSerializer Serializer)
		{
			return Serializer.HasByReference ? new NestedLocks(File, WriteLock) : null;
		}

		/// <summary>
		/// Checks if a lock has been acquired during the current operation.
		/// </summary>
		/// <param name="File">File being quieried.</param>
		/// <param name="WriteLock">true=Write lock, false=Read lock.</param>
		/// <returns>If a lock has been acquired.</returns>
		public bool HasLock(ObjectBTreeFile File, out bool WriteLock)
		{
			if (File == this.file)
			{
				WriteLock = this.count > 0;
				return true;
			}

			if (this.additionalLocks is null)
			{
				WriteLock = false;
				return false;
			}

			if (this.additionalLocks.TryGetValue(File, out int i))
			{
				WriteLock = i > 0;
				return true;
			}
			else
			{
				WriteLock = false;
				return false;
			}
		}

		/// <summary>
		/// Adds information about a nested lock being acquired.
		/// </summary>
		/// <param name="File">Locked file.</param>
		/// <param name="WriteLock">true=Write lock, false=Read lock.</param>
		public void AddLock(ObjectBTreeFile File, bool WriteLock)
		{
			if (File == this.file)
			{
				if (WriteLock ^ (this.count > 0))
					throw new InvalidOperationException("Mismatching lock state.");

				if (WriteLock)
					this.count++;
				else
					this.count--;
			}
			else
			{
				if (this.additionalLocks is null)
				{
					if (this.additionalLocks is null)
						this.additionalLocks = new Dictionary<ObjectBTreeFile, int>();
				}

				if (this.additionalLocks.TryGetValue(File, out int i))
				{
					if (WriteLock ^ (i > 0))
						throw new InvalidOperationException("Mismatching lock state.");

					if (WriteLock)
						i++;
					else
						i--;
				}
				else
					i = WriteLock ? 1 : -1;

				this.additionalLocks[File] = i;
			}
		}

		/// <summary>
		/// Removes a nested lock.
		/// </summary>
		/// <param name="File">Locked file.</param>
		/// <returns>If lock was removed.</returns>
		public bool RemoveLock(ObjectBTreeFile File)
		{
			if (File == this.file)
			{
				if (this.count > 0)
					this.count--;
				else
					this.count++;

				return true;
			}
			else
			{
				if (this.additionalLocks is null)
					return false;

				if (!this.additionalLocks.TryGetValue(File, out int i))
					return false;

				if (i > 0)
					i--;
				else
					i++;

				if (i == 0)
					this.additionalLocks.Remove(File);
				else
					this.additionalLocks[File] = i;

				return true;
			}
		}

	}
}
