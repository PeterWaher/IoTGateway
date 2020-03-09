using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Text
{
	/// <summary>
	/// Represents a sub-sequence of symbols.
	/// </summary>
	/// <typeparam name="T">Type of symbols being compared.</typeparam>
	public class Step<T>
	{
		private readonly T[] symbols;
		private readonly int index1;
		private readonly int index2;
		private readonly EditOperation operation;

		/// <summary>
		/// Represents a sub-sequence of symbols.
		/// </summary>
		/// <param name="Symbols">Sequence of symbols.</param>
		/// <param name="Index1">Index into the first sequence of symbols.</param>
		/// <param name="Index2">Index into the second sequence of symbols.</param>
		/// <param name="Operation">Edit operation being performed.</param>
		public Step(T[] Symbols, int Index1, int Index2, EditOperation Operation)
		{
			this.symbols = Symbols;
			this.index1 = Index1;
			this.index2 = Index2;
			this.operation = Operation;
		}

		/// <summary>
		/// Sequence of symbols.
		/// </summary>
		public T[] Symbols => this.symbols;

		/// <summary>
		/// Index into the first sequence of symbols.
		/// </summary>
		public int Index1 => this.index1;

		/// <summary>
		/// Index into the second sequence of symbols.
		/// </summary>
		public int Index2 => this.index2;

		/// <summary>
		/// Edit operation being performed.
		/// </summary>
		public EditOperation Operation => this.operation;
	}
}
