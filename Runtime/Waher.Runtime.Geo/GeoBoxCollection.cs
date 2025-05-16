using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Waher.Runtime.Collections;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// In-memory thread-safe geo-spatial collection of bounding boxes.
	/// </summary>
	public class GeoBoxCollection<T> : ICollection<T>
		where T : IGeoBoundingBox
	{
		private const int DefaultGridSize = 8;
		private const int DefaultMaxCellCount = 8;

		private readonly Dictionary<string, BoxNode> boxesById = new Dictionary<string, BoxNode>();
		private readonly object synchObj = new object();
		private readonly int gridSize;
		private readonly int maxCellCount;
		private volatile int token = 0;
		private int count = 0;
		private BoxGrid root = null;

		private class BoxNode
		{
			public T Box;
			public ChunkedList<BoxListReference> References;
		}

		private class BoxListReference
		{
			public BoxGrid Grid;
			public NormalizedBox Box;
			public int X;
			public int Y;
		}

		private class BoxGrid
		{
			public ChunkedList<NormalizedBox> EntireArea;
			public Cell[,] Grid;
			public readonly BoxGrid Parent;
			public readonly NormalizedBox Box;
			public readonly int Width;
			public readonly int Height;
			public readonly int MaxCellCount;
			public readonly int X;
			public readonly int Y;

			public BoxGrid(BoxGrid Parent, NormalizedBox Box, int Width, int Height,
				int MaxCellCount, int X, int Y)
			{
				this.Parent = Parent;
				this.Box = Box;
				this.Width = Width;
				this.Height = Height;
				this.MaxCellCount = MaxCellCount;
				this.Grid = new Cell[this.Width, this.Height];
				this.X = X;
				this.Y = Y;
			}

			public void Fill(NormalizedBox Box)
			{
				if (this.EntireArea is null)
					this.EntireArea = new ChunkedList<NormalizedBox>(1);

				this.EntireArea.Add(Box);

				if (Box.Node.References is null)
					Box.Node.References = new ChunkedList<BoxListReference>(1);

				Box.Node.References.Add(new BoxListReference()
				{
					Grid = this,
					Box = Box,
					X = -1,
					Y = -1
				});
			}

			public void Add(NormalizedBox Box)
			{
				if (Box.Contains(this.Box))
					this.Fill(Box);
				else
				{
					int ScaleX = this.Width;
					int ScaleY = this.Height;
					int Scale = this.Box.Scale;

					if (Scale > 1)
					{
						ScaleX *= Scale;
						ScaleY *= Scale;

						ScaleX <<= 1;
					}

					int x0 = (int)((Box.MinLongitude - this.Box.MinLongitude) * ScaleX);
					int x1 = (int)((Box.MaxLongitude - this.Box.MinLongitude) * ScaleX);
					int y0 = (int)((Box.MinLatitude - this.Box.MinLatitude) * ScaleY);
					int y1 = (int)((Box.MaxLatitude - this.Box.MinLatitude) * ScaleY);

					if (x0 < 0)
						x0 = 0;
					else if (x0 >= this.Width)
						x0 = this.Width - 1;

					if (y0 < 0)
						y0 = 0;
					else if (y0 >= this.Height)
						y0 = this.Height - 1;

					if (x1 < 0)
						x1 = 0;
					else if (x1 >= this.Width)
						x1 = this.Width - 1;

					if (y1 < 0)
						y1 = 0;
					else if (y1 >= this.Height)
						y1 = this.Height - 1;

					if (Box.Node.References is null)
						Box.Node.References = new ChunkedList<BoxListReference>((x1 - x0 + 1) * (y1 - y0 + 1));

					for (int y = y0; y <= y1; y++)
					{
						for (int x = x0; x <= x1; x++)
							this.AddCell(Box, x, y);
					}
				}
			}

			public void AddCell(NormalizedBox Box, int x, int y)
			{
				Cell Cell = this.Grid[x, y];

				if (Cell is null)
				{
					this.Grid[x, y] = new Cell()
					{
						List = new ChunkedList<NormalizedBox>(1) { Box }
					};

					Box.Node.References.Add(new BoxListReference()
					{
						Grid = this,
						Box = Box,
						X = x,
						Y = y
					});
				}
				else if (!(Cell.Grid is null))
					Cell.Grid.Add(Box);
				else if (Cell.List.Count >= this.MaxCellCount && this.Box.CanIncreaseScale(this.Height))
				{
					double DeltaLat = this.Box.DeltaLatitude / this.Height;
					double DeltaLong = this.Box.DeltaLongitude / this.Width;

					Cell.Grid = new BoxGrid(this, new NormalizedBox(default)
					{
						MinLatitude = this.Box.MinLatitude + y * DeltaLat,
						MaxLatitude = this.Box.MinLatitude + (y + 1) * DeltaLat,
						MinLongitude = this.Box.MinLongitude + x * DeltaLong,
						MaxLongitude = this.Box.MinLongitude + (x + 1) * DeltaLong,
						Scale = this.Box.Scale * this.Height,
					}, this.Height, this.Height, this.MaxCellCount, x, y);    // Only first level grids have different widths and heights, deeper levels are square.

					Cell.Grid.Add(Box);
				}
				else
				{
					Cell.List.Add(Box);

					Box.Node.References.Add(new BoxListReference()
					{
						Grid = this,
						Box = Box,
						X = x,
						Y = y
					});
				}
			}

			public bool Remove(BoxGrid Grid)
			{
				if (this.Grid is null)
					return false;

				Cell Cell = this.Grid[Grid.X, Grid.Y];
				if (Cell is null)
					return false;

				if (!(Cell.Grid == Grid))
					return false;

				Cell.Grid = null;

				if (Cell.List is null)
					this.Grid[Grid.X, Grid.Y] = null;

				return true;
			}

			public bool Remove(BoxListReference Reference)
			{
				if (Reference.X < 0 || Reference.Y < 0)
				{
					if (!this.EntireArea.Remove(Reference.Box))
						return false;

					if (this.EntireArea.Count == 0)
						this.EntireArea = null;
				}
				else
				{
					Cell Cell = this.Grid[Reference.X, Reference.Y];
					if (Cell is null)
						return false;

					if (!(Cell.List?.Remove(Reference.Box) ?? false))
						return false;

					if (Cell.List.Count == 0)
					{
						Cell.List = null;

						if (Cell.Grid is null)
						{
							this.Grid[Reference.X, Reference.Y] = null;

							for (int y = 0; y < this.Height; y++)
							{
								for (int x = 0; x < this.Width; x++)
								{
									if (!(this.Grid[x, y] is null))
										return true;
								}
							}

							this.Parent?.Remove(this);
						}
					}
				}

				return true;
			}

			public void Find(GeoPosition Position, ref ChunkedList<T> Result)
			{
				if (!(this.EntireArea is null))
				{
					foreach (NormalizedBox Box in this.EntireArea)
					{
						if (Box.Node.Box.Contains(Position))
						{
							if (Result is null)
								Result = new ChunkedList<T>();

							Result.Add(Box.Node.Box);
						}
					}
				}

				int ScaleX = this.Width;
				int ScaleY = this.Height;
				int Scale = this.Box.Scale;

				if (Scale > 1)
				{
					ScaleX *= Scale;
					ScaleY *= Scale;

					ScaleX <<= 1;
				}

				int x = (int)((Position.NormalizedLongitude - this.Box.MinLongitude) * ScaleX);
				int y = (int)((Position.NormalizedLatitude - this.Box.MinLatitude) * ScaleY);

				if (x >= 0 && x < this.Width && y >= 0 && y < this.Height)
				{
					Cell Cell = this.Grid[x, y];

					if (!(Cell is null))
					{
						if (!(Cell.List is null))
						{
							foreach (NormalizedBox Box in Cell.List)
							{
								if (Box.Node.Box.Contains(Position))
								{
									if (Result is null)
										Result = new ChunkedList<T>();

									Result.Add(Box.Node.Box);
								}
							}
						}

						Cell.Grid?.Find(Position, ref Result);
					}
				}
			}

			public void Export(XmlWriter Output)
			{
				Output.WriteStartElement("Grid");
				Output.WriteAttributeString("width", this.Width.ToString());
				Output.WriteAttributeString("height", this.Height.ToString());

				this.Box.Export(Output, "GridNBox");

				if (!(this.EntireArea is null))
				{
					foreach (NormalizedBox Box in this.EntireArea)
						Box.Export(Output, "FullNBox");
				}

				for (int y = 0; y < this.Height; y++)
				{
					for (int x = 0; x < this.Width; x++)
						this.Grid[x, y]?.Export(Output, x, y);
				}

				Output.WriteEndElement();
			}
		}

		private class Cell
		{
			public ChunkedList<NormalizedBox> List;
			public BoxGrid Grid;

			public void Export(XmlWriter Output, int X, int Y)
			{
				Output.WriteStartElement("Cell");
				Output.WriteAttributeString("x", X.ToString());
				Output.WriteAttributeString("y", Y.ToString());

				if (!(this.List is null))
				{
					foreach (NormalizedBox Box in this.List)
						Box.Export(Output, "NBoxRef");
				}

				this.Grid?.Export(Output);
				Output.WriteEndElement();
			}
		}

		[DebuggerDisplay("{MinLatitude},{MinLongitude} - {MaxLatitude},{MaxLongitude}")]
		private class NormalizedBox
		{
			public NormalizedBox(BoxNode Node)
			{
				this.Node = Node;
			}

			public BoxNode Node;
			public double MinLatitude;
			public double MaxLatitude;
			public double MinLongitude;
			public double MaxLongitude;
			public int Scale;

			public double DeltaLatitude => this.MaxLatitude - this.MinLatitude;
			public double DeltaLongitude => this.MaxLongitude - this.MinLongitude;

			public bool Contains(NormalizedBox Box)
			{
				return
					Box.MinLatitude >= this.MinLatitude &&
					Box.MinLongitude >= this.MinLongitude &&
					Box.MaxLatitude <= this.MaxLatitude &&
					Box.MaxLongitude <= this.MaxLongitude;
			}

			public bool CanIncreaseScale(int Factor)
			{
				long x = this.Scale;
				x *= Factor;
				x <<= 1;

				return ((int)x) == x;
			}

			public void Export(XmlWriter Output, string ElementName)
			{
				Output.WriteStartElement(ElementName);
				Output.WriteAttributeString("minLat", GeoPosition.ToString(this.MinLatitude));
				Output.WriteAttributeString("maxLat", GeoPosition.ToString(this.MaxLatitude));
				Output.WriteAttributeString("minLong", GeoPosition.ToString(this.MinLongitude));
				Output.WriteAttributeString("maxLong", GeoPosition.ToString(this.MaxLongitude));
				Output.WriteAttributeString("scale", this.Scale.ToString());

				if (!(this.Node is null))
				{
					Output.WriteAttributeString("boxId", this.Node.Box.BoxId);

					Output.WriteStartElement("Box");
					Output.WriteAttributeString("minLat", GeoPosition.ToString(this.Node.Box.Min.Latitude));
					Output.WriteAttributeString("maxLat", GeoPosition.ToString(this.Node.Box.Max.Latitude));
					Output.WriteAttributeString("minLong", GeoPosition.ToString(this.Node.Box.Min.Longitude));
					Output.WriteAttributeString("maxLong", GeoPosition.ToString(this.Node.Box.Max.Longitude));
					Output.WriteAttributeString("minIncl", GeoPosition.ToString(this.Node.Box.IncludeMin));
					Output.WriteAttributeString("maxIncl", GeoPosition.ToString(this.Node.Box.IncludeMax));
					Output.WriteEndElement();
				}

				Output.WriteEndElement();
			}
		}

		/// <summary>
		/// In-memory thread-safe geo-spatial collection of bounding boxes.
		/// </summary>
		public GeoBoxCollection()
			: this(DefaultGridSize, DefaultMaxCellCount)
		{
		}

		/// <summary>
		/// In-memory thread-safe geo-spatial collection of bounding boxes.
		/// </summary>
		/// <param name="GridSize">Width and Height of each grid of box collections.
		/// (Topmost grid will have twice the width.)</param>
		/// <param name="MaxCellCount">Maximum number of boxes in a cell, before splitting
		/// it into a new set of grids.</param>
		public GeoBoxCollection(int GridSize, int MaxCellCount)
		{
			this.gridSize = GridSize;
			this.maxCellCount = MaxCellCount;
			this.Clear();
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
		/// Adds a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Bounding box to add.</param>
		public void Add(T Box)
		{
			lock (this.synchObj)
			{
				if (this.boxesById.TryGetValue(Box.BoxId, out BoxNode Node))
				{
					if (Node.Box.Equals(Box))
						return;
					else
						throw new ArgumentException("An bounding box with the same ID already exists in the collection.", nameof(Box));
				}

				Node = new BoxNode()
				{
					Box = Box
				};
				this.boxesById[Box.BoxId] = Node;

				if (Box.LongitudeWrapped)
				{
					double MinLat = Box.Min.NormalizedLatitude;
					double MaxLat = Box.Max.NormalizedLatitude;

					this.root.Add(new NormalizedBox(Node)
					{
						MinLatitude = MinLat,
						MaxLatitude = MaxLat,
						MinLongitude = Box.Min.NormalizedLongitude,
						MaxLongitude = 1,
						Scale = 1
					});

					this.root.Add(new NormalizedBox(Node)
					{
						MinLatitude = MinLat,
						MaxLatitude = MaxLat,
						MinLongitude = 0,
						MaxLongitude = Box.Max.NormalizedLongitude,
						Scale = 1
					});
				}
				else
				{
					this.root.Add(new NormalizedBox(Node)
					{
						MinLatitude = Box.Min.NormalizedLatitude,
						MaxLatitude = Box.Max.NormalizedLatitude,
						MinLongitude = Box.Min.NormalizedLongitude,
						MaxLongitude = Box.Max.NormalizedLongitude,
						Scale = 1
					});
				}

				this.token++;
				this.count++;
			}
		}

		/// <summary>
		/// Clears the collection.
		/// </summary>
		public void Clear()
		{
			lock (this.synchObj)
			{
				this.root = new BoxGrid(null, new NormalizedBox(default)
				{
					MinLatitude = 0,
					MaxLatitude = 1,
					MinLongitude = 0,
					MaxLongitude = 1,
					Scale = 1
				}, 2 * this.gridSize, this.gridSize, this.maxCellCount, 0, 0);

				this.boxesById.Clear();
				this.count = 0;
				this.token++;
			}
		}

		/// <summary>
		/// Checks if the collection contains a geo-spatial bounding box.
		/// </summary>
		/// <param name="Box">Geo-spatial bounding box.</param>
		/// <returns>If the collection contains the bounding box.</returns>
		public bool Contains(T Box)
		{
			lock (this.synchObj)
			{
				if (this.boxesById.TryGetValue(Box.BoxId, out BoxNode Node))
				{
					if (Node.Box.Equals(Box))
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
				if (Offset + this.boxesById.Count > Destination.Length)
					throw new ArgumentException("Offset must allow all elements to be copied into the destination array.", nameof(Offset));

				foreach (BoxNode Node in this.boxesById.Values)
					Destination[Offset++] = Node.Box;
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

				foreach (BoxNode Node in this.boxesById.Values)
					Result[i++] = Node.Box;

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
				return new GeoBoxEnumerator(this, this.boxesById.Values.GetEnumerator());
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

		private class GeoBoxEnumerator : IEnumerator<T>
		{
			private readonly IEnumerator<BoxNode> e;
			private readonly GeoBoxCollection<T> collection;
			private readonly int token;

			public GeoBoxEnumerator(GeoBoxCollection<T> Collection, IEnumerator<BoxNode> Enumerator)
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

						return this.e.Current.Box;
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
		/// Removes a bounding box from the collection.
		/// </summary>
		/// <param name="Box">Bounding box to remove.</param>
		/// <returns>If the bounding box was found and moved.</returns>
		public bool Remove(T Box)
		{
			lock (this.synchObj)
			{
				if (!this.boxesById.TryGetValue(Box.BoxId, out BoxNode Node))
					return false;

				if (!Node.Box.Equals(Box))
					return false;

				this.boxesById.Remove(Box.BoxId);
				this.count--;
				this.token++;

				bool Result = false;

				if (!(Node.References is null))
				{
					foreach (BoxListReference Reference in Node.References)
					{
						if (Reference.Grid.Remove(Reference))
							Result = true;
					}

					Node.References.Clear();
					Node.References = null;
				}

				return Result;
			}
		}

		/// <summary>
		/// Finds all bounding boxes containing a geo-spatial position.
		/// </summary>
		/// <param name="Position">Geo-spatial position.</param>
		/// <returns>Array of bounding boxes found that contains the given position.</returns>
		public T[] Find(GeoPosition Position)
		{
			ChunkedList<T> Result = null;

			lock (this.synchObj)
			{
				this.root.Find(Position, ref Result);
			}

			return Result?.ToArray() ?? Array.Empty<T>();
		}

		/// <summary>
		/// Outputs the contents of the collection to XML.
		/// </summary>
		/// <returns>XML output.</returns>
		public string Export()
		{
			StringBuilder Output = new StringBuilder();
			this.Export(Output);
			return Output.ToString();
		}

		/// <summary>
		/// Outputs the contents of the collection to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public void Export(StringBuilder Output)
		{
			XmlWriterSettings Settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				NewLineChars = Environment.NewLine,
				OmitXmlDeclaration = true
			};

			using (XmlWriter w = XmlWriter.Create(Output, Settings))
			{
				this.Export(w);
				w.Flush();
			}
		}

		/// <summary>
		/// Outputs the contents of the collection to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public void Export(XmlWriter Output)
		{
			lock (this.synchObj)
			{
				Output.WriteStartElement("Boxes");
				Output.WriteAttributeString("gridSize", this.gridSize.ToString());
				Output.WriteAttributeString("maxCellCount", this.maxCellCount.ToString());
				Output.WriteAttributeString("count", this.count.ToString());
				Output.WriteAttributeString("token", this.token.ToString());

				this.root.Export(Output);

				Output.WriteEndElement();
			}
		}
	}
}
