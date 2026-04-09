using System;
using Waher.Runtime.Collections;

namespace Waher.Runtime.Text
{
	/// <summary>
	/// Computes the difference between two sequences of symbols.
	/// </summary>
	public static class Difference
	{
		/// <summary>
		/// Analyzes two sequences of symbols to estimate the difference between them.
		/// </summary>
		/// <remarks>
		/// Method searches for the shortest path in changing <paramref name="S1"/> to
		/// <paramref name="S2"/>. Costs are 0, if keeping a symbol: Cost of inserting
		/// or deleting a symbol is 1, if first symbol, or same operation as previous
		/// symbol, or 2, if chaning operation. The reason for this is to avoid altering
		/// inserts and deletions when blocks are changed.
		/// </remarks>
		/// <typeparam name="T">Type of symbols to compare.</typeparam>
		/// <param name="S1">First sequence.</param>
		/// <param name="S2">Second sequence.</param>
		/// <returns>Edit script</returns>
		public static EditScript<T> Analyze<T>(T[] S1, T[] S2)
		{
			int c1 = S1.Length;
			int c2 = S2.Length;

			if (c1 == 0 && c2 == 0)
				return new EditScript<T>(S1, S2, Array.Empty<Step<T>>());

			int StartOffset = 0;

			while (StartOffset < c1 && StartOffset < c2 &&
				S1[StartOffset].Equals(S2[StartOffset]))
			{
				StartOffset++;
			}

			int EndOffset1 = c1;
			int EndOffset2 = c2;

			while (EndOffset1 > StartOffset && EndOffset2 > StartOffset &&
				S1[EndOffset1 - 1].Equals(S2[EndOffset2 - 1]))
			{
				EndOffset1--;
				EndOffset2--;
			}

			int c1p = EndOffset1 - StartOffset + 1;
			int c2p = EndOffset2 - StartOffset + 1;
			long NrBits = ((long)c1p) * c2p;
			long NrBytes = (NrBits + 7) >> 3;

			if (NrBytes > int.MaxValue)
				throw new OutOfMemoryException("Unable to allocate enough memory to process the difference between the two sequences.");

			byte[] Processed = new byte[NrBytes];

			ChunkedList<State<T>> Current = new ChunkedList<State<T>>();
			ChunkedList<State<T>> Next = new ChunkedList<State<T>>();
			ChunkedList<State<T>> NextNext = new ChunkedList<State<T>>();
			ChunkedList<State<T>> Temp;
			State<T> P, Q;
			bool b1, b2;
			int i, j, iByte;
			byte iBit;

			P = null;
			for (i = 0; i <= StartOffset; i++)
			{
				P = new State<T>()
				{
					Op = EditOperation.Keep,
					i1 = i,
					i2 = i,
					Prev = P
				};
			}

			while (true)
			{
				if (P.i1 == EndOffset1 && P.i2 == EndOffset2)
					break;

				i = P.i1 - StartOffset + (P.i2 - StartOffset) * c1p;
				iByte = i >> 3;
				iBit = (byte)(1 << (i & 7));

				if ((Processed[iByte] & iBit) == 0)
				{
					Processed[iByte] |= iBit;

					if (b2 = P.i2 < EndOffset2)
					{
						j = i + c1p;
						if ((Processed[j >> 3] & (byte)(1 << (j & 7))) == 0)
						{
							Q = new State<T>()
							{
								Op = EditOperation.Insert,
								i1 = P.i1,
								i2 = P.i2 + 1,
								Prev = P
							};

							if (P.Op == EditOperation.Insert)
								Next.AddLastItem(Q);
							else
								NextNext.AddLastItem(Q);
						}
					}

					if (b1 = P.i1 < EndOffset1)
					{
						j = i + 1;
						if ((Processed[j >> 3] & (byte)(1 << (j & 7))) == 0)
						{
							Q = new State<T>()
							{
								Op = EditOperation.Delete,
								i1 = P.i1 + 1,
								i2 = P.i2,
								Prev = P
							};

							if (P.Op == EditOperation.Delete)
								Next.AddLastItem(Q);
							else
								NextNext.AddLastItem(Q);
						}
					}

					if (b1 && b2 && S1[P.i1].Equals(S2[P.i2]))
					{
						j = i + 1 + c1p;
						if ((Processed[j >> 3] & (byte)(1 << (j & 7))) == 0)
						{
							Q = new State<T>()
							{
								Op = EditOperation.Keep,
								i1 = P.i1 + 1,
								i2 = P.i2 + 1,
								Prev = P
							};

							Current.AddLastItem(Q);

							if (Q.i1 == EndOffset1 && Q.i2 == EndOffset2)
							{
								P = Q;
								break;
							}
						}
					}
				}

				if (!Current.HasFirstItem)
				{
					Temp = Current;

					if (!Next.HasFirstItem)
						Current = NextNext;
					else
					{
						Current = Next;
						Next = NextNext;
					}

					NextNext = Temp;
				}

				P = Current.RemoveLast();
			}

			while (EndOffset1 < c1 && EndOffset2 < c2)
			{
				P = new State<T>()
				{
					Op = EditOperation.Keep,
					i1 = ++EndOffset1,
					i2 = ++EndOffset2,
					Prev = P
				};
			}

			State<T> Loop = P;
			int NrSteps = 0;

			while (!(Loop.Prev is null))
			{
				NrSteps++;
				Loop = Loop.Prev;
			}

			State<T>[] Steps = new State<T>[NrSteps];
			for (i = NrSteps, Loop = P; i > 0; Loop = Loop.Prev)
				Steps[--i] = Loop;

			ChunkedList<T> Elements = new ChunkedList<T>();
			ChunkedList<Step<T>> Operations = new ChunkedList<Step<T>>();
			EditOperation Op;

			Q = Steps[0];
			Op = Q.Op;

			for (i = 0; i < NrSteps; i++)
			{
				P = Steps[i];
				if (P.Op != Op)
				{
					Operations.Add(new Step<T>(Elements.ToArray(), Q.i1, Q.i2, Op));
					Q = P;
					Op = P.Op;
					Elements.Clear();
				}

				if (Op == EditOperation.Insert)
					Elements.Add(S2[P.i2 - 1]);
				else
					Elements.Add(S1[P.i1 - 1]);
			}

			Operations.Add(new Step<T>(Elements.ToArray(), Q.i1, Q.i2, Op));

			return new EditScript<T>(S1, S2, Operations.ToArray());
		}

