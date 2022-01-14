using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Named function call operator
	/// </summary>
	public class NamedFunctionCall : ScriptNode
	{
		private readonly string functionName;
		private readonly ScriptNode[] arguments;
		private readonly int nrArguments;
		private readonly bool nullCheck;

		/// <summary>
		/// Named function call operator
		/// </summary>
		/// <param name="FunctionName">Function</param>
		/// <param name="Arguments">Arguments</param>
		/// <param name="NullCheck">If null should be returned if operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NamedFunctionCall(string FunctionName, ScriptNode[] Arguments, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.functionName = FunctionName;
			this.arguments = Arguments;
			this.nullCheck = NullCheck;
			this.nrArguments = this.arguments.Length;
		}

		/// <summary>
		/// Function name.
		/// </summary>
		public string FunctionName => this.functionName;

		/// <summary>
		/// Arguments
		/// </summary>
		public ScriptNode[] Arguments => this.arguments;

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			string s = this.nrArguments.ToString();

			if ((!Variables.TryGetVariable(this.functionName + " " + s, out Variable v) &&
			   !Variables.TryGetVariable(this.functionName, out v)) ||
			   (!(v.ValueObject is ILambdaExpression f)))
			{
				if (this.nullCheck)
					return ObjectValue.Null;
				else if (this.nrArguments == 1)
					throw new ScriptRuntimeException("No function defined having 1 argument named '" + this.functionName + "' found.", this);
				else
					throw new ScriptRuntimeException("No function defined having " + s + " arguments named '" + this.functionName + "' found.", this);
			}

			IElement[] Arg = new IElement[this.nrArguments];
			ScriptNode Node;
			int i;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (Node is null)
					Arg[i] = ObjectValue.Null;
				else
					Arg[i] = await Node.EvaluateAsync(Variables);
			}

			if (f.IsAsynchronous)
				return await f.EvaluateAsync(Arg, Variables);
			else
				return f.Evaluate(Arg, Variables);
		}

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="DepthFirst">If calls are made depth first (true) or on each node and then its leaves (false).</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, bool DepthFirst)
		{
			int i;

			if (DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.arguments, State, DepthFirst))
					return false;
			}

			ScriptNode Node;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (!(Node is null))
				{
					bool b = !Callback(Node, out ScriptNode NewNode, State);
					if (!(NewNode is null))
						this.arguments[i] = NewNode;

					if (b)
						return false;
				}
			}

			if (!DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.arguments, State, DepthFirst))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is NamedFunctionCall O &&
				this.functionName.Equals(O.functionName) &&
				this.nullCheck.Equals(O.nullCheck) &&
				AreEqual(this.arguments, O.arguments) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.functionName.GetHashCode();
			Result ^= Result << 5 ^ this.nullCheck.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.arguments);
			return Result;
		}
	}
}
