﻿using System;
using System.Collections.Generic;
using Waher.Runtime.Collections;
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
	public abstract class FlameVariationMultipleParameters : FunctionMultiVariate, IFlameVariation
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
		public FlameVariationMultipleParameters(ScriptNode[] Arguments, ArgumentType[] ArgumentTypes, int Start, int Length, Expression Expression)
            : base(Arguments, ArgumentTypes, Start, Length, Expression)
        {
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

		IElement ILambdaExpression.Evaluate(IElement[] Arguments, Variables Variables)
        {
            double x = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double y = Expression.ToDouble(Arguments[1].AssociatedObjectValue);

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
		public IElement Encapsulate(ChunkedList<IElement> Elements, ScriptNode Node)
		{
			return this;
		}

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
				return Array.Empty<IElement>();
			}
		}

		#endregion

	}
}
