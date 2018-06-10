using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Operators;

namespace Waher.Script.Fractals.IFS.Variations
{
    public abstract class FlameVariationZeroParameters : Function, IFlameVariation
    {
        protected double[] homogeneousTransform = new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 };
        protected double variationWeight = 1;

        public FlameVariationZeroParameters(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[0];
			}
		}

		public override IElement Evaluate(Variables Variables)
        {
			return this;
        }

        public override string ToString()
        {
            return LambdaDefinition.ToString(this);
        }

        #region IFlameVariation Members

        public abstract void Operate(ref double x, ref double y);

        public void Initialize(double[] HomogeneousTransform, double VariationWeight)
        {
            this.homogeneousTransform = HomogeneousTransform;
            this.variationWeight = VariationWeight;
        }

        #endregion

        #region ILambdaExpression Members

        IElement ILambdaExpression.Evaluate(IElement[] Arguments, Variables Variables)
        {
            double x = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double y = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

            this.Operate(ref x, ref y);

			return new DoubleVector(new double[] { x, y });
		}

		ILambdaExpression ILambdaExpression.Differentiate(string VariableName, Variables Variables)
		{
			throw new NotImplementedException();
		}

		int ILambdaExpression.NrArguments
		{
			get { return 2; }
		}

		string[] ILambdaExpression.ArgumentNames
		{
			get { return parameterNames; }
		}

		ArgumentType[] ILambdaExpression.ArgumentTypes
		{
			get { return parameterTypes; }
		}

		internal static readonly string[] parameterNames = new string[] { "x", "y" };
        internal static readonly ArgumentType[] parameterTypes = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar };

		#endregion

		#region IElement members

		public IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			return this;
		}

		public bool TryConvertTo(Type DesiredType, out object Value)
		{
			Value = null;
			return false;
		}

		public ISet AssociatedSet
		{
			get
			{
				return SetOfVariations.Instance;
			}
		}

		public object AssociatedObjectValue
		{
			get
			{
				return this;
			}
		}

		public bool IsScalar
		{
			get
			{
				return false;
			}
		}

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
