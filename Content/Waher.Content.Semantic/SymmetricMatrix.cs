using System;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Implements a dynamic symmetric matrix.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public class SymmetricMatrix<T>
	{
		private T[] elements;
		private int size;
		private int len;

		/// <summary>
		/// Implements a dynamic symmetric matrix.
		/// </summary>
		public SymmetricMatrix()
			: this(1)
		{
		}

		/// <summary>
		/// Implements a dynamic symmetric matrix.
		/// </summary>
		/// <param name="Size">Original size.</param>
		public SymmetricMatrix(int Size)
		{
			if (Size <= 0)
				throw new ArgumentOutOfRangeException(nameof(Size), "Must be positive.");

			this.size = Size;
			this.len = (this.size + 1) * this.size / 2;
			this.elements = new T[this.len];
		}

		/// <summary>
		/// Size of Matrix. The size represents both width and height, as 
		/// a symmetric matrix have the same width and height.
		/// </summary>
		public int Size => this.size;

		/// <summary>
		/// Access to elements in the matrix.
		/// </summary>
		/// <param name="X">Zero-based X-coordinate.</param>
		/// <param name="Y">Zero-based Y-coordinate.</param>
		/// <returns>Value of element at coordinate.</returns>
		/// <exception cref="ArgumentOutOfRangeException">If any of the coordinates are negative.</exception>
		public T this[int X, int Y]
		{
			get
			{
				int i = this.GetIndex(X, Y);
				if (i >= this.len)
					return default;
				else
					return this.elements[i];
			}

			set
			{
				int i = this.GetIndex(X, Y);
				if (i >= this.len)
				{
					this.size = Math.Max(X, Y) + 1;
					this.len = i + 1;
					Array.Resize(ref this.elements, this.len);
				}

				this.elements[i] = value;
			}
		}

		private int GetIndex(int X, int Y)
		{
			if (X < 0)
				throw new ArgumentOutOfRangeException(nameof(X), "Negative cooridates not allowed.");

			if (Y < 0)
				throw new ArgumentOutOfRangeException(nameof(Y), "Negative cooridates not allowed.");

			if (X > Y)
				return X * (X - 1) / 2 + Y;
			else if (Y == 0)
				return 0;
			else
				return Y * (Y - 1) / 2 + X;
		}
	}
}
