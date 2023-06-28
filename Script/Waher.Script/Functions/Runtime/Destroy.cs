using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Membership;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Destroys a value. If the function references a variable, the variable is also removed.
	/// </summary>
	public class Destroy : FunctionOneVariable
	{
		private readonly string variableName;
		private readonly bool isMember;
		private readonly NamedMember member;

		/// <summary>
		/// Destroys a value. If the function references a variable, the variable is also removed.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Destroy(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
			if (Argument is VariableReference Ref)
			{
				this.variableName = Ref.VariableName;
				this.isMember = false;
				this.member = null;
			}
			else if (Argument is NamedMember Member)
			{
				this.variableName = Member.Name;
				this.isMember = true;
				this.member = Member;
			}
			else
			{
				this.variableName = string.Empty;
				this.isMember = false;
				this.member = null;
			}
		}

		/// <summary>
		/// Name of variable.
		/// </summary>
		public string VariableName => this.variableName;

		/// <summary>
		/// If argument is a named member reference. <see cref="VariableName"/> will contain
		/// the name of a named member.
		/// </summary>
		public bool IsMember => this.IsMember;

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Destroy);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "delete" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
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
			IElement Result = ObjectValue.Null;
			IElement Element;

			if (this.isMember)
			{
				IElement Obj = await this.member.Operand.EvaluateAsync(Variables);

				try
				{
					Element = await this.member.EvaluateAsync(Obj, Variables);
				}
				catch (Exception)
				{
					return BooleanValue.False;
				}

				if (await RemovePropertyIfPossible(Obj, this.variableName))
					Result = BooleanValue.True;
				else
					Result = BooleanValue.False;
			}
			else if (!string.IsNullOrEmpty(this.variableName))
			{
				if (Variables.TryGetVariable(this.variableName, out Variable v))
				{
					if (Variables.Remove(this.variableName))
						Result = BooleanValue.True;
					else
						Result = BooleanValue.False;

					Element = v.ValueElement;
				}
				else
					Element = null;
			}
			else
				Element = await this.Argument.EvaluateAsync(Variables);

			if (!(Element is null))
			{
				if (Element.AssociatedObjectValue is IDisposable D)
					D.Dispose();
			}

			return Result;
		}

		private static async Task<bool> RemovePropertyIfPossible(IElement Element, string Name)
		{
			object Obj = Element.AssociatedObjectValue;

			if (Obj is IDictionary<string, IElement> ObjExNihilo)
				return ObjExNihilo.Remove(Name);
			else if (Obj is IDictionary<string, object> Object)
				return Object.Remove(Name);
			else if (Obj is null)
				return false;
			else
			{
				Type T = Obj.GetType();
				MethodInfo MI = T.GetRuntimeMethod("Remove", stringArgument);
				if (MI is null)
					return false;

				object Result = MI.Invoke(Obj, new object[] { Name });
				Result = await WaitPossibleTask(Result);

				if (Result is bool b)
					return b;
				else
					return false;
			}
		}

		private static readonly Type[] stringArgument = new Type[] { typeof(string) };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return ObjectValue.Null;
		}
	}
}
