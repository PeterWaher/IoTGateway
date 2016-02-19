using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators.Comparisons
{
	/// <summary>
	/// Not Like
	/// </summary>
	public class NotLike : BinaryScalarOperator
    {
		/// <summary>
		/// Not Like
		/// </summary>
		/// <param name="Left">Left operand.</param>
		/// <param name="Right">Right operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public NotLike(ScriptNode Left, ScriptNode Right, int Start, int Length)
			: base(Left, Right, Start, Length)
		{
		}

        /// <summary>
        /// Evaluates the operator on scalar operands.
        /// </summary>
        /// <param name="Left">Left value.</param>
        /// <param name="Right">Right value.</param>
        /// <returns>Result</returns>
        public override IElement EvaluateScalar(IElement Left, IElement Right)
        {
            StringValue L = Left as StringValue;
            if (L == null)
                throw new ScriptRuntimeException("String values expected.", this);

            StringValue R = Right as StringValue;
            if (R == null)
                throw new ScriptRuntimeException("String values expected.", this);

            string sl = L.Value;
            string sr = R.Value;
            Match M;

            lock (this.synchObject)
            {
                if (this.lastExpression == null || sr != this.lastExpression)
                {
                    this.lastExpression = sr;
                    this.regex = new Regex(sr, RegexOptions.Singleline);
                }

                M = this.regex.Match(sl);
            }

            if (M.Success && M.Index == 0 && M.Length == sl.Length)
                return BooleanValue.False;
            else
                return BooleanValue.True;
        }

        private Regex regex = null;
        private string lastExpression = null;
        private object synchObject = new object();
    }
}
