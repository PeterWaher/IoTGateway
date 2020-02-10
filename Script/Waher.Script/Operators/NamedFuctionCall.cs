using System;
using System.Collections.Generic;
using System.Text;
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
		}

		/// <summary>
		/// Function name.
		/// </summary>
		public string FunctionName
		{
			get { return this.functionName; }
		}

		/// <summary>
		/// Arguments
		/// </summary>
		public ScriptNode[] Arguments
		{
			get { return this.arguments; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			string s = this.arguments.Length.ToString();

			if ((!Variables.TryGetVariable(this.functionName + " " + s, out Variable v) &&
			   !Variables.TryGetVariable(this.functionName, out v)) ||
			   (!(v.ValueElement is ILambdaExpression f)))
			{
				if (this.nullCheck)
					return ObjectValue.Null;
				else if (this.arguments.Length == 1)
					throw new ScriptRuntimeException("No function defined having 1 argument named '" + this.functionName + "' found.", this);
				else
					throw new ScriptRuntimeException("No function defined having " + s + " arguments named '" + this.functionName + "' found.", this);
			}

			int i, c = this.arguments.Length;
			IElement[] Arg = new IElement[c];
			ScriptNode Node;

			for (i = 0; i < c; i++)
			{
				Node = this.arguments[i];
				if (Node is null)
					Arg[i] = ObjectValue.Null;
				else
					Arg[i] = Node.Evaluate(Variables);
			}

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
			int i, c=this.arguments.Length;

			if (DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.arguments, State, DepthFirst))
					return false;
			}

			for (i = 0; i < c; i++)
			{
				if (!(this.arguments[i] is null) && !Callback(ref this.arguments[i], State))
					return false;
			}

			if (!DepthFirst)
			{
				if (!ForAllChildNodes(Callback, this.arguments, State, DepthFirst))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is NamedFunctionCall O &&
				this.functionName.Equals(O.functionName) &&
				this.nullCheck.Equals(O.nullCheck) &&
				AreEqual(this.arguments, O.arguments) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
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
