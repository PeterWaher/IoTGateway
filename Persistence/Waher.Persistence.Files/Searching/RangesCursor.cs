using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Filters;
using Waher.Persistence.Files.Serialization;

namespace Waher.Persistence.Files.Searching
{
	/// <summary>
	/// Provides a cursor that enumerates ranges of values using an index.
	/// </summary>
	/// <typeparam name="T">Class defining how to deserialize objects found.</typeparam>
	internal class RangesCursor<T> : ICursor<T>
	{
		private RangeInfo[] ranges;
		private IndexBTreeFile index;
		private IApplicableFilter filter;
		private ICursor<T> currentRange;
		private bool locked;
		private bool firstAscending;
		private bool[] ascending;
		private string[] fieldNames;

		/// <summary>
		/// Provides a cursor that joins results from multiple cursors. It only returns an object once, regardless of how many times
		/// it appears in the different child cursors.
		/// </summary>
		/// <param name="Index">Index.</param>
		/// <param name="Ranges">Ranges to enumerate.</param>
		/// <param name="AdditionalFilter">Additional filter.</param>
		/// <param name="Locked">If locked access is desired.</param>
		public RangesCursor(IndexBTreeFile Index, RangeInfo[] Ranges, IApplicableFilter AdditionalFilter, bool Locked)
		{
			this.index = Index;
			this.ranges = Ranges;
			this.filter = AdditionalFilter;
			this.locked = Locked;
			this.currentRange = null;
			this.ascending = Index.Ascending;
			this.fieldNames = Index.FieldNames;
			this.firstAscending = this.ascending[0];
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNextAsync()"/> to start the enumeration after creating or resetting it.</exception>
		public T Current
		{
			get
			{
				return this.CurrentCursor.Current;
			}
		}

		private ICursor<T> CurrentCursor
		{
			get
			{
				if (this.currentRange == null)
					throw new InvalidOperationException("Enumeration not started or has already ended.");
				else
					return this.currentRange;
			}
		}

		/// <summary>
		/// Serializer used to deserialize <see cref="Current"/>.
		/// </summary>
		public IObjectSerializer CurrentSerializer
		{
			get
			{
				return this.CurrentCursor.CurrentSerializer;
			}
		}

		/// <summary>
		/// If the curent object is type compatible with <typeparamref name="T"/> or not. If not compatible, <see cref="Current"/> 
		/// will be null, even if there exists an object at the current position.
		/// </summary>
		public bool CurrentTypeCompatible
		{
			get
			{
				return this.CurrentCursor.CurrentTypeCompatible;
			}
		}

		/// <summary>
		/// Gets the Object ID of the current object.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the enumeration has not started. 
		/// Call <see cref="MoveNext()"/> to start the enumeration after creating or resetting it.</exception>
		public Guid CurrentObjectId
		{
			get
			{
				return this.CurrentCursor.CurrentObjectId;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.currentRange != null)
			{
				this.currentRange.Dispose();
				this.currentRange = null;
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			while (true)
			{
				if (this.currentRange == null)
				{
					List<KeyValuePair<string, object>> SearchParameters = new List<KeyValuePair<string, object>>();
					List<Searching.IApplicableFilter> Filters = null;
					int i, c = this.ranges.Length;
					RangeInfo Range;

					for (i = 0; i < c; i++)
					{
						Range = this.ranges[i];

						if (Range.IsPoint)
							SearchParameters.Add(new KeyValuePair<string, object>(Range.FieldName, Range.Point));
						else if (this.ascending[i] == this.firstAscending)
						{
							if (this.firstAscending && Range.HasMin)
								SearchParameters.Add(new KeyValuePair<string, object>(Range.FieldName, Range.Min));
						}
						else
						{
							if (Filters == null)
								Filters = new List<IApplicableFilter>();

						}
					}

					throw new NotImplementedException();	// TODO
				}

				if (!await this.currentRange.MoveNextAsync())
				{
					this.currentRange.Dispose();
					this.currentRange = null;
					continue;
				}

				if (!this.currentRange.CurrentTypeCompatible)
					continue;

				return true;
			}
		}

		/// <summary>
		/// Advances the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the previous element; false if
		/// the enumerator has passed the beginning of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public Task<bool> MovePreviousAsync()
		{
			return this.MoveNextAsync();	// Range operator not ordered.
		}

	}
}
