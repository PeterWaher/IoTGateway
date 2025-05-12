using System;
using System.Collections;
using System.Collections.Generic;
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
		/// Checks if the collection contains a geo-spatial object.
		/// </summary>
		/// <param name="Object">Geo-spatial object.</param>
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
				if (!this.nodesById.TryGetValue(Object.GeoId, out GeoNode Node))
					return false;

				if (!Node.Object.Equals(Object))
					return false;

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
		}

		/// <summary>
		/// Finds items that reside inside a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <returns>Array of items found that reside inside the bounding box.</returns>
		public T[] Find(GeoBoundingBox Box)
		{
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
						if (i == 15 && Pos.AltitudeCheck(Box))
						{
							if (Result is null)
								Result = new ChunkedList<T>();

							Result.Add(Obj);
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
