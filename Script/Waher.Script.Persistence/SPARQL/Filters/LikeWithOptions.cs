using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Comparisons;

namespace Waher.Script.Persistence.SPARQL.Filters
{
    /// <summary>
    /// Extension of the Like operator, that allows the script to set options.
    /// </summary>
    public class LikeWithOptions : Like, IFilterNode
    {
        private ScriptNode options;

        /// <summary>
        /// Extension of the Like operator, that allows the script to set options.
        /// </summary>
        /// <param name="Argument1">Left argument</param>
        /// <param name="Argument2">Right argument</param>
        /// <param name="Argument3">Options argument</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public LikeWithOptions(ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3,
            int Start, int Length, Expression Expression)
            : base(Argument1, Argument2, Start, Length, Expression)
        {
            this.options = Argument3;
			this.PartialMatch = true;

			this.CalcIsAsync();
        }

        /// <summary>
        /// Recalculates if operator is asynchronous or not.
        /// </summary>
        protected override void CalcIsAsync()
        {
            base.CalcIsAsync();

            if (this.options?.IsAsynchronous ?? false)
				this.isAsync = true;
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override IElement Evaluate(Variables Variables)
        {
            if (!(this.options is null))
				this.SetOptions(this.options.Evaluate(Variables));

            IElement Left = SemanticElementToRegexString(this.left.Evaluate(Variables));
            IElement Right = SemanticElementToRegexString(this.right.Evaluate(Variables));

            return this.EvaluateScalar(Left, Right, Variables);
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override async Task<IElement> EvaluateAsync(Variables Variables)
        {
            if (!(this.options is null))
				this.SetOptions(await this.options.EvaluateAsync(Variables));

            IElement Left = SemanticElementToRegexString(await this.left.EvaluateAsync(Variables));
            IElement Right = SemanticElementToRegexString(await this.right.EvaluateAsync(Variables));

            return await this.EvaluateScalarAsync(Left, Right, Variables);
        }

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <param name="Cube">Semantic cube being processed.</param>
		/// <param name="VariablesProcessed">Variables already processed.</param>
		/// <param name="Query">Query being processed.</param>
		/// <param name="Possibility">Current possibility being evaluated.</param>
		/// <returns>Result.</returns>
		public Task<IElement> EvaluateAsync(Variables Variables, ISemanticCube Cube,
			Dictionary<string, bool> VariablesProcessed, SparqlQuery Query, Possibility Possibility)
        {
            return this.EvaluateAsync(Variables);
        }

		/// <summary>
		/// Reference to the underlying script node.
		/// </summary>
		public ScriptNode ScriptNode => this;

		private static IElement SemanticElementToRegexString(IElement E)
        {
            if (E is StringValue S)
                return S;
            else if (E.AssociatedObjectValue is StringLiteral L)
                return new StringValue(L.StringValue);
            else
                return new StringValue(E.AssociatedObjectValue?.ToString() ?? string.Empty);
        }

        private void SetOptions(IElement Options)
        {
            Options = SemanticElementToRegexString(Options);
            if (!(Options.AssociatedObjectValue is string s))
                throw new ScriptRuntimeException("Options argument to regex function must be a string.", this);

            RegexOptions Result = RegexOptions.Singleline;

            foreach (char ch in s)
            {
                switch (ch)
                {
                    case 'i':
                        Result |= RegexOptions.IgnoreCase;
                        break;

                    case 'm':
                        Result &= ~RegexOptions.Singleline;
                        Result |= RegexOptions.Multiline;
                        break;

                    case 'x':
                        Result |= RegexOptions.IgnorePatternWhitespace;
                        break;

                    default:
                        throw new ScriptRuntimeException("Regular expression option not supported: " + ch, this);
                }
            }

            this.Options = Result;
        }

        /// <summary>
        /// Calls the callback method for all child nodes.
        /// </summary>
        /// <param name="Callback">Callback method to call.</param>
        /// <param name="State">State object to pass on to the callback method.</param>
        /// <param name="Order">Order to traverse the nodes.</param>
        /// <returns>If the process was completed.</returns>
        public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
        {
            if (Order == SearchMethod.DepthFirst)
            {
                if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true))
                    return false;

                if (!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true))
                    return false;

                if (!(this.options?.ForAllChildNodes(Callback, State, Order) ?? true))
                    return false;
            }

            ScriptNode NewNode;
            bool RecalcIsAsync = false;
            bool b;

            if (!(this.left is null))
            {
                b = !Callback(this.left, out NewNode, State);
                if (!(NewNode is null))
                {
					this.left = NewNode;
					this.left.SetParent(this);

                    RecalcIsAsync = true;
                }

                if (b || Order == SearchMethod.TreeOrder && !this.left.ForAllChildNodes(Callback, State, Order))
                {
                    if (RecalcIsAsync)
						this.CalcIsAsync();

                    return false;
                }
            }

            if (!(this.right is null))
            {
                b = !Callback(this.right, out NewNode, State);
                if (!(NewNode is null))
                {
					this.right = NewNode;
					this.right.SetParent(this);

                    RecalcIsAsync = true;
                }

                if (b || Order == SearchMethod.TreeOrder && !this.right.ForAllChildNodes(Callback, State, Order))
                {
                    if (RecalcIsAsync)
						this.CalcIsAsync();

                    return false;
                }
            }

            if (!(this.options is null))
            {
                b = !Callback(this.options, out NewNode, State);
                if (!(NewNode is null))
                {
                    this.options = NewNode;
					this.options.SetParent(this);

                    RecalcIsAsync = true;
                }

                if (b || Order == SearchMethod.TreeOrder && !this.options.ForAllChildNodes(Callback, State, Order))
                {
                    if (RecalcIsAsync)
						this.CalcIsAsync();

                    return false;
                }
            }

            if (RecalcIsAsync)
				this.CalcIsAsync();

            if (Order == SearchMethod.BreadthFirst)
            {
                if (!(this.left?.ForAllChildNodes(Callback, State, Order) ?? true))
                    return false;

                if (!(this.right?.ForAllChildNodes(Callback, State, Order) ?? true))
                    return false;

                if (!(this.options?.ForAllChildNodes(Callback, State, Order) ?? true))
                    return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is LikeWithOptions O &&
                AreEqual(this.left, O.left) &&
                AreEqual(this.right, O.right) &&
                AreEqual(this.options, O.options) &&
                base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int Result = base.GetHashCode();
            Result ^= Result << 5 ^ GetHashCode(this.left);
            Result ^= Result << 5 ^ GetHashCode(this.right);
            Result ^= Result << 5 ^ GetHashCode(this.options);
            return Result;
        }
    }
}
