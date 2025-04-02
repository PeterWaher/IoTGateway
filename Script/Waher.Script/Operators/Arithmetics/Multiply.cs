using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Arithmetics
{
	/// <summary>
	/// Multiplication operator.
	/// </summary>
	public class Multiply : BinaryOperator, IDifferentiable
	{
		/// <summary>
		/// Multiplication operator.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Multiply(ScriptNode Left, ScriptNode Right, int Start, int Length, Expression Expression)
			: base(Left, Right, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Left = this.left.Evaluate(Variables);
			IElement Right = this.right.Evaluate(Variables);

			return EvaluateMultiplication(Left, Right, this);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement Left = await this.left.EvaluateAsync(Variables);
			IElement Right = await this.right.EvaluateAsync(Variables);

			return EvaluateMultiplication(Left, Right, this);
		}

		/// <summary>
		/// Multiplies two operands.
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateMultiplication(IElement Left, IElement Right, ScriptNode Node)
		{
			IElement Result;

			if (Left is IRingElement LE && Right is IRingElement RE)
			{
				Result = LE.MultiplyRight(RE);
				if (!(Result is null))
					return Result;

				Result = RE.MultiplyLeft(LE);
				if (!(Result is null))
					return Result;
			}

			if (Left.IsScalar)
			{
				if (Right.IsScalar)
				{
					ISet LeftSet = Left.AssociatedSet;
					ISet RightSet = Right.AssociatedSet;

					if (!LeftSet.Equals(RightSet))
					{
						if (Expression.UpgradeField(ref Left, ref LeftSet, ref Right, ref RightSet))
						{
							LE = Left as IRingElement;
							RE = Right as IRingElement;
							if (!(LE is null) && !(RE is null))
							{
								Result = LE.MultiplyRight(RE);
								if (!(Result is null))
									return Result;

								Result = RE.MultiplyLeft(LE);
								if (!(Result is null))
									return Result;
							}
						}
					}

					Result = EvaluateNamedOperator("op_Multiply", Left, Right, Node);
					if (!(Result is null))
						return Result;

					object LeftObj = Left.AssociatedObjectValue;

					if (LeftObj is double D)
					{
						if (Right is GroupElement E)
							return EvaluateScalarMultiplication(D, E, Node);
						else
							return EvaluateScalarMultiplication(D, Right, Node);
					}
					else if (LeftObj is BigInteger I)
					{
						if (Right is GroupElement E)
							return EvaluateScalarMultiplication(I, E, Node);
						else
							return EvaluateScalarMultiplication(I, Right, Node);
					}

					object RightObj = Right.AssociatedObjectValue;

					if (RightObj is double D2)
					{
						if (Left is GroupElement E2)
							return EvaluateScalarMultiplication(D2, E2, Node);
						else
							return EvaluateScalarMultiplication(D2, Left, Node);
					}
					else if (RightObj is BigInteger I2)
					{
						if (Left is GroupElement E2)
							return EvaluateScalarMultiplication(I2, E2, Node);
						else
							return EvaluateScalarMultiplication(I2, Left, Node);
					}

					throw new ScriptRuntimeException("Operands cannot be multiplied.", Node);
				}
				else
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement RightChild in Right.ChildElements)
						Elements.AddLast(EvaluateMultiplication(Left, RightChild, Node));

					return Right.Encapsulate(Elements, Node);
				}
			}
			else
			{
				if (Right.IsScalar)
				{
					LinkedList<IElement> Elements = new LinkedList<IElement>();

					foreach (IElement LeftChild in Left.ChildElements)
						Elements.AddLast(EvaluateMultiplication(LeftChild, Right, Node));

					return Left.Encapsulate(Elements, Node);
				}
				else
				{
					ICollection<IElement> LeftChildren = Left.ChildElements;
					ICollection<IElement> RightChildren = Right.ChildElements;

					if (LeftChildren.Count == RightChildren.Count)
					{
						LinkedList<IElement> Elements = new LinkedList<IElement>();
						IEnumerator<IElement> eLeft = LeftChildren.GetEnumerator();
						IEnumerator<IElement> eRight = RightChildren.GetEnumerator();

						try
						{
							while (eLeft.MoveNext() && eRight.MoveNext())
								Elements.AddLast(EvaluateMultiplication(eLeft.Current, eRight.Current, Node));
						}
						finally
						{
							eLeft.Dispose();
							eRight.Dispose();
						}

						return Left.Encapsulate(Elements, Node);
					}
					else
					{
						LinkedList<IElement> LeftResult = new LinkedList<IElement>();

						foreach (IElement LeftChild in LeftChildren)
						{
							LinkedList<IElement> RightResult = new LinkedList<IElement>();

							foreach (IElement RightChild in RightChildren)
								RightResult.AddLast(EvaluateMultiplication(LeftChild, RightChild, Node));

							LeftResult.AddLast(Right.Encapsulate(RightResult, Node));
						}

						return Left.Encapsulate(LeftResult, Node);
					}
				}
			}
		}

		/// <summary>
		/// Performs a scalar multiplication.
		/// </summary>
		/// <param name="N">Scalar.</param>
		/// <param name="Element">Element to multiply <paramref name="N"/> times.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateScalarMultiplication(long N, IGroupElement Element, ScriptNode Node)
		{
			bool Sign = N < 0;
			if (Sign)
				N = -N;

			ISemiGroupElement Result = Element.AssociatedGroup.AdditiveIdentity;
			ISemiGroupElement Power = Element;

			while (N != 0)
			{
				if ((N & 1) != 0)
					Result = Result.AddRight(Power);

				N >>= 1;
				Power = Power.AddRight(Power);
			}

			if (Sign)
				return Negate(Result, Node);
			else
				return Result;
		}

		/// <summary>
		/// Performs a scalar multiplication.
		/// </summary>
		/// <param name="N">Scalar.</param>
		/// <param name="Element">Element to multiply <paramref name="N"/> times.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateScalarMultiplication(double N, IGroupElement Element, ScriptNode Node)
		{
			if (N != Math.Floor(N))
				throw new ScriptRuntimeException("Only scalar multiplication of integers allowed.", Node);

			if (N <= long.MaxValue && N >= long.MinValue)
				return EvaluateScalarMultiplication((long)N, Element, Node);

			bool Sign = N < 0;
			if (Sign)
				N = -N;

			ISemiGroupElement Result = Element.AssociatedGroup.AdditiveIdentity;
			ISemiGroupElement Power = Element;

			while (N >= 1)
			{
				if (Math.IEEERemainder(N, 2) != 0)
				{
					Result = Result.AddRight(Power);
					N -= 1;
				}

				N /= 2;
				Power = Power.AddRight(Power);
			}

			if (Sign)
				return Negate(Result, Node);
			else
				return Result;
		}

		/// <summary>
		/// Performs a scalar multiplication.
		/// </summary>
		/// <param name="N">Scalar.</param>
		/// <param name="Element">Element to multiply <paramref name="N"/> times.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateScalarMultiplication(BigInteger N, IGroupElement Element, ScriptNode Node)
		{
			bool Sign = N.Sign < 0;
			if (Sign)
				N = -N;

			ISemiGroupElement Result = Element.AssociatedGroup.AdditiveIdentity;
			ISemiGroupElement Power = Element;

			while (!N.IsZero)
			{
				N = BigInteger.DivRem(N, two, out BigInteger Rem);
				if (!Rem.IsZero)
					Result = Result.AddRight(Power);

				Power = Power.AddRight(Power);
			}

			if (Sign)
				return Negate(Result, Node);
			else
				return Result;
		}

		/// <summary>
		/// Performs a scalar multiplication.
		/// </summary>
		/// <param name="N">Scalar.</param>
		/// <param name="Element">Element to multiply <paramref name="N"/> times.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateScalarMultiplication(long N, IElement Element, ScriptNode Node)
		{
			bool Sign = N < 0;
			if (Sign)
				N = -N;

			IElement Result = null;
			IElement Power = Element;

			while (N != 0)
			{
				if ((N & 1) != 0)
					Result = Result is null ? Power : Add.EvaluateAddition(Result, Power, Node);

				N >>= 1;
				Power = Add.EvaluateAddition(Power, Power, Node);
			}

			if (Result is null)
				return GetZero(Element, Node);
			else if (Sign)
				return Negate(Result, Node);
			else
				return Result;
		}

		private static IElement Negate(IElement Element, ScriptNode Node)
		{
			if (Element is IAbelianGroupElement E)
				return E.Negate();

			object Value = Element.AssociatedObjectValue;
			if (Value is null)
				return Element;

			Type T = Value.GetType();
			MethodInfo MI = T.GetRuntimeMethod("Negate", Types.NoTypes);
			if (!(MI is null))
				return Expression.Encapsulate(MI.Invoke(Value, Types.NoParameters));

			throw new ScriptRuntimeException("Not an abelian group element.", Node);
		}

		private static IElement GetZero(IElement Element, ScriptNode Node)
		{
			if (Element is IGroupElement E)
				return E.AssociatedGroup.AdditiveIdentity;

			object Value = Element.AssociatedObjectValue;
			if (Value is null)
				return Element;

			Type T = Value.GetType();
			PropertyInfo PI = T.GetRuntimeProperty("Zero");
			if (!(PI is null))
				return Expression.Encapsulate(PI.GetValue(Value));

			FieldInfo FI = T.GetRuntimeField("Zero");
			if (!(FI is null))
				return Expression.Encapsulate(FI.GetValue(Value));

			throw new ScriptRuntimeException("Zero not defined.", Node);
		}

		/// <summary>
		/// Performs a scalar multiplication.
		/// </summary>
		/// <param name="N">Scalar.</param>
		/// <param name="Element">Element to multiply <paramref name="N"/> times.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateScalarMultiplication(double N, IElement Element, ScriptNode Node)
		{
			if (N != Math.Floor(N))
				throw new ScriptRuntimeException("Only scalar multiplication of integers allowed.", Node);

			if (N <= long.MaxValue && N >= long.MinValue)
				return EvaluateScalarMultiplication((long)N, Element, Node);

			bool Sign = N < 0;
			if (Sign)
				N = -N;

			IElement Result = null;
			IElement Power = Element;

			while (N >= 1)
			{
				if (Math.IEEERemainder(N, 2) != 0)
				{
					Result = Result is null ? Power : Add.EvaluateAddition(Result, Power, Node);
					N -= 1;
				}

				N /= 2;
				Power = Add.EvaluateAddition(Power, Power, Node);
			}

			if (Result is null)
				return GetZero(Element, Node);
			else if (Sign)
				return Negate(Result, Node);
			else
				return Result;
		}

		/// <summary>
		/// Performs a scalar multiplication.
		/// </summary>
		/// <param name="N">Scalar.</param>
		/// <param name="Element">Element to multiply <paramref name="N"/> times.</param>
		/// <param name="Node">Node performing the operation.</param>
		/// <returns>Result</returns>
		public static IElement EvaluateScalarMultiplication(BigInteger N, IElement Element, ScriptNode Node)
		{
			bool Sign = N.Sign < 0;
			if (Sign)
				N = -N;

			IElement Result = null;
			IElement Power = Element;

			while (!N.IsZero)
			{
				N = BigInteger.DivRem(N, two, out BigInteger Rem);
				if (!Rem.IsZero)
					Result = Result is null ? Power : Add.EvaluateAddition(Result, Power, Node);

				Power = Add.EvaluateAddition(Power, Power, Node);
			}

			if (Result is null)
				return GetZero(Element, Node);
			else if (Sign)
				return Negate(Result, Node);
			else
				return Result;
		}


		private static readonly BigInteger two = new BigInteger(2);


		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (this.left is IDifferentiable Left &&
				this.right is IDifferentiable Right)
			{
				int Start = this.Start;
				int Len = this.Length;
				Expression Expression = this.Expression;

				return new Add(
					new Multiply(
						Left.Differentiate(VariableName, Variables),
						this.right,
						Start, Len, Expression),
					new Multiply(
						this.left,
						Right.Differentiate(VariableName, Variables),
						Start, Len, Expression),
					Start, Len, Expression);
			}
			else
				throw new ScriptRuntimeException("Factors not differentiable.", this);
		}

	}
}
