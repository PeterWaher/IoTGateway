﻿using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Scalar
{
    /// <summary>
    /// Min(x,y)
    /// </summary>
    public class Min : FunctionTwoScalarVariables
    {
        /// <summary>
        /// Min(x,y)
        /// </summary>
        /// <param name="Argument1">Argument 1.</param>
        /// <param name="Argument2">Argument 2.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Min(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length, Expression Expression)
            : base(Argument1, Argument2, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Min);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument1, IElement Argument2, Variables Variables)
        {
			if (!(Argument1.AssociatedSet is IOrderedSet S))
				throw new ScriptRuntimeException("Unable to compare elements.", this);

			if (S.Compare(Argument1, Argument2) < 0)
                return Argument1;
            else
                return Argument2;
        }

		/// <summary>
		/// Evaluates the function on two scalar arguments.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument1, IElement Argument2, Variables Variables)
		{
			return Task.FromResult<IElement>(this.EvaluateScalar(Argument1, Argument2, Variables));
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
        {
            return new DoubleNumber(Math.Min(Argument1, Argument2));
        }

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument1, string Argument2, Variables Variables)
        {
            return new StringValue(string.Compare(Argument1, Argument2) < 0 ? Argument1 : Argument2);
        }

    }
}
