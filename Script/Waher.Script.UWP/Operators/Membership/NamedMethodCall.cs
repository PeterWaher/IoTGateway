using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Named method call operator.
	/// </summary>
	public class NamedMethodCall : UnaryOperator
	{
		private string name;
		private ScriptNode[] parameters;
		private int nrParameters;

		/// <summary>
		/// Named method call operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name</param>
		/// <param name="Parameters">Method arguments.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public NamedMethodCall(ScriptNode Operand, string Name, ScriptNode[] Parameters, int Start, int Length)
			: base(Operand, Start, Length)
		{
			this.name = Name;
			this.parameters = Parameters;
			this.nrParameters = Parameters.Length;
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Method arguments.
		/// </summary>
		public ScriptNode[] Parameters
		{
			get { return this.parameters; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement E = this.op.Evaluate(Variables);
			object Object = E.AssociatedObjectValue;
			Type T = Object.GetType();
			IElement[] Arguments = null;
			object[] ParameterValues;
			bool[] Extend;
			object Value;
			int i;
			bool DoExtend = false;

			lock (this.synchObject)
			{
				if (this.lastType != T)
				{
					this.method = null;
					this.methods = null;
					this.lastType = T;
				}

				Arguments = new IElement[this.nrParameters];
				for (i = 0; i < this.nrParameters; i++)
					Arguments[i] = this.parameters[i].Evaluate(Variables);

				if (this.method != null)
				{
					if (this.methodParametersTypes.Length != this.nrParameters)
					{
						this.method = null;
						this.methods = null;
					}
					else
					{
						for (i = 0; i < this.methodParametersTypes.Length; i++)
						{
							if (!Arguments[i].TryConvertTo(this.methodParametersTypes[i].ParameterType, out Value))
							{
								if (Arguments[i].IsScalar)
									break;

								this.methodArgumentExtensions[i] = true;
								this.methodArguments[i] = null;
								DoExtend = true;
							}
							else
							{
								this.methodArgumentExtensions[i] = false;
								this.methodArguments[i] = Value;
							}
						}

						if (i < this.methodParametersTypes.Length)
						{
							this.method = null;
							this.methods = null;
						}
					}
				}

				if (this.method == null)
				{
					if (this.methods == null)
						this.methods = this.GetMethods(T);

					ParameterValues = null;
					Extend = null;

					foreach (KeyValuePair<MethodInfo, ParameterInfo[]> P in this.methods)
					{
						DoExtend = false;

						for (i = 0; i < this.nrParameters; i++)
						{
							if (!Arguments[i].TryConvertTo(P.Value[i].ParameterType, out Value))
							{
								if (Arguments[i].IsScalar)
									break;

								if (Extend == null)
								{
									Extend = new bool[this.nrParameters];
									ParameterValues = new object[this.nrParameters];
								}

								Extend[i] = true;
								ParameterValues[i] = null;
								DoExtend = true;
							}
							else
							{
								if (ParameterValues == null)
								{
									Extend = new bool[this.nrParameters];
									ParameterValues = new object[this.nrParameters];
								}

								Extend[i] = false;
								ParameterValues[i] = Value;
							}
						}

						if (i < this.nrParameters)
							continue;

						this.method = P.Key;
						this.methodParametersTypes = P.Value;
						this.methodArguments = ParameterValues;
						this.methodArgumentExtensions = Extend;
						break;
					}

					if (this.method == null)
						throw new ScriptRuntimeException("Invalid number or type of parameters.", this);
				}
			}

			if (DoExtend)
			{
				return this.EvaluateCanonical(Object, this.method, this.methodParametersTypes, Arguments,
					this.methodArguments, this.methodArgumentExtensions);
			}
			else
				return Expression.Encapsulate(this.method.Invoke(Object, this.methodArguments));
		}

		private IElement EvaluateCanonical(object Object, MethodInfo Method, ParameterInfo[] ParametersTypes,
			IElement[] Arguments, object[] ArgumentValues, bool[] Extend)
		{
			IEnumerator<IElement>[] Enumerators = null;
			ICollection<IElement> Children;
			IEnumerator<IElement> e;
			IElement First = null;
			int i, c = 0;

			for (i = 0; i < this.nrParameters; i++)
			{
				if (Extend[i])
				{
					if (Arguments[i].IsScalar)
					{
						if (!Arguments[i].TryConvertTo(ParametersTypes[i].ParameterType, out ArgumentValues[i]))
							throw new ScriptRuntimeException("Inconsistent argument types.", this);
					}
					else
					{
						Children = Arguments[i].ChildElements;
						if (First == null)
						{
							Enumerators = new IEnumerator<IElement>[this.nrParameters];
							First = Arguments[i];
							c = Children.Count;
						}
						else if (c != Children.Count)
							throw new ScriptRuntimeException("Incompatible dimensions.", this);

						Enumerators[i] = Children.GetEnumerator();
					}
				}
			}

			if (First == null)
				return Expression.Encapsulate(Method.Invoke(Object, ArgumentValues));

			LinkedList<IElement> Elements = new LinkedList<IElement>();
			Arguments = (IElement[])Arguments.Clone();

			while (true)
			{
				for (i = 0; i < this.nrParameters; i++)
				{
					if (!Extend[i])
						continue;

					if (!(e = Enumerators[i]).MoveNext())
						break;

					Arguments[i] = e.Current;
				}

				if (i < this.nrParameters)
					break;

				Elements.AddLast(this.EvaluateCanonical(Object, Method, ParametersTypes, Arguments, ArgumentValues, Extend));
			}

			for (i = 0; i < this.nrParameters; i++)
			{
				if (Extend[i])
					Enumerators[i].Dispose();
			}

			return First.Encapsulate(Elements, this);
		}

		private KeyValuePair<MethodInfo, ParameterInfo[]>[] GetMethods(Type Type)
		{
			List<KeyValuePair<MethodInfo, ParameterInfo[]>> Result = new List<KeyValuePair<MethodInfo, ParameterInfo[]>>();
			ParameterInfo[] ParameterInfo;

			foreach (MethodInfo MI in Type.GetMethods())
			{
				if (MI.Name != this.name)
					continue;

				ParameterInfo = MI.GetParameters();
				if (ParameterInfo.Length != this.nrParameters)
					continue;

				Result.Add(new KeyValuePair<MethodInfo, System.Reflection.ParameterInfo[]>(MI, ParameterInfo));
			}

			return Result.ToArray();
		}

		private Type lastType = null;
		private MethodInfo method = null;
		private ParameterInfo[] methodParametersTypes = null;
		private KeyValuePair<MethodInfo, ParameterInfo[]>[] methods = null;
		private object[] methodArguments = null;
		private bool[] methodArgumentExtensions = null;
		private object synchObject = new object();
	}
}
