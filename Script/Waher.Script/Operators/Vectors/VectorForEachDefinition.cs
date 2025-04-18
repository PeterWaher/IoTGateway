﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Operators.Conditional;

namespace Waher.Script.Operators.Vectors
{
	/// <summary>
	/// Creates a vector using a FOREACH statement.
	/// </summary>
	public class VectorForEachDefinition : BinaryOperator
	{
        private readonly string variableName;

		/// <summary>
		/// Creates a vector using a FOREACH statement.
		/// </summary>
		/// <param name="Elements">Elements.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VectorForEachDefinition(ForEach Elements, int Start, int Length, Expression Expression)
			: base(Elements.LeftOperand, Elements.RightOperand, Start, Length, Expression)
		{
            this.variableName = Elements.VariableName;
		}

        /// <summary>
        /// Variable Name.
        /// </summary>
        public string VariableName => this.variableName;

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
		{
            IElement S = this.left.Evaluate(Variables);
			ChunkedList<IElement> Elements2 = new ChunkedList<IElement>();

			if (!(S is ICollection<IElement> Elements))
            {
                if (S is IVector Vector)
                    Elements = Vector.VectorElements;
                else if (!S.IsScalar)
                    Elements = S.ChildElements;
                else if (S.AssociatedObjectValue is IEnumerable Enumerable)
                {
                    IEnumerator e = Enumerable.GetEnumerator();

                    while (e.MoveNext())
                    {
                        Variables[this.variableName] = e.Current;
                        Elements2.Add(this.right.Evaluate(Variables));
                    }

                    return this.Encapsulate(Elements2);
                }
                else
                    Elements = new IElement[] { S };
            }

            foreach (IElement Element in Elements)
            {
				try
				{
					Variables[this.variableName] = Element;
			    	Elements2.Add(this.right.Evaluate(Variables));
				}
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Elements2.Add(ex.LoopValue);

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
                    if (ex.HasLoopValue)
						Elements2.Add(ex.LoopValue);

					//ScriptContinueLoopException.Reuse(ex);
				}
			}

			return this.Encapsulate(Elements2);
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

            IElement S = await this.left.EvaluateAsync(Variables);
			ChunkedList<IElement> Elements2 = new ChunkedList<IElement>();

			if (!(S is ICollection<IElement> Elements))
            {
                if (S is IVector Vector)
                    Elements = Vector.VectorElements;
                else if (!S.IsScalar)
                    Elements = S.ChildElements;
                else if (S.AssociatedObjectValue is IEnumerable Enumerable)
                {
                    IEnumerator e = Enumerable.GetEnumerator();

                    while (e.MoveNext())
                    {
                        Variables[this.variableName] = e.Current;
                        Elements2.Add(await this.right.EvaluateAsync(Variables));
                    }

                    return this.Encapsulate(Elements2);
                }
                else
                    Elements = new IElement[] { S };
            }

            foreach (IElement Element in Elements)
            {
				try
				{
					Variables[this.variableName] = Element;
			    	Elements2.Add(await this.right.EvaluateAsync(Variables));
				}
				catch (ScriptBreakLoopException ex)
				{
					if (ex.HasLoopValue)
						Elements2.Add(ex.LoopValue);

					//ScriptBreakLoopException.Reuse(ex);
					break;
				}
				catch (ScriptContinueLoopException ex)
				{
					if (ex.HasLoopValue)
						Elements2.Add(ex.LoopValue);

					//ScriptContinueLoopException.Reuse(ex);
					continue;
				}
            }

            return this.Encapsulate(Elements2);
        }

        /// <summary>
        /// Encapsulates the calculated elements.
        /// </summary>
        /// <param name="Elements">Elements to encapsulate.</param>
        /// <returns>Encapsulated element.</returns>
        protected virtual IElement Encapsulate(ChunkedList<IElement> Elements)
        {
            return VectorDefinition.Encapsulate(Elements, true, this);
        }

    }
}
