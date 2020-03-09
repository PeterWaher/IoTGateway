using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Text
{
	/// <summary>
	/// Represents an Edit-script, converting one sequence of symbols to another.
	/// </summary>
	/// <typeparam name="T">Type of symbols used.</typeparam>
	public class EditScript<T>
	{
		private readonly Step<T>[] steps;
		private readonly T[] s1;
		private readonly T[] s2;

		/// <summary>
		/// Represents an Edit-script, converting one sequence of symbols to another.
		/// </summary>
		/// <param name="S1">First sequence of symbols.</param>
		/// <param name="S2">Second sequence of symbols.</param>
		/// <param name="Steps">Steps making up how to transform <paramref name="S1"/> to <paramref name="S2"/>.</param>
		public EditScript(T[] S1, T[] S2, Step<T>[] Steps)
		{
			this.s1 = S1;
			this.s2 = S2;
			this.steps = Steps;
		}

		/// <summary>
		/// Steps making up how to transform <see cref="S1"/> to <see cref="S2"/>.
		/// </summary>
		public Step<T>[] Steps => this.steps;

		/// <summary>
		/// First sequence of symbols.
		/// </summary>
		public T[] S1 => this.s1;

		/// <summary>
		/// Second sequence of symbols.
		/// </summary>
		public T[] S2 => this.s2;
	}
}
