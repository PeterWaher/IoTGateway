using System;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Functions.Runtime
{
    /// <summary>
    /// Removes a variable from the variables collection, without destroying its value.
    /// </summary>
    public class Remove : FunctionOneVariable
    {
        private string variableName;
        private ScriptNode @object = null;

        /// <summary>
        /// Removes a variable from the variables collection, without destroying its value.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Remove(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
            this.CheckArgument();
        }

        private void CheckArgument()
        {
            if (this.Argument is null)
            {
                this.@object = null;
                this.variableName = string.Empty;
            }
            else
            {
                if (this.Argument is VariableReference Ref)
                {
                    this.@object = null;
                    this.variableName = Ref.VariableName;
                }
                else if (this.Argument is NamedMember Member)
                {
                    this.@object = Member.Operand;
                    this.variableName = Member.Name;
                }
                else
                    throw new SyntaxException("Variable reference or named property expected.", Argument.Start, string.Empty);
            }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "var" };

        /// <summary>
        /// Name of variable.
        /// </summary>
        public string VariableName => this.variableName;

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Remove);

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
        {
            if (this.@object is null)
            {
                if (Variables.TryGetVariable(this.variableName, out Variable v))
                {
                    Variables.Remove(this.variableName);
                    return v.ValueElement;
                }
                else
                    return ObjectValue.Null;
            }
            else
			{
                IElement Obj = this.@object.Evaluate(Variables);
                Type T = Obj.AssociatedObjectValue.GetType();
                MethodInfo MI = T.GetRuntimeMethod("Remove", stringArgument);
                if (MI is null)
                    throw new ScriptRuntimeException("Unable to remove property " + this.variableName + " from objects of type " + T.FullName + ".", this);

                object Result = MI.Invoke(Obj.AssociatedObjectValue, new object[] { this.variableName });

                return Expression.Encapsulate(UnnestPossibleTaskSync(Result));
            }
        }

        /// <summary>
        /// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
        /// This method should be used for nodes whose <see cref="ScriptNode.IsAsynchronous"/> is true.
        /// </summary>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Result.</returns>
        public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
            if (this.@object is null)
            {
                if (Variables.TryGetVariable(this.variableName, out Variable v))
                {
                    Variables.Remove(this.variableName);
                    return v.ValueElement;
                }
                else
                    return ObjectValue.Null;
            }
            else
            {
                IElement Obj = await this.@object.EvaluateAsync(Variables);
                Type T = Obj.AssociatedObjectValue.GetType();
                MethodInfo MI = T.GetRuntimeMethod("Remove", stringArgument);
                if (MI is null)
                    throw new ScriptRuntimeException("Unable to remove property " + this.variableName + " from objects of type " + T.FullName + ".", this);

                object Result = MI.Invoke(Obj.AssociatedObjectValue, new object[] { this.variableName });

                return Expression.Encapsulate(await WaitPossibleTask(Result));
            }
        }

        internal static readonly Type[] stringArgument = new Type[] { typeof(string) };

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement Evaluate(IElement Argument, Variables Variables)
		{
            return Argument;
        }

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override Task<IElement> EvaluateAsync(IElement Argument, Variables Variables)
		{
            return Task.FromResult<IElement>(Argument);
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
            ScriptNode Argument = this.Argument;
			bool Result = base.ForAllChildNodes(Callback, State, Order);

            if (Argument != this.Argument)
                this.CheckArgument();

            return Result;
		}


	}
}