		private class State<T>
		{
			public EditOperation Op;
			public int i1;
			public int i2;
			public State<T> Prev;

			public override string ToString()
			{
				return this.Op.ToString() + " " + this.i1.ToString() + "," + this.i2.ToString();
			}
		}

		/// <summary>
		/// Analyzes two text strings, estimating the difference between them.
		/// </summary>
		/// <param name="s1">First string.</param>
		/// <param name="s2">Second string.</param>
		/// <returns>Differences found.</returns>
		public static EditScript<char> AnalyzeStrings(string s1, string s2)
		{
			return Analyze(s1.ToCharArray(), s2.ToCharArray());
		}

		/// <summary>
		/// Analyzes two texts, estimating the difference between them, as a sequence of rows.
		/// </summary>
		/// <param name="Text1">First text.</param>
		/// <param name="Text2">Second text.</param>
		/// <returns>Differences found.</returns>
		public static EditScript<string> AnalyzeRows(string Text1, string Text2)
		{
			return Analyze(ExtractRows(Text1), ExtractRows(Text2));
		}

		/// <summary>
		/// Extracts the rows from a text.
		/// </summary>
		/// <param name="Text">Text.</param>
		/// <returns>Sequence of rows.</returns>
		public static string[] ExtractRows(string Text)
		{
			return Text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
		}

	}
}
