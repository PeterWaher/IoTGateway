using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Functions.Vectors
{
	/// <summary>
	/// Sort(v[,x1[,x2[,x3[,x4[,x5[,x6[,x7[,x8[,x9]]]]]]]]])
	/// </summary>
	public class Sort : FunctionMultiVariate
	{
		/// <summary>
		/// Sort(v)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector },
				  new ArgumentType[] { ArgumentType.Vector },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2,x3)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Order3">Third order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, ScriptNode Order3, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2, Order3 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2,x3,x4)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Order3">Third order.</param>
		/// <param name="Order4">Fourth order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, ScriptNode Order3, ScriptNode Order4, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2, Order3, Order4 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2,x3,x4,x5)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Order3">Third order.</param>
		/// <param name="Order4">Fourth order.</param>
		/// <param name="Order5">Fifth order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, ScriptNode Order3, ScriptNode Order4, ScriptNode Order5,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2, Order3, Order4, Order5 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2,x3,x4,x5,x6)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Order3">Third order.</param>
		/// <param name="Order4">Fourth order.</param>
		/// <param name="Order5">Fifth order.</param>
		/// <param name="Order6">Sixth order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, ScriptNode Order3, ScriptNode Order4, ScriptNode Order5,
			ScriptNode Order6, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2, Order3, Order4, Order5, Order6 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2,x3,x4,x5,x6,x7)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Order3">Third order.</param>
		/// <param name="Order4">Fourth order.</param>
		/// <param name="Order5">Fifth order.</param>
		/// <param name="Order6">Sixth order.</param>
		/// <param name="Order7">Seventh order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, ScriptNode Order3, ScriptNode Order4, ScriptNode Order5,
			ScriptNode Order6, ScriptNode Order7, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2, Order3, Order4, Order5, Order6, Order7 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2,x3,x4,x5,x6,x7,x8)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Order3">Third order.</param>
		/// <param name="Order4">Fourth order.</param>
		/// <param name="Order5">Fifth order.</param>
		/// <param name="Order6">Sixth order.</param>
		/// <param name="Order7">Seventh order.</param>
		/// <param name="Order8">Eights order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, ScriptNode Order3, ScriptNode Order4, ScriptNode Order5,
			ScriptNode Order6, ScriptNode Order7, ScriptNode Order8, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2, Order3, Order4, Order5, Order6, Order7, Order8 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Sort(v,x1,x2,x3,x4,x5,x6,x7,x8,x9)
		/// </summary>
		/// <param name="Vector">Vector to sort.</param>
		/// <param name="Order1">First order.</param>
		/// <param name="Order2">Second order.</param>
		/// <param name="Order3">Third order.</param>
		/// <param name="Order4">Fourth order.</param>
		/// <param name="Order5">Fifth order.</param>
		/// <param name="Order6">Sixth order.</param>
		/// <param name="Order7">Seventh order.</param>
		/// <param name="Order8">Eights order.</param>
		/// <param name="Order9">Ninth order.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Sort(ScriptNode Vector, ScriptNode Order1, ScriptNode Order2, ScriptNode Order3, ScriptNode Order4, ScriptNode Order5,
			ScriptNode Order6, ScriptNode Order7, ScriptNode Order8, ScriptNode Order9, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Order1, Order2, Order3, Order4, Order5, Order6, Order7, Order8, Order9 },
				  new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "sort"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "x", "o1", "o2", "o3", "o4", "o5", "o6", "o7", "o8", "o9" }; }
		}

		/// <summary>
		/// Evaluates the function on a vector argument.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int c = Arguments.Length;

			if (!(Arguments[0] is IVector Vector))
				throw new ScriptRuntimeException("First argument muts be a vector.", this);

			ICollection<IElement> Temp = Vector.VectorElements;
			IElement[] Elements;
			int i;

			if (Temp is IElement[] V)
				Elements = (IElement[])V.Clone();
			else
			{
				Elements = new IElement[Temp.Count];
				i = 0;

				foreach (IElement E in Temp)
					Elements[i++] = E;
			}

			if (c == 1)
				Array.Sort<IElement>(Elements, new ElementOrder(this));
			else
			{
				IComparer<IElement>[] Comparers = new IComparer<IElement>[c - 1];
				IElement Element;

				for (i = 1; i < c; i++)
				{
					Element = Arguments[i];

					if (Element is DoubleNumber N)
					{
						double d = N.Value;
						int Sign = 1;

						if (d < 0)
						{
							Sign = -1;
							d = -d;
						}

						if (d == 0 || d > int.MaxValue || d != Math.Truncate(d))
							throw new ScriptRuntimeException("Index values must be non-zero integers.", this);

						Comparers[i - 1] = new IndexOrder(this, (int)(d - 1), Sign);
					}
					else if (Element is StringValue S)
					{
						string s = S.Value;
						int Sign = 1;

						if (s.StartsWith("-"))
						{
							s = s.Substring(1);
							Sign = -1;
						}

						Comparers[i - 1] = new PropertyOrder(this, s, Sign);
					}
					else if (Element is ILambdaExpression Lambda)
					{
						if (Lambda.NrArguments != 2)
							throw new ScriptRuntimeException("Lambda expressions must take exactly two parameters.", this);

						Comparers[i - 1] = new LambdaOrder(Lambda, Variables);
					}
					else
					{
						throw new ScriptRuntimeException("Order parameters must be either lambda expressions, " +
							"string values representing field names or numeric index values.", this);
					}
				}

				Array.Sort(Elements, new CompoundOrder(Comparers));
			}

			return Vector.Encapsulate(Elements, this);
		}

		private class CompoundOrder : IComparer<IElement>
		{
			private readonly IComparer<IElement>[] comparers;
			private readonly int c;

			public CompoundOrder(IComparer<IElement>[] Comparers)
			{
				this.comparers = Comparers;
				this.c = Comparers.Length;
			}

			public int Compare(IElement x, IElement y)
			{
				int i, j;

				for (i = 0; i < c; i++)
				{
					j = this.comparers[i].Compare(x, y);
					if (j != 0)
						return j;
				}

				return 0;
			}
		}

		private class ElementOrder : IComparer<IElement>
		{
			private readonly ScriptNode node;

			public ElementOrder(ScriptNode Node)
			{
				this.node = Node;
			}

			public int Compare(IElement x, IElement y)
			{
				return Sort.Compare(x, y, this.node);
			}
		}

		/// <summary>
		/// Compares two elements using the order operator defined by their associated sets.
		/// </summary>
		/// <param name="x">First operand.</param>
		/// <param name="y">Second operand.</param>
		/// <param name="Node">Node performing the comparison.</param>
		/// <returns>Comparison result.</returns>
		public static int Compare(IElement x, IElement y, ScriptNode Node)
		{
			if (x.AssociatedSet is IOrderedSet OrderedSet1)
				return OrderedSet1.Compare(x, y);
			else if (y.AssociatedSet is IOrderedSet OrderedSet2)
				return OrderedSet2.Compare(x, y);
			else
				throw new ScriptRuntimeException("Elements not ordered", Node);
		}

		private class IndexOrder : IComparer<IElement>
		{
			private readonly ScriptNode node;
			private readonly int index;
			private readonly int sign;

			public IndexOrder(ScriptNode Node, int Index, int Sign)
			{
				this.node = Node;
				this.index = Index;
				this.sign = Sign;
			}

			public int Compare(IElement x, IElement y)
			{
				if (x is IVector v1 && y is IVector v2)
				{
					IElement e1 = v1.GetElement(this.index);
					IElement e2 = v2.GetElement(this.index);

					return this.sign * Sort.Compare(e1, e2, this.node);
				}
				else
					throw new ScriptRuntimeException("Elements in array must be vectors.", this.node);
			}
		}

		private class PropertyOrder : IComparer<IElement>
		{
			private readonly ScriptNode node;
			private readonly string name;
			private readonly int sign;
			private Type lastType = null;
			private FieldInfo lastFieldInfo = null;
			private PropertyInfo lastPropertyInfo = null;

			public PropertyOrder(ScriptNode Node, string Name, int Sign)
			{
				this.node = Node;
				this.name = Name;
				this.sign = Sign;
			}

			public int Compare(IElement x, IElement y)
			{
				IElement v1 = this.GetValue(x);
				IElement v2 = this.GetValue(y);

				return this.sign * Sort.Compare(v1, v2, this.node);
			}

			private IElement GetValue(IElement Obj)
			{
				object Value = Obj.AssociatedObjectValue;
				Type T = Value.GetType();
				if (T != this.lastType)
				{
					this.lastType = T;

					PropertyInfo PI = T.GetRuntimeProperty(this.name);

					if (!(PI is null))
					{
						this.lastPropertyInfo = PI;
						this.lastFieldInfo = null;
					}
					else
					{
						FieldInfo FI = T.GetRuntimeField(this.name);

						if (!(PI is null))
						{
							this.lastPropertyInfo = null;
							this.lastFieldInfo = FI;
						}
						else
						{
							this.lastPropertyInfo = null;
							this.lastFieldInfo = null;
						}
					}
				}

				if (!(this.lastPropertyInfo is null))
					Value = this.lastPropertyInfo.GetValue(Value);
				else if (!(this.lastFieldInfo is null))
					Value = this.lastFieldInfo.GetValue(Value);
				else
					Value = null;

				return Expression.Encapsulate(Value);
			}
		}

		private class LambdaOrder : IComparer<IElement>
		{
			private readonly ILambdaExpression lambda;
			private readonly IElement[] arguments;
			private readonly Variables variables;

			public LambdaOrder(ILambdaExpression Lambda, Variables Variables)
			{
				this.lambda = Lambda;
				this.arguments = new IElement[2];
				this.variables = Variables;
			}

			public int Compare(IElement x, IElement y)
			{
				this.arguments[0] = x;
				this.arguments[1] = y;

				IElement Result = this.lambda.Evaluate(this.arguments, this.variables);

				return Math.Sign(Expression.ToDouble(Result));
			}
		}

	}
}