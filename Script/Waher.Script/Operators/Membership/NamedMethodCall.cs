﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Named method call operator.
	/// </summary>
	public class NamedMethodCall : NullCheckUnaryOperator
	{
		private readonly string name;
		private readonly ScriptNode[] parameters;
		private readonly int nrParameters;

		/// <summary>
		/// Named method call operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name</param>
		/// <param name="Parameters">Method arguments.</param>
		/// <param name="NullCheck">If null should be returned if operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NamedMethodCall(ScriptNode Operand, string Name, ScriptNode[] Parameters, bool NullCheck,
			int Start, int Length, Expression Expression)
			: base(Operand, NullCheck, Start, Length, Expression)
		{
			this.name = Name;

			this.parameters = Parameters;
			this.parameters?.SetParent(this);

			this.nrParameters = Parameters.Length;
			this.isAsync = true;
		}

		/// <summary>
		/// Name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Method arguments.
		/// </summary>
		public ScriptNode[] Parameters => this.parameters;

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(IElement Operand, Variables Variables)
		{
			return this.EvaluateAsync(Operand, Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement Operand, Variables Variables)
		{
			object Object = Operand.AssociatedObjectValue;
			Type T;
			IElement[] Arguments;
			object Instance;
			int i;

			if (Object is null && this.nullCheck)
				return ObjectValue.Null;

			T = Object as Type;
			if (T is null)
			{
				T = Object?.GetType() ?? typeof(object);
				Instance = Object;
			}
			else
				Instance = null;

			Arguments = new IElement[this.nrParameters];
			for (i = 0; i < this.nrParameters; i++)
				Arguments[i] = await this.parameters[i].EvaluateAsync(Variables);

			IElement Result = await this.EvaluateAsync(T, Instance, Arguments, Variables);

			if (Result is null)
			{
				if (Operand.IsScalar)
					throw new ScriptRuntimeException("Invalid number or type of parameters.", this);

				ChunkedList<IElement> Elements = new ChunkedList<IElement>();

				foreach (IElement Item in Operand.ChildElements)
					Elements.Add(await this.EvaluateAsync(Item, Variables));

				return Operand.Encapsulate(Elements, this);
			}

			return Result;
		}

		/// <summary>
		/// Executes a code-behind method.
		/// </summary>
		/// <param name="T">Type</param>
		/// <param name="Instance">Object instance, or null if static method.</param>
		/// <param name="Arguments">Method arguments.</param>
		/// <param name="Variables">Variables. If null, argument extensions will not be evaluated.</param>
		/// <returns>Result, or null if no suitable method found.</returns>
		public async Task<IElement> EvaluateAsync(Type T, object Instance, IElement[] Arguments, Variables Variables)
		{
			Type PT;
			object[] ParameterValues;
			bool[] Extend;
			object Value;
			bool DoExtend = false;
			int i;

			lock (this.synchObject)
			{
				if (this.lastType != T)
				{
					this.method = null;
					this.methodType = MethodType.Method;
					this.methods = null;
					this.byReference = null;
					this.lastType = T;
				}

				if (!(this.method is null) && this.methodType == MethodType.Method)
				{
					if (this.methodParametersTypes.Length != this.nrParameters)
					{
						this.method = null;
						this.methodType = MethodType.Method;
						this.methods = null;
						this.byReference = null;
					}
					else
					{
						for (i = 0; i < this.methodParametersTypes.Length; i++)
						{
							PT = this.methodParametersTypes[i].ParameterType;

							if (PT.IsByRef && Arguments[i].TryConvertTo(PT.GetElementType(), out Value))
							{
								this.methodArgumentExtensions[i] = false;
								this.methodArguments[i] = Value;
							}
							else if (Arguments[i].TryConvertTo(PT, out Value))
							{
								this.methodArgumentExtensions[i] = false;
								this.methodArguments[i] = Value;
							}
							else
							{
								if (Arguments[i].IsScalar || Variables is null)
									break;

								this.methodArgumentExtensions[i] = true;
								this.methodArguments[i] = null;
								DoExtend = true;
							}
						}

						if (i < this.methodParametersTypes.Length)
						{
							this.method = null;
							this.methodType = MethodType.Method;
							this.methods = null;
							this.byReference = null;
						}
					}
				}

				if (this.method is null)
				{
					if (this.methods is null)
						this.methods = this.GetMethods(T);

					ChunkedList<KeyValuePair<string, int>> ByRef = null;
					ParameterValues = null;
					Extend = null;

					foreach (MethodRec Rec in this.methods)
					{
						DoExtend = false;

						if (Instance is null)
						{
							if (!Rec.Method.IsStatic)
								continue;
						}
						else
						{
							if (Rec.Method.IsStatic)
								continue;
						}

						if (Rec.MethodType == MethodType.Method)
						{
							for (i = 0; i < this.nrParameters; i++)
							{
								PT = Rec.Parameters[i].ParameterType;
								IElement Argument = Arguments[i];

								if (PT.IsByRef && ((Value = Argument.AssociatedObjectValue) is null ||
									Argument.TryConvertTo(PT.GetElementType(), out Value)))
								{
									if (ParameterValues is null)
									{
										Extend = new bool[this.nrParameters];
										ParameterValues = new object[this.nrParameters];
									}

									Extend[i] = false;
									ParameterValues[i] = Value;

									if (ByRef is null)
										ByRef = new ChunkedList<KeyValuePair<string, int>>();

									if (this.parameters[i] is VariableReference Ref)
										ByRef.Add(new KeyValuePair<string, int>(Ref.VariableName, i));
									else
										ByRef.Add(new KeyValuePair<string, int>(null, i));
								}
								else if (Argument.TryConvertTo(PT, out Value))
								{
									if (ParameterValues is null)
									{
										Extend = new bool[this.nrParameters];
										ParameterValues = new object[this.nrParameters];
									}

									Extend[i] = false;
									ParameterValues[i] = Value;
								}
								else
								{
									if (Argument.IsScalar || Variables is null)
										break;

									if (Extend is null)
									{
										Extend = new bool[this.nrParameters];
										ParameterValues = new object[this.nrParameters];
									}

									Extend[i] = true;
									ParameterValues[i] = null;
									DoExtend = true;
								}
							}

							if (i < this.nrParameters)
							{
								ByRef?.Clear();
								continue;
							}
						}

						this.method = Rec.Method;
						this.methodType = Rec.MethodType;
						this.methodParametersTypes = Rec.Parameters;
						this.methodArguments = ParameterValues;
						this.methodArgumentExtensions = Extend;

						if (!(ByRef is null) && ByRef.Count > 0)
							this.byReference = ByRef.ToArray();
						else
							this.byReference = null;

						break;
					}

					if (this.method is null)
						return null;
				}
			}

			if (DoExtend)
			{
				if (!(this.byReference is null))
					throw new ScriptException("Canonical extensions of method calls having reference type arguments not supported.");   // TODO

				return await this.EvaluateCanonicalAsync(Instance, this.method, this.methodType, this.methodParametersTypes, 
					Arguments, this.methodArguments, this.methodArgumentExtensions, Variables);
			}
			else
			{
				Value = await this.EvaluateAsync(Instance, this.method, this.methodType, Arguments, this.methodArguments, Variables);
				return Expression.Encapsulate(Value);
			}
		}

		private async Task<IElement> EvaluateAsync(object Instance, MethodInfo Method, MethodType MethodType,
			IElement[] Arguments, object[] ArgumentValues, Variables Variables)
		{
			object Value;

			switch (MethodType)
			{
				case MethodType.Method:
				default:
					Value = Method.Invoke(Instance, ArgumentValues);
					Value = await WaitPossibleTask(Value);

					if (!(this.byReference is null))
					{
						int i, j, c = this.byReference.Length;
						string s;

						for (i = 0; i < c; i++)
						{
							j = this.byReference[i].Value;
							if (string.IsNullOrEmpty(s = this.byReference[i].Key))
								Operators.PatternMatch.Match(this.parameters[j], Expression.Encapsulate(this.methodArguments[j]), Variables, this);
							else
								Variables[s] = this.methodArguments[j];
						}
					}

					break;

				case MethodType.LambdaProperty:
					Value = Method.Invoke(Instance, Types.NoParameters);
					Value = await WaitPossibleTask(Value);

					if (!(Value is ILambdaExpression LambdaExpression))
						throw new ScriptRuntimeException("Lambda expression property expected.", this);

					Value = await LambdaExpression.EvaluateAsync(Arguments, Variables);
					break;

				case MethodType.LambdaIndexProperty:
					object[] Index;

					if (this.methodParametersTypes[0].ParameterType == typeof(string))
						Index = new object[] { this.name };
					else
						Index = new object[] { Expression.ConvertTo(this.name, this.methodParametersTypes[0].ParameterType, this) };

					Value = Method.Invoke(Instance, Index);

					Value = await WaitPossibleTask(Value);

					LambdaExpression = Value as ILambdaExpression;
					if (LambdaExpression is null)
						throw new ScriptRuntimeException("Lambda expression property expected.", this);

					Value = await LambdaExpression.EvaluateAsync(Arguments, Variables);
					break;
			}

			return Expression.Encapsulate(Value);
		}

		private async Task<IElement> EvaluateCanonicalAsync(object Object, MethodInfo Method, MethodType MethodType,
			ParameterInfo[] ParametersTypes, IElement[] Arguments, object[] ArgumentValues, bool[] Extend, Variables Variables)
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
						if (First is null)
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

			if (First is null)
			{
				object Value = await this.EvaluateAsync(Object, Method, MethodType, Arguments, ArgumentValues, Variables);
				return Expression.Encapsulate(Value);
			}

			ChunkedList<IElement> Elements = new ChunkedList<IElement>();
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

				Elements.Add(await this.EvaluateCanonicalAsync(Object, Method, MethodType, ParametersTypes, Arguments, 
					ArgumentValues, Extend, Variables));
			}

			for (i = 0; i < this.nrParameters; i++)
			{
				if (Extend[i])
					Enumerators[i].Dispose();
			}

			return First.Encapsulate(Elements, this);
		}

		private MethodRec[] GetMethods(Type Type)
		{
			ChunkedList<MethodRec> Result = new ChunkedList<MethodRec>();
			ParameterInfo[] ParameterInfo;
			IEnumerable<MethodInfo> Methods = Type.GetRuntimeMethods();

			foreach (MethodInfo MI in Methods)
			{
				if (MI.IsAbstract || !MI.IsPublic || MI.Name != this.name)
					continue;

				ParameterInfo = MI.GetParameters();
				if (ParameterInfo.Length != this.nrParameters)
					continue;

				Result.Add(new MethodRec()
				{
					Method = MI,
					Parameters = ParameterInfo,
					MethodType = MethodType.Method
				});
			}

			if (Result.Count == 0)
			{
				PropertyInfo PI = Type.GetRuntimeProperty(this.name);
				if (!(PI is null) && PI.GetIndexParameters().Length == 0)
				{
					if (!PI.CanRead)
						throw new ScriptRuntimeException("Property cannot be read: " + this.name, this);
					else if (!PI.GetMethod.IsPublic)
						throw new ScriptRuntimeException("Property not accessible: " + this.name, this);
					else
					{
						Result.Add(new MethodRec()
						{
							Method = PI.GetMethod,
							Parameters = Array.Empty<ParameterInfo>(),
							MethodType = MethodType.LambdaProperty
						});
					}
				}
				else if (VectorIndex.TryGetIndexProperty(Type, true, false, out PI, out ParameterInfo[] Parameters))
				{
					Result.Add(new MethodRec()
					{
						Method = PI.GetMethod,
						Parameters = Parameters,
						MethodType = MethodType.LambdaIndexProperty
					});
				}
			}

			return Result.ToArray();
		}

		private enum MethodType
		{
			Method,
			LambdaProperty,
			LambdaIndexProperty
		}

		private class MethodRec
		{
			public MethodInfo Method;
			public ParameterInfo[] Parameters;
			public MethodType MethodType;
		}

		private Type lastType = null;
		private MethodInfo method = null;
		private MethodType methodType = MethodType.Method;
		private ParameterInfo[] methodParametersTypes = null;
		private MethodRec[] methods = null;
		private KeyValuePair<string, int>[] byReference = null;
		private object[] methodArguments = null;
		private bool[] methodArgumentExtensions = null;
		private readonly object synchObject = new object();

		/// <summary>
		/// Calls the callback method for all child nodes.
		/// </summary>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public override bool ForAllChildNodes(ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			int i;

			if (Order == SearchMethod.DepthFirst)
			{
				if (!this.parameters.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			ScriptNode Node;

			for (i = 0; i < this.nrParameters; i++)
			{
				Node = this.parameters[i];
				if (!(Node is null))
				{
					bool b = !Callback(Node, out ScriptNode NewNode, State);
					if (!(NewNode is null))
					{
						this.parameters[i] = Node = NewNode;
						NewNode.SetParent(this);
					}

					if (b || (Order == SearchMethod.TreeOrder && !Node.ForAllChildNodes(Callback, State, Order)))
						return false;
				}
			}

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.parameters.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is NamedMethodCall O &&
				this.name == O.name &&
				AreEqual(this.parameters, O.parameters) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.name);
			Result ^= Result << 5 ^ GetHashCode(this.parameters);
			return Result;
		}
	}
}
