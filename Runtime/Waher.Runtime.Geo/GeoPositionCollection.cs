using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Waher.Runtime.Collections;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// In-memory thread-safe geo-spatial collection of points (positions).
	/// </summary>
	public class GeoPositionCollection<T> : ICollection<T>
		where T : IGeoSpatialObjectReference
	{
		private readonly Dictionary<string, GeoNode> nodesById = new Dictionary<string, GeoNode>();
		private readonly object synchObj = new object();
		private volatile int token = 0;
		private int count = 0;
		private GeoNode root = null;

		private class GeoNode
		{
			public GeoNode(T Object)
			{
				this.Object = Object;
			}

			public T Object;
			public GeoNode NW;
			public GeoNode NE;
			public GeoNode SW;
			public GeoNode SE;
			public GeoNode Parent;
		}

		/// <summary>
		/// In-memory thread-safe geo-spatial collection of points (positions).
		/// </summary>
		public GeoPositionCollection()
		{
		}

		/// <summary>
		/// Number of items in collection
		/// </summary>
		public int Count
		{
			get
			{
				lock (this.synchObj)
				{
					return this.count;
				}
			}
		}

		/// <summary>
		/// If collection is read-only.
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Adds a geo-spatial object reference.
		/// </summary>
		/// <param name="Object">Object to add.</param>
		public void Add(T Object)
		{
			lock (this.synchObj)
			{
				this.AddLocked(Object);
			}
		}

		private void AddLocked(T Object)
		{
			if (this.nodesById.TryGetValue(Object.GeoId, out GeoNode Node))
			{
				if (Node.Object.Equals(Object))
					return;
				else
					throw new ArgumentException("An object with the same ID already exists in the collection.", nameof(Object));
			}

			Node = new GeoNode(Object);
			this.nodesById[Object.GeoId] = Node;
			this.AddNodeLocked(Node);
			this.token++;
			this.count++;
		}

		private void AddNodeLocked(GeoNode Node)
		{
			if (this.root is null)
				this.root = Node;
			else
			{
				GeoPosition Location = Node.Object.Location;
				GeoNode Loop = this.root;

				while (true)
				{
					if (Location.Latitude >= Loop.Object.Location.Latitude)
					{
						if (Location.Longitude >= Loop.Object.Location.Longitude)
						{
							if (Loop.NE is null)
							{
								Loop.NE = Node;
								Node.Parent = Loop;
								return;
							}
							else
								Loop = Loop.NE;
						}
						else
						{
							if (Loop.NW is null)
							{
								Loop.NW = Node;
								Node.Parent = Loop;
								return;
							}
							else
								Loop = Loop.NW;
						}
					}
					else
					{
						if (Location.Longitude >= Loop.Object.Location.Longitude)
						{
							if (Loop.SE is null)
							{
								Loop.SE = Node;
								Node.Parent = Loop;
								return;
							}
							else
								Loop = Loop.SE;
						}
						else
						{
							if (Loop.SW is null)
							{
								Loop.SW = Node;
								Node.Parent = Loop;
								return;
							}
							else
								Loop = Loop.SW;
						}
					}
				}
			}
		}

		/// <summary>
		/// Clears the collection.
		/// </summary>
		public void Clear()
		{
			lock (this.synchObj)
			{
				this.nodesById.Clear();
				this.root = null;
				this.count = 0;
				this.token++;
			}
		}

		/// <summary>
		/// Checks if the collection contains a geo-spatial object reference.
		/// </summary>
		/// <param name="Object">Geo-spatial object reference.</param>
		/// <returns>If the collection contains the object.</returns>
		public bool Contains(T Object)
		{
			lock (this.synchObj)
			{
				if (this.nodesById.TryGetValue(Object.GeoId, out GeoNode Node))
				{
					if (Node.Object.Equals(Object))
						return true;
					else
						return false;
				}
				else
					return false;
			}
		}

		/// <summary>
		/// Checks if the collection contains a geo-spatial object reference with a given ID.
		/// </summary>
		/// <param name="GeoId">ID of Geo-spatial object reference.</param>
		/// <returns>If the collection contains an object with the corresponding ID.</returns>
		public bool Contains(string GeoId)
		{
			lock (this.synchObj)
			{
				return this.nodesById.ContainsKey(GeoId);
			}
		}

		/// <summary>
		/// Tries to get a geo-spatial object reference, given its ID.
		/// </summary>
		/// <param name="GeoId">ID of geo-spatial object reference.</param>
		/// <param name="Object">Object, if found.</param>
		/// <returns>If a geo-spatial object reference was found.</returns>
		public bool TryGetObject(string GeoId, out T Object)
		{
			lock (this.synchObj)
			{
				if (!this.nodesById.TryGetValue(GeoId, out GeoNode Node))
				{
					Object = default;
					return false;
				}
				else
				{
					Object = Node.Object;
					return true;
				}
			}
		}

		/// <summary>
		/// Copies the contents of the collection to a destination array.
		/// </summary>
		/// <param name="Destination">Destination array</param>
		/// <param name="Offset">Index of first element.</param>
		public void CopyTo(T[] Destination, int Offset)
		{
			if (Destination is null)
				throw new ArgumentNullException(nameof(Destination));

			if (Offset < 0 || Offset >= Destination.Length)
				throw new ArgumentOutOfRangeException(nameof(Offset), "Offset must be a non-negative number  less than the size of the destination array.");

			lock (this.synchObj)
			{
				if (Offset + this.nodesById.Count > Destination.Length)
					throw new ArgumentException("Offset must allow all elements to be copied into the destination array.", nameof(Offset));

				foreach (GeoNode Node in this.nodesById.Values)
					Destination[Offset++] = Node.Object;
			}
		}

		/// <summary>
		/// Returns the elements of the collection as an array.
		/// </summary>
		/// <returns>Array of elements.</returns>
		public T[] ToArray()
		{
			lock (this.synchObj)
			{
				T[] Result = new T[this.count];
				int i = 0;

				foreach (GeoNode Node in this.nodesById.Values)
					Result[i++] = Node.Object;

				return Result;
			}
		}

		/// <summary>
		/// Returns an enumerator for all the elements in the collection.
		/// </summary>
		/// <returns>Enumerator.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			lock (this.synchObj)
			{
				return new GeoObjectEnumerator(this, this.nodesById.Values.GetEnumerator());
			}
		}

		/// <summary>
		/// Returns an enumerator for all the elements in the collection.
		/// </summary>
		/// <returns>Enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private class GeoObjectEnumerator : IEnumerator<T>
		{
			private readonly IEnumerator<GeoNode> e;
			private readonly GeoPositionCollection<T> collection;
			private readonly int token;

			public GeoObjectEnumerator(GeoPositionCollection<T> Collection, IEnumerator<GeoNode> Enumerator)
			{
				this.collection = Collection;
				this.token = Collection.token;
				this.e = Enumerator;
			}

			public T Current
			{
				get
				{
					lock (this.collection.synchObj)
					{
						if (this.token != this.collection.token)
							throw new InvalidOperationException("Collection has been modified.");

						return this.e.Current.Object;
					}
				}
			}

			object IEnumerator.Current => this.Current;

			public void Dispose()
			{
				lock (this.collection.synchObj)
				{
					this.e.Dispose();
				}
			}

			public bool MoveNext()
			{
				lock (this.collection.synchObj)
				{
					if (this.token != this.collection.token)
						throw new InvalidOperationException("Collection has been modified.");

					return this.e.MoveNext();
				}
			}

			public void Reset()
			{
				lock (this.collection.synchObj)
				{
					if (this.token != this.collection.token)
						throw new InvalidOperationException("Collection has been modified.");

					this.e.Reset();
				}
			}
		}

		/// <summary>
		/// Removes an object from the collection.
		/// </summary>
		/// <param name="Object">Object to remove.</param>
		/// <returns>If the object was found and moved.</returns>
		public bool Remove(T Object)
		{
			lock (this.synchObj)
			{
				return this.RemoveLocked(Object);
			}
		}

		/// <summary>
		/// Removes an object from the collection, given its ID.
		/// </summary>
		/// <param name="GeoId">ID of Object to remove.</param>
		/// <returns>If the object was found and moved.</returns>
		public bool Remove(string GeoId)
		{
			return this.Remove(GeoId, out _);
		}

		/// <summary>
		/// Removes an object from the collection, given its ID.
		/// </summary>
		/// <param name="GeoId">ID of Object to remove.</param>
		/// <param name="Object">Object that was removed.</param>
		/// <returns>If the object was found and moved.</returns>
		public bool Remove(string GeoId, out T Object)
		{
			lock (this.synchObj)
			{
				if (!this.nodesById.TryGetValue(GeoId, out GeoNode Node) ||
					!this.RemoveLocked(Node))
				{
					Object = default;
					return false;
				}
				else
				{
					Object = Node.Object;
					return true;
				}
			}
		}

		private bool RemoveLocked(T Object)
		{
			if (!this.nodesById.TryGetValue(Object.GeoId, out GeoNode Node))
				return false;

			if (!Node.Object.Equals(Object))
				return false;

			return this.RemoveLocked(Node);
		}

		private bool RemoveLocked(GeoNode Node)
		{
			T Object = Node.Object;

			this.nodesById.Remove(Object.GeoId);
			this.count--;
			this.token++;

			GeoNode Parent = Node.Parent;
			if (!(Parent is null))
			{
				Node.Parent = null;

				if (Parent.NW == Node)
					Parent.NW = null;
				else if (Parent.NE == Node)
					Parent.NE = null;
				else if (Parent.SW == Node)
					Parent.SW = null;
				else if (Parent.SE == Node)
					Parent.SE = null;
			}
			else
				this.root = null;

			if (!(Node.NW is null))
			{
				Node.NW.Parent = null;
				this.AddNodeLocked(Node.NW);
				Node.NW = null;
			}

			if (!(Node.NE is null))
			{
				Node.NE.Parent = null;
				this.AddNodeLocked(Node.NE);
				Node.NE = null;
			}

			if (!(Node.SW is null))
			{
				Node.SW.Parent = null;
				this.AddNodeLocked(Node.SW);
				Node.SW = null;
			}

			if (!(Node.SE is null))
			{
				Node.SE.Parent = null;
				this.AddNodeLocked(Node.SE);
				Node.SE = null;
			}

			return true;
		}

		/// <summary>
		/// Each time an object has moved, the collection must be notified. This is done
		/// by calling this method. The method will return true if the object was found,
		/// and consequently moved, and false if the object was not found, and therefore
		/// not moved.
		/// </summary>
		/// <param name="Object">Object that has moved.</param>
		/// <returns>If the object was found and moved.</returns>
		public bool Moved(T Object)
		{
			lock (this.synchObj)
			{
				if (!this.RemoveLocked(Object))
					return false;

				this.AddLocked(Object);

				return true;
			}
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(IGeoBoundingBox Box)
		{
			return this.Find(Box, 0, int.MaxValue, null, null);
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <param name="MaxCount">Maximum number of items in the result.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(IGeoBoundingBox Box, int MaxCount)
		{
			return this.Find(Box, 0, MaxCount, null, null);
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <param name="Offset">Offset of first item to return.</param>
		/// <param name="MaxCount">Maximum number of items in the result.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(IGeoBoundingBox Box, int Offset, int MaxCount)
		{
			return this.Find(Box, Offset, MaxCount, null, null);
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <param name="GeoIdPattern">Restricting search to items with IDs matching
		/// this regular expression.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(IGeoBoundingBox Box, Regex GeoIdPattern)
		{
			return this.Find(Box, 0, int.MaxValue, GeoIdPattern, null);
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <param name="MaxCount">Maximum number of items in the result.</param>
		/// <param name="GeoIdPattern">Restricting search to items with IDs matching
		/// this regular expression.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(IGeoBoundingBox Box, int MaxCount, Regex GeoIdPattern)
		{
			return this.Find(Box, 0, MaxCount, GeoIdPattern, null);
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <param name="Offset">Offset of first item to return.</param>
		/// <param name="MaxCount">Maximum number of items in the result.</param>
		/// <param name="GeoIdPattern">Restricting search to items with IDs matching
		/// this regular expression.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(IGeoBoundingBox Box, int Offset, int MaxCount, Regex GeoIdPattern)
		{
			return this.Find(Box, Offset, MaxCount, GeoIdPattern, null);
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <param name="Offset">Offset of first item to return.</param>
		/// <param name="MaxCount">Maximum number of items in the result.</param>
		/// <param name="GeoIdPattern">Restricting search to items with IDs matching
		/// this regular expression.</param>
		/// <param name="CustomFilter">Custom filter applied to object references matching
		/// other filters.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(IGeoBoundingBox Box, int Offset, int MaxCount, Regex GeoIdPattern,
			Predicate<T> CustomFilter)
		{
			if (Offset < 0)
				throw new ArgumentOutOfRangeException(nameof(Offset), "Offset must be a non-negative number.");

			if (MaxCount <= 0)
				return Array.Empty<T>();

			ChunkedList<T> Result = null;
			ChunkedList<GeoNode> Queue;
			GeoPosition Pos;
			GeoNode Loop;
			T Obj;
			int i;

			lock (this.synchObj)
			{
				if (this.root is null)
					return Array.Empty<T>();

				Queue = new ChunkedList<GeoNode>() { this.root };

				while (Queue.HasFirstItem)
				{
					Loop = Queue.RemoveFirst();
					Obj = Loop.Object;
					Pos = Obj.Location;

					i = 15; // 1 = NW, 2 = NE, 4 = SW, 8 = SE

					if (Pos.NorthOf(Box))
						i &= 12;

					if (Pos.SouthOf(Box))
						i &= 3;

					if (Pos.EastOf(Box))
						i &= 5;

					if (Pos.WestOf(Box))
						i &= 10;

					if (i != 0)
					{
						if (i == 15 &&
							Pos.AltitudeCheck(Box) &&
							(GeoIdPattern is null || Obj.GeoId.GeoIdPatternCheck(GeoIdPattern)) &&
							(CustomFilter is null || CustomFilter(Obj)))
						{
							if (Offset >= 0)
							{
								if (Result is null)
									Result = new ChunkedList<T>();

								Result.Add(Obj);

								if (--MaxCount <= 0)
									return Result.ToArray();
							}
							else
								Offset--;
						}

						if ((i & 1) != 0 && !(Loop.NW is null))
							Queue.Add(Loop.NW);

						if ((i & 2) != 0 && !(Loop.NE is null))
							Queue.Add(Loop.NE);

						if ((i & 4) != 0 && !(Loop.SW is null))
							Queue.Add(Loop.SW);

						if ((i & 8) != 0 && !(Loop.SE is null))
							Queue.Add(Loop.SE);
					}
				}
			}

			return Result?.ToArray() ?? Array.Empty<T>();
		}
	}
}
