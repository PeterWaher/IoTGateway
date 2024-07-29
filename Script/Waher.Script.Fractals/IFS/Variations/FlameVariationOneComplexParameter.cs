using System;
using System.Collections.Generic;
using System.Numerics;
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
	public abstract class FlameVariationOneComplexParameter : FunctionMultiVariate, IFlameVariation
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
		protected double re;

		/// <summary>
		/// TODO
		/// </summary>
		protected double im;

		/// <summary>
		/// TODO
		/// </summary>
		public FlameVariationOneComplexParameter(ScriptNode Parameter1, ScriptNode Parameter2, 
            int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Parameter1, Parameter2 },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
        {
        }

		/// <summary>
		/// TODO
		/// </summary>
		protected FlameVariationOneComplexParameter(Complex z, ScriptNode Parameter,
			int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Parameter }, new ArgumentType[] { ArgumentType.Scalar }, 
				  Start, Length, Expression)
        {
            this.re = z.Real;
            this.im = z.Imaginary;
        }

		/// <summary>
		/// TODO
		/// </summary>
		protected FlameVariationOneComplexParameter(double Re, double Im, ScriptNode Parameter1,
            ScriptNode Parameter2, int Start, int Length, Expression Expression)
            : base(new ScriptNode[] { Parameter1, Parameter2 },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
        {
            this.re = Re;
            this.im = Im;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "z" };
			}
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
		public void Initialize(double[] HomogeneousTransform, double VariationWeight)
        {
            this.homogeneousTransform = HomogeneousTransform;
            this.variationWeight = VariationWeight;
        }

		/// <summary>
		/// TODO
		/// </summary>
		public abstract void Operate(ref double x, ref double y);

        #endregion

        #region ILambdaExpression Members

        IElement ILambdaExpression.Evaluate(IElement[] Parameters, Variables Variables)
        {
            double x = Expression.ToDouble(Parameters[0].AssociatedObjectValue);
            double y = Expression.ToDouble(Parameters[1].AssociatedObjectValue);

            this.Operate(ref x, ref y);

			return new DoubleVector(new double[] { x, y });
		}

		int ILambdaExpression.NrArguments
		{
			get { return 2; }
		}

		string[] ILambdaExpression.ArgumentNames
		{
			get { return FlameVariationZeroParameters.parameterNames; }
		}

		ArgumentType[] ILambdaExpression.ArgumentTypes
		{
			get { return FlameVariationZeroParameters.parameterTypes; }
		}

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
		public ISet AssociatedSet
		{
			get
			{
				return SetOfVariations.Instance;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public object AssociatedObjectValue
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public bool IsScalar
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// TODO
		/// </summary>
		public ICollection<IElement> ChildElements
		{
			get
			{
				return new IElement[0];
			}
		}

		#endregion
	}
}
