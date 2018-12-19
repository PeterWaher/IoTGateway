using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Lambda expression executing object methods.
	/// </summary>
	public class MethodLambda : ILambdaExpression
	{
		private readonly object obj;
		private readonly MethodInfo method;
		private readonly ParameterInfo[] parameters;
		private string[] argumentNames = null;
		private ArgumentType[] argumentTypes = null;

		/// <summary>
		/// Lambda expression executing object methods.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <param name="Method">Method</param>
		public MethodLambda(object Object, MethodInfo Method)
		{
			this.obj = Object;
			this.method = Method;
			this.parameters = Method.GetParameters();
		}

		/// <summary>
		/// Number of arguments.
		/// </summary>
		public int NrArguments => this.parameters.Length;

		/// <summary>
		/// Argument Names.
		/// </summary>
		public string[] ArgumentNames
		{
			get
			{
				if (this.argumentNames is null)
				{
					int i, c = this.parameters.Length;
					string[] Names = new string[c];

					for (i = 0; i < c; i++)
						Names[i] = this.parameters[i].Name;

					this.argumentNames = Names;
				}

				return this.argumentNames;
			}
		}

		/// <summary>
		/// Argument types.
		/// </summary>
		public ArgumentType[] ArgumentTypes
		{
			get
			{
				if (this.argumentTypes is null)
				{
					int i, c = this.parameters.Length;
					ArgumentType[] Types = new ArgumentType[c];

					for (i = 0; i < c; i++)
					{
						Type T = this.parameters[i].GetType();
						if (T.IsArray || T is IVector)
							Types[i] = ArgumentType.Vector;
						else if (T is IMatrix)
							Types[i] = ArgumentType.Matrix;
						else if (T is Abstraction.Sets.ISet)
							Types[i] = ArgumentType.Set;
						else
							Types[i] = ArgumentType.Normal;
					}

					this.argumentTypes = Types;
				}

				return this.argumentTypes;
			}
		}

		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			int i, c = Arguments.Length;
			object[] P = new object[c];
			object Obj;
			Type T, T0;

			for (i = 0; i < c; i++)
			{
				Obj = Arguments[i].AssociatedObjectValue;
				if (Obj != null)
				{
					T = Obj.GetType();
					T0 = this.parameters[i].ParameterType;
					if (T != T0)
					{
						if (Arguments[i].TryConvertTo(T0, out object Obj0))
							Obj = Obj0;
					}
				}

				P[i] = Obj;
			}

			return Expression.Encapsulate(this.method.Invoke(this.obj, P));
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return Operators.LambdaDefinition.ToString(this);
		}
	}
}
