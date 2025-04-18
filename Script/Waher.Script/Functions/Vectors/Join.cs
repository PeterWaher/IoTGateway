﻿using System.Collections.Generic;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Joins vectors.
	/// </summary>
	public class Join : FunctionMultiVariate
	{
		/// <summary>
		/// Joins vectors in a vector of vectors.
		/// </summary>
		/// <param name="v">Vector of vectors</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v }, new ArgumentType[] { ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, ScriptNode v3, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2, v3 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="v4">Vector 4</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, ScriptNode v3, ScriptNode v4, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2, v3, v4 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="v4">Vector 4</param>
		/// <param name="v5">Vector 5</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, ScriptNode v3, ScriptNode v4, ScriptNode v5,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2, v3, v4, v5 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector, ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="v4">Vector 4</param>
		/// <param name="v5">Vector 5</param>
		/// <param name="v6">Vector 6</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, ScriptNode v3, ScriptNode v4, ScriptNode v5, ScriptNode v6,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2, v3, v4, v5, v6 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="v4">Vector 4</param>
		/// <param name="v5">Vector 5</param>
		/// <param name="v6">Vector 6</param>
		/// <param name="v7">Vector 7</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, ScriptNode v3, ScriptNode v4, ScriptNode v5, ScriptNode v6,
			ScriptNode v7, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2, v3, v4, v5, v6, v7 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="v4">Vector 4</param>
		/// <param name="v5">Vector 5</param>
		/// <param name="v6">Vector 6</param>
		/// <param name="v7">Vector 7</param>
		/// <param name="v8">Vector 8</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, ScriptNode v3, ScriptNode v4, ScriptNode v5, ScriptNode v6,
			ScriptNode v7, ScriptNode v8, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2, v3, v4, v5, v6, v7, v8 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Joins vectors.
		/// </summary>
		/// <param name="v1">Vector 1</param>
		/// <param name="v2">Vector 2</param>
		/// <param name="v3">Vector 3</param>
		/// <param name="v4">Vector 4</param>
		/// <param name="v5">Vector 5</param>
		/// <param name="v6">Vector 6</param>
		/// <param name="v7">Vector 7</param>
		/// <param name="v8">Vector 8</param>
		/// <param name="v9">Vector 9</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Join(ScriptNode v1, ScriptNode v2, ScriptNode v3, ScriptNode v4, ScriptNode v5, ScriptNode v6,
			ScriptNode v7, ScriptNode v8, ScriptNode v9, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { v1, v2, v3, v4, v5, v6, v7, v8, v9 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector, ArgumentType.Vector,
					  ArgumentType.Vector, ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "v1", "v2" };

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Join);

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.Evaluate(Arguments, true);
		}


		private IElement Evaluate(IElement[] Arguments, bool CheckVectorOfVectors)
		{
			int i, c = Arguments.Length;

			if (c == 1 && CheckVectorOfVectors)
			{
				IElement E = Arguments[0];

				if (E is IVector V)
				{
					ICollection<IElement> Elements = V.VectorElements;
					c = Elements.Count;

					if (!(Elements is IElement[] ElementArray))
					{
						ElementArray = new IElement[c];
						Elements.CopyTo(ElementArray, 0);
					}

					return this.Evaluate(ElementArray, false);
				}
				else
					return E;
			}
			else
			{
				IElement ResultRef = Arguments[0];
				IElement Ref;
				ISet ResultSet = ResultRef.AssociatedSet;
				ISet Set;

				for (i = 1; i < c; i++)
				{
					Ref = Arguments[i];
					Set = Arguments[i].AssociatedSet;

					if (!ResultSet.Equals(Set))
					{
						if (Expression.UpgradeField(ref ResultRef, ref ResultSet, ref Ref, ref Set))
						{
							Arguments[0] = ResultRef;
							Arguments[i] = Ref;
						}
						else
						{
							ResultRef = new ObjectVector(ResultRef.ChildElements);
							ResultSet = ResultRef.AssociatedSet;
							Arguments[0] = ResultRef;

							Ref = new ObjectVector(Ref.ChildElements);
							Arguments[i] = Ref;
						}
					}
				}

				ChunkedList<IElement> Elements = new ChunkedList<IElement>();
				Elements.AddRange(ResultRef.ChildElements);

				for (i = 1; i < c; i++)
				{
					Ref = Arguments[i];
					Set = Arguments[i].AssociatedSet;

					if (!ResultSet.Equals(Set))
					{
						if (!Expression.UpgradeField(ref ResultRef, ref ResultSet, ref Ref, ref Set))
							Ref = new ObjectVector(Ref.ChildElements);
					}

					Elements.AddRange(Ref.ChildElements);
				}

				return ResultRef.Encapsulate(Elements, this);
			}
		}
	}
}
