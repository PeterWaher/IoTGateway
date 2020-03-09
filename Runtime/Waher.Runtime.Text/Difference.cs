using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
			int c1p = c1 + 1;
			int c2p = c2 + 1;
			BitArray Processed = new BitArray(c1p * c2p);
			LinkedList<State<T>> Current = new LinkedList<State<T>>();
			LinkedList<State<T>> Next = new LinkedList<State<T>>();
			LinkedList<State<T>> NextNext = new LinkedList<State<T>>();
			LinkedList<State<T>> Temp;
			State<T> P, Q;
			bool b1, b2;
			int i;

			P = new State<T>()
			{
				Op = EditOperation.Keep,
				i1 = 0,
				i2 = 0,
				Prev = null
			};

			while (true)
			{
				if (P.i1 == c1 && P.i2 == c2)
					break;

				i = P.i1 + P.i2 * c1p;
				if (!Processed[i])
				{
					Processed[i] = true;

					if (b2 = (P.i2 < c2))
					{
						if (!Processed[i + c1p])
						{
							Q = new State<T>()
							{
								Op = EditOperation.Insert,
								i1 = P.i1,
								i2 = P.i2 + 1,
								Prev = P
							};

							if (P.Op == EditOperation.Insert)
								Next.AddLast(Q);
							else
								NextNext.AddLast(Q);
						}
					}

					if (b1 = (P.i1 < c1))
					{
						if (!Processed[i + 1])
						{
							Q = new State<T>()
							{
								Op = EditOperation.Delete,
								i1 = P.i1 + 1,
								i2 = P.i2,
								Prev = P
							};

							if (P.Op == EditOperation.Delete)
								Next.AddLast(Q);
							else
								NextNext.AddLast(Q);
						}
					}

					if (b1 && b2 && S1[P.i1].Equals(S2[P.i2]))
					{
						if (!Processed[i + 1 + c1p])
						{
							Q = new State<T>()
							{
								Op = EditOperation.Keep,
								i1 = P.i1 + 1,
								i2 = P.i2 + 1,
								Prev = P
							};

							Current.AddLast(Q);

							if (Q.i1 == c1 && Q.i2 == c2)
							{
								P = Q;
								break;
							}
						}
					}
				}

				if (Current.First is null)
				{
					Temp = Current;

					if (Next.First is null)
						Current = NextNext;
					else
					{
						Current = Next;
						Next = NextNext;
					}

					NextNext = Temp;
				}

				P = Current.Last.Value;
				Current.RemoveLast();
			}

			LinkedList<T> SameOp = new LinkedList<T>();
			LinkedList<Step<T>> Steps = new LinkedList<Step<T>>();
			EditOperation Op = P.Op;
			State<T> First = P;
			T[] Symbols;

			c1 = 0;
			c2 = 0;

			while (!((Q = P.Prev) is null))
			{
				if (P.Op != Op)
				{
					Symbols = new T[c1];
					SameOp.CopyTo(Symbols, 0);
					Steps.AddFirst(new Step<T>(Symbols, First.i1, First.i2, Op));

					SameOp.Clear();
					c1 = 0;
					c2++;
					Op = P.Op;
				}

				switch (Op)
				{
					case EditOperation.Keep:
					case EditOperation.Delete:
						SameOp.AddFirst(S1[P.i1 - 1]);
						break;

					case EditOperation.Insert:
						SameOp.AddFirst(S2[P.i2 - 1]);
						break;
				}

				c1++;
				First = P;
				P = Q;
			}

			Symbols = new T[c1];
			SameOp.CopyTo(Symbols, 0);
			Steps.AddFirst(new Step<T>(Symbols, First.i1, First.i2, Op));
			c2++;

			Step<T>[] Result = new Step<T>[c2];
			Steps.CopyTo(Result, 0);

			return new EditScript<T>(S1, S2, Result);
		}

		private class State<T>
		{
			public EditOperation Op;
			public int i1;
			public int i2;
			public State<T> Prev;
		}

		/// <summary>
		/// Analyzes two text strings, estimating the difference between them.
		/// </summary>
		/// <param name="s1">First string.</param>
		/// <param name="s2">Second string.</param>
		/// <returns></returns>
		public static EditScript<char> AnalyzeStrings(string s1, string s2)
		{
			return Analyze<char>(s1.ToCharArray(), s2.ToCharArray());
		}

		/// <summary>
		/// Analyzes two texts, estimating the difference between them, as a sequence of rows.
		/// </summary>
		/// <param name="Text1">First text.</param>
		/// <param name="Text2">Second text.</param>
		/// <returns></returns>
		public static EditScript<string> AnalyzeRows(string Text1, string Text2)
		{
			return Analyze<string>(ExtractRows(Text1), ExtractRows(Text2));
		}

		/// <summary>
		/// Extracts the rows from a text.
		/// </summary>
		/// <param name="Text">Text.</param>
		/// <returns>Sequence of rows.</returns>
		public static string[] ExtractRows(string Text)
		{
			return Text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

	}
}
