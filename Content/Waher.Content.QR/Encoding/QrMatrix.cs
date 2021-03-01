using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.QR.Encoding
{
	/// <summary>
	/// Class used to compute a QR code matrix.
	/// </summary>
	public class QrMatrix
	{
		private const bool X = true;
		private const bool _ = false;
		private static readonly bool[,] finderMarker = new bool[,]
		{
			{  X, X, X, X, X, X, X },
			{  X, _, _, _, _, _, X },
			{  X, _, X, X, X, _, X },
			{  X, _, X, X, X, _, X },
			{  X, _, X, X, X, _, X },
			{  X, _, _, _, _, _, X },
			{  X, X, X, X, X, X, X }
		};
		private static readonly bool[,] alignmentMarker = new bool[,]
		{
			{  X, X, X, X, X },
			{  X, _, _, _, X },
			{  X, _, X, _, X },
			{  X, _, _, _, X },
			{  X, X, X, X, X }
		};
		private static readonly char[] halfBlocks = new char[] { ' ', '▀', '▄', '█' };
		private static readonly char[] quarterBlocks = new char[] { ' ', '▘', '▝', '▀', '▖', '▌', '▞', '▛', '▗', '▚', '▐', '▜', '▄', '▙', '▟', '█' };

		private readonly bool[,] defined;
		private readonly bool[,] dots;
		private readonly int size;

		/// <summary>
		/// Class used to compute a QR code matrix.
		/// </summary>
		/// <param name="Size">Size of matrix.</param>
		public QrMatrix(int Size)
		{
			if (Size < 21)
				throw new ArgumentException("Invalid size.", nameof(Size));

			this.size = Size;
			this.defined = new bool[Size, Size];
			this.dots = new bool[Size, Size];
		}

		/// <summary>
		/// Encoded dots.
		/// </summary>
		public bool[,] Dots => this.dots;

		/// <summary>
		/// What parts of the mask has been defined.
		/// </summary>
		public bool[,] Defined => this.defined;

		/// <summary>
		/// Draws a Finder marker pattern in the matrix.
		/// </summary>
		/// <param name="X">Left coordinate of the marker.</param>
		/// <param name="Y">Right coordinate of the marker.</param>
		public void DrawFinderMarker(int X, int Y)
		{
			int x, y;

			for (y = 0; y < 7; y++)
			{
				for (x = 0; x < 7; x++)
				{
					this.defined[y + Y, x + X] = true;
					this.dots[y + Y, x + X] = finderMarker[y, x];
				}
			}
		}

		/// <summary>
		/// Draws a Alignment marker pattern in the matrix.
		/// </summary>
		/// <param name="X">Left coordinate of the marker.</param>
		/// <param name="Y">Right coordinate of the marker.</param>
		public void DrawAlignmentMarker(int X, int Y)
		{
			int x, y;

			for (y = 0; y < 5; y++)
			{
				for (x = 0; x < 5; x++)
				{
					this.defined[y + Y, x + X] = true;
					this.dots[y + Y, x + X] = alignmentMarker[y, x];
				}
			}
		}

		/// <summary>
		/// Draws a horizontal line in the matrix.
		/// </summary>
		/// <param name="X1">Left coordinate.</param>
		/// <param name="X2">Right coordinate.</param>
		/// <param name="Y">Y coordinate.</param>
		/// <param name="Dot">If pixels should be lit (true) or cleared (false).</param>
		/// <param name="Toggle">If pixels should be set and cleared consecutively.</param>
		public void HLine(int X1, int X2, int Y, bool Dot, bool Dotted)
		{
			while (X1 <= X2)
			{
				this.defined[Y, X1] = true;
				this.dots[Y, X1] = Dot;
				X1++;
				Dot ^= Dotted;
			}
		}

		/// <summary>
		/// Draws a horizontal line in the matrix.
		/// </summary>
		/// <param name="X">X coordinate.</param>
		/// <param name="Y1">Top coordinate.</param>
		/// <param name="Y2">Bottom coordinate.</param>
		/// <param name="Dot">If pixels should be lit (true) or cleared (false).</param>
		/// <param name="Toggle">If pixels should be set and cleared consecutively.</param>
		public void VLine(int X, int Y1, int Y2, bool Dot, bool Dotted)
		{
			while (Y1 <= Y2)
			{
				this.defined[Y1, X] = true;
				this.dots[Y1, X] = Dot;
				Y1++;
				Dot ^= Dotted;
			}
		}

		/// <summary>
		/// Encodes data bits in free positions in the matrix.
		/// </summary>
		/// <param name="Data"></param>
		/// <returns>If all bytes fit into the matrix.</returns>
		public bool EncodeDataBits(byte[] Data)
		{
			int i = 0;
			int c = Data.Length;
			int State = 0;
			int x = this.size - 1;
			int y = this.size - 1;
			int j;
			byte b;

			while (i < c)
			{
				b = Data[i++];

				for (j = 0; j < 8; j++)
				{
					if (x < 0)
						return false;

					if (this.defined[y, x])
						j--;
					else
					{
						this.defined[y, x] = true;
						this.dots[y, x] = (b & 0x80) != 0;
						b <<= 1;
					}

					switch (State)
					{
						case 0:
							x--;
							State++;
							break;

						case 1:
							if (y == 0)
							{
								x--;
								if (x == 6)
									x--;
								State++;
							}
							else
							{
								y--;
								x++;
								State--;
							}
							break;

						case 2:
							x--;
							State++;
							break;

						case 3:
							if (y == this.size - 1)
							{
								x--;
								State = 0;
							}
							else
							{
								y++;
								x++;
								State--;
							}
							break;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Generates a text string representing the QR code using
		/// full block characters and spaces.
		/// Must be displayed in a font with fixed character sizes,
		/// preferrably where each character is twice as high as wide.
		/// </summary>
		/// <returns>QR Code.</returns>
		public string ToFullBlockText()
		{
			StringBuilder sb = new StringBuilder();

			int x, y;

			for (y = 0; y < this.size; y++)
			{
				for (x = 0; x < this.size; x++)
				{
					if (!this.defined[y, x])
						sb.Append("░░");
					else if (this.dots[y, x])
						sb.Append("██");
					else
						sb.Append("  ");
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Generates a text string representing the QR code using
		/// half block characters and spaces.
		/// Must be displayed in a font with fixed character sizes,
		/// preferrably where each character is twice as high as wide.
		/// </summary>
		/// <returns>QR Code.</returns>
		public string ToHalfBlockText()
		{
			StringBuilder sb = new StringBuilder();

			int x, y, y2;
			int i;

			for (y = 0; y < this.size; y += 2)
			{
				y2 = y + 1;

				for (x = 0; x < this.size; x++)
				{
					i = this.defined[y, x] && this.dots[y, x] ? 1 : 0;

					if (y2 < this.size && this.defined[y2, x] && this.dots[y2, x])
						i |= 2;

					sb.Append(halfBlocks[i]);
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Generates a text string representing the QR code using
		/// quarter block characters and spaces.
		/// Must be displayed in a font with fixed character sizes,
		/// preferrably where each character are square.
		/// </summary>
		/// <returns>QR Code.</returns>
		public string ToQuarterBlockText()
		{
			StringBuilder sb = new StringBuilder();

			int x, y, x2, y2;
			int i;

			for (y = 0; y < this.size; y += 2)
			{
				y2 = y + 1;

				for (x = 0; x < this.size; x++)
				{
					x2 = x + 1;

					i = this.defined[y, x] && this.dots[y, x] ? 1 : 0;

					if (x2 < this.size && this.defined[y, x2] && this.dots[y, x2])
						i |= 2;

					if (y2 < this.size && this.defined[y2, x] && this.dots[y2, x])
						i |= 4;

					if (x2 < this.size && y2 < this.size && this.defined[y2, x2] && this.dots[y2, x2])
						i |= 8;

					sb.Append(quarterBlocks[i]);
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
