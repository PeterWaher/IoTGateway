using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators;

namespace Waher.Script.Fractals.IFS.Variations
{
	/// <summary>
	/// TODO
	/// </summary>
	public abstract class FlameVariationZeroParameters : FunctionZeroVariables, IFlameVariation
	{
		/// <summary>
		/// TODO
		/// </summary>
		protected double[] homogeneousTransform = new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };

		/// <summary>
		/// TODO
		/// </summary>
		protected double variationWeight = 1;

		/// <summary>
		/// TODO
		/// </summary>
		public FlameVariationZeroParameters(int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override IElement Evaluate(Variables Variables)
		{
			return this;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override string ToString()
		{
			return LambdaDefinition.ToString(this);
		}

		#region IFlameVariation Members

		/// <summary>
		/// TODO
		/// </summary>
		public abstract void Operate(ref double x, ref double y);

		/// <summary>
		/// TODO
		/// </summary>
		public void Initialize(double[] HomogeneousTransform, double VariationWeight)
		{
			this.homogeneousTransform = HomogeneousTransform;
			this.variationWeight = VariationWeight;
		}

		#endregion

		#region ILambdaExpression Members

		/// <summary>
		/// TODO
		/// </summary>
		public IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double x = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			double y = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

			this.Operate(ref x, ref y);

			return new DoubleVector(new double[] { x, y });
		}

		/// <summary>
		/// TODO
		/// </summary>
		public Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			return Task.FromResult(this.Evaluate(Arguments, Variables));
		}

		/// <summary>
		/// TODO
		/// </summary>
		public int NrArguments => 2;

		/// <summary>
		/// TODO
		/// </summary>
		public string[] ArgumentNames => parameterNames;

		/// <summary>
		/// TODO
		/// </summary>
		public ArgumentType[] ArgumentTypes => parameterTypes;

		internal static readonly string[] parameterNames = new string[] { "x", "y" };
		internal static readonly ArgumentType[] parameterTypes = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar };

		#endregion

		#region IElement members

		/// <summary>
		/// TODO
		/// </summary>
		public IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			return this;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public bool TryConvertTo(Type DesiredType, out object Value)
		{
			Value = null;
			return false;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public ISet AssociatedSet => SetOfVariations.Instance;

		/// <summary>
		/// TODO
		/// </summary>
		public object AssociatedObjectValue => this;

		/// <summary>
		/// TODO
		/// </summary>
		public bool IsScalar => false;

		/// <summary>
		/// TODO
		/// </summary>
		public ICollection<IElement> ChildElements => new IElement[0];

		#endregion
	}
}
