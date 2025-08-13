﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.Sets;
using Waher.Script.Operators.Matrices;
using Waher.Script.Operators.Sets;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Lambda Definition.
	/// </summary>
	public class LambdaDefinition : UnaryOperator, IElement, ILambdaExpression, IDifferentiable
	{
		private readonly string[] argumentNames;
		private readonly ArgumentType[] argumentTypes;
		private readonly int nrArguments;
		private readonly bool allNormal;

		/// <summary>
		/// Lambda Definition.
		/// </summary>
		/// <param name="ArgumentNames">Argument Names.</param>
		/// <param name="ArgumentTypes">Argument Types.</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LambdaDefinition(string[] ArgumentNames, ArgumentType[] ArgumentTypes, ScriptNode Operand, int Start, int Length, Expression Expression)
			: base(Operand, Start, Length, Expression)
		{
			this.argumentNames = ArgumentNames;
			this.argumentTypes = ArgumentTypes;
			this.nrArguments = ArgumentNames.Length;

			this.allNormal = true;
			foreach (ArgumentType Type in this.argumentTypes)
			{
				if (Type != ArgumentType.Normal)
				{
					this.allNormal = false;
					break;
				}
			}
		}

		/// <summary>
		/// Number of arguments.
		/// </summary>
		public int NrArguments => this.nrArguments;

		/// <summary>
		/// Argument Names.
		/// </summary>
		public string[] ArgumentNames => this.argumentNames;

		/// <summary>
		/// Argument types.
		/// </summary>
		public ArgumentType[] ArgumentTypes => this.argumentTypes;

		/// <summary>
		/// Associated object value.
		/// </summary>
		public object AssociatedObjectValue => this;

		/// <summary>
		/// Associated Set.
		/// </summary>
		public ISet AssociatedSet => SetOfFunctions.Instance;

		/// <summary>
		/// An enumeration of child elements. If the element is a scalar, this property will return null.
		/// </summary>
		public ICollection<IElement> ChildElements => null;

		/// <summary>
		/// If the element represents a scalar value.
		/// </summary>
		public bool IsScalar => true;

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public IElement Encapsulate(ChunkedList<IElement> Elements, ScriptNode Node)
		{
			return null;
		}

		/// <summary>
		/// Encapsulates a set of elements into a similar structure as that provided by the current element.
		/// </summary>
		/// <param name="Elements">New set of child elements, not necessarily of the same type as the child elements of the current object.</param>
		/// <param name="Node">Script node from where the encapsulation is done.</param>
		/// <returns>Encapsulated object of similar type as the current object.</returns>
		public IElement Encapsulate(ICollection<IElement> Elements, ScriptNode Node)
		{
			return null;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Task<IElement> EvaluateAsync(Variables Variables)
		{
			return Task.FromResult<IElement>(this);
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override IElement Evaluate(IElement Operand, Variables Variables)
		{
			return this;
		}

		/// <summary>
		/// Evaluates the operator.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result</returns>
		public override Task<IElement> EvaluateAsync(IElement Operand, Variables Variables)
		{
			return Task.FromResult<IElement>(this);
		}

		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (Arguments.Length != this.nrArguments)
				throw new ScriptRuntimeException("Expected " + this.nrArguments.ToString() + " arguments.", this);

			Variables.Push();
			try
			{
				if (this.allNormal)
				{
					int i;

					for (i = 0; i < this.nrArguments; i++)
						Variables[this.argumentNames[i]] = Arguments[i];

					try
					{
						return this.op.Evaluate(Variables);
					}
					catch (ScriptReturnValueException ex)
					{
						return ex.ReturnValue;
						//IElement ReturnValue = ex.ReturnValue;
						//ScriptReturnValueException.Reuse(ex);
						//return ReturnValue;
					}
					catch (ScriptBreakLoopException ex)
					{
						return ex.LoopValue ?? ObjectValue.Null;
						//ScriptBreakLoopException.Reuse(ex);
					}
					catch (ScriptContinueLoopException ex)
					{
						return ex.LoopValue ?? ObjectValue.Null;
						//ScriptContinueLoopException.Reuse(ex);
					}
				}
				else
					return this.EvaluateCanonicalExtension(Arguments, Variables);
			}
			finally
			{
				Variables.Pop();
			}
		}

		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			if (!this.IsAsynchronous)
				return this.Evaluate(Arguments, Variables);

			if (Arguments.Length != this.nrArguments)
				throw new ScriptRuntimeException("Expected " + this.nrArguments.ToString() + " arguments.", this);

			Variables.Push();
			try
			{
				if (this.allNormal)
				{
					int i;

					for (i = 0; i < this.nrArguments; i++)
						Variables[this.argumentNames[i]] = Arguments[i];

					try
					{
						return await this.op.EvaluateAsync(Variables);
					}
					catch (ScriptReturnValueException ex)
					{
						return ex.ReturnValue;
						//IElement Returnvalue = ex.ReturnValue;
						//ScriptReturnValueException.Reuse(ex);
						//return Returnvalue;
					}
					catch (ScriptBreakLoopException ex)
					{
						return ex.LoopValue ?? ObjectValue.Null;
						//ScriptBreakLoopException.Reuse(ex);
					}
					catch (ScriptContinueLoopException ex)
					{
						return ex.LoopValue ?? ObjectValue.Null;
						//ScriptContinueLoopException.Reuse(ex);
					}
				}
				else
					return await this.EvaluateCanonicalExtensionAsync(Arguments, Variables);
			}
			finally
			{
				Variables.Pop();
			}
		}

		private IElement EvaluateCanonicalExtension(IElement[] Arguments, Variables Variables)
		{
			int i, j;

			this.Prepare(Arguments, out Encapsulation Encapsulation, out int Dimension, out IEnumerator<IElement>[] e);

			if (!(Encapsulation is null))
			{
				ChunkedList<IElement> Result = new ChunkedList<IElement>();
				IElement[] Arguments2 = new IElement[this.nrArguments];

				for (j = 0; j < Dimension; j++)
				{
					for (i = 0; i < this.nrArguments; i++)
					{
						if (e[i] is null || !e[i].MoveNext())
							Arguments2[i] = Arguments[i];
						else
							Arguments2[i] = e[i].Current;
					}

					Result.Add(this.EvaluateCanonicalExtension(Arguments2, Variables));
				}

				return Encapsulation(Result, this);
			}
			else
			{
				for (i = 0; i < this.nrArguments; i++)
					Variables[this.argumentNames[i]] = Arguments[i];

				try
				{
					return this.op.Evaluate(Variables);
				}
				catch (ScriptReturnValueException ex)
				{
					return ex.ReturnValue;
					//IElement Returnvalue = ex.ReturnValue;
					//ScriptReturnValueException.Reuse(ex);
					//return Returnvalue;
				}
				catch (ScriptBreakLoopException ex)
				{
					return ex.LoopValue ?? ObjectValue.Null;
					//ScriptBreakLoopException.Reuse(ex);
				}
				catch (ScriptContinueLoopException ex)
				{
					return ex.LoopValue ?? ObjectValue.Null;
					//ScriptContinueLoopException.Reuse(ex);
				}
			}
		}

		private async Task<IElement> EvaluateCanonicalExtensionAsync(IElement[] Arguments, Variables Variables)
		{
			int i, j;

			this.Prepare(Arguments, out Encapsulation Encapsulation, out int Dimension, out IEnumerator<IElement>[] e);

			if (!(Encapsulation is null))
			{
				ChunkedList<IElement> Result = new ChunkedList<IElement>();
				IElement[] Arguments2 = new IElement[this.nrArguments];

				for (j = 0; j < Dimension; j++)
				{
					for (i = 0; i < this.nrArguments; i++)
					{
						if (e[i] is null || !e[i].MoveNext())
							Arguments2[i] = Arguments[i];
						else
							Arguments2[i] = e[i].Current;
					}

					Result.Add(await this.EvaluateCanonicalExtensionAsync(Arguments2, Variables));
				}

				return Encapsulation(Result, this);
			}
			else
			{
				for (i = 0; i < this.nrArguments; i++)
					Variables[this.argumentNames[i]] = Arguments[i];

				try
				{
					return await this.op.EvaluateAsync(Variables);
				}
				catch (ScriptReturnValueException ex)
				{
					return ex.ReturnValue;
					//IElement Returnvalue = ex.ReturnValue;
					//ScriptReturnValueException.Reuse(ex);
					//return Returnvalue;
				}
				catch (ScriptBreakLoopException ex)
				{
					return ex.LoopValue ?? ObjectValue.Null;
					//ScriptBreakLoopException.Reuse(ex);
				}
				catch (ScriptContinueLoopException ex)
				{
					return ex.LoopValue ?? ObjectValue.Null;
					//ScriptContinueLoopException.Reuse(ex);
				}
			}
		}

		private void Prepare(IElement[] Arguments, out Encapsulation Encapsulation, out int Dimension, out IEnumerator<IElement>[] e)
		{
			ICollection<IElement> ChildElements;
			IElement Argument;
			IMatrix M;
			ISet S;
			IVectorSpaceElement V;
			int i, j;

			e = new IEnumerator<IElement>[this.nrArguments];
			Encapsulation = null;
			Dimension = -1;

			for (i = 0; i < this.nrArguments; i++)
			{
				Argument = Arguments[i];

				switch (this.argumentTypes[i])
				{
					case ArgumentType.Normal:
						e[i] = null;
						break;

					case ArgumentType.Scalar:
						if (Argument.IsScalar)
							e[i] = null;
						else
						{
							ChildElements = Argument.ChildElements;

							if (Dimension < 0)
								Dimension = ChildElements.Count;
							else if (ChildElements.Count != Dimension)
								throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

							e[i] = ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Argument.Encapsulate;
						}
						break;

					case ArgumentType.Vector:
						if (Argument is IVectorSpaceElement)
							e[i] = null;
						else if (!((M = Argument as IMatrix) is null))
						{
							if (Dimension < 0)
								Dimension = M.Rows;
							else if (M.Rows != Dimension)
								throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

							ChunkedList<IElement> Vectors = new ChunkedList<IElement>();

							for (j = 0; j < Dimension; j++)
								Vectors.Add(M.GetRow(j));

							e[i] = Vectors.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = EncapsulateToVector;
						}
						else if (!((S = Argument as ISet) is null))
						{
							int? Size = S.Size;
							if (!Size.HasValue)
								throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

							if (Dimension < 0)
								Dimension = Size.Value;
							else if (Size.Value != Dimension)
								throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

							e[i] = S.ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Argument.Encapsulate;
						}
						else
						{
							Arguments[i] = VectorDefinition.Encapsulate(new IElement[] { Argument }, false, this);
							e[i] = null;
						}
						break;

					case ArgumentType.Set:
						if (Argument is ISet)
							e[i] = null;
						else if (!((V = Argument as IVectorSpaceElement) is null))
						{
							Arguments[i] = SetDefinition.Encapsulate(V.ChildElements);
							e[i] = null;
						}
						else if (!((M = Argument as IMatrix) is null))
						{
							if (Dimension < 0)
								Dimension = M.Rows;
							else if (M.Rows != Dimension)
								throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

							ChunkedList<IElement> Vectors = new ChunkedList<IElement>();

							for (j = 0; j < Dimension; j++)
								Vectors.Add(M.GetRow(j));

							Arguments[i] = Argument = SetDefinition.Encapsulate(Vectors);
							ChildElements = Argument.ChildElements;

							e[i] = ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = EncapsulateToVector;
						}
						else
						{
							Arguments[i] = SetDefinition.Encapsulate(new IElement[] { Argument });
							e[i] = null;
						}
						break;

					case ArgumentType.Matrix:
						if (Argument is IMatrix)
							e[i] = null;
						else if (!((V = Argument as IVectorSpaceElement) is null))
						{
							Arguments[i] = MatrixDefinition.Encapsulate(V.ChildElements, 1, V.Dimension, this);
							e[i] = null;
						}
						else if (!((S = Argument as ISet) is null))
						{
							int? Size = S.Size;
							if (!Size.HasValue)
								throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

							if (Dimension < 0)
								Dimension = Size.Value;
							else if (Size.Value != Dimension)
								throw new ScriptRuntimeException("Argument dimensions not consistent.", this);

							e[i] = S.ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Argument.Encapsulate;
						}
						else
						{
							Arguments[i] = MatrixDefinition.Encapsulate(new IElement[] { Argument }, 1, 1, this);
							e[i] = null;
						}
						break;

					default:
						throw new ScriptRuntimeException("Unhandled argument type.", this);
				}
			}
		}

		internal static IElement EncapsulateToVector(ICollection<IElement> Elements, ScriptNode Node)
		{
			return VectorDefinition.Encapsulate(Elements, true, Node);
		}

		/// <summary>
		/// Converts the value to a .NET type.
		/// </summary>
		/// <param name="DesiredType">Desired .NET type.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public bool TryConvertTo(Type DesiredType, out object Value)
		{
			if (DesiredType.IsAssignableFrom(this.GetType().GetTypeInfo()))
			{
				Value = this;
				return true;
			}

			Value = null;
			return false;
		}

		/// <summary>
		/// Differentiates a lambda expression, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated lambda expression.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (Array.IndexOf(this.argumentNames, VariableName) < 0)
				return new ConstantElement(Objects.DoubleNumber.ZeroElement, this.Start, this.Length, this.Expression);
			else if (this.op is IDifferentiable Differentiable)
				return new LambdaDefinition(this.argumentNames, this.argumentTypes, Differentiable.Differentiate(VariableName, Variables), this.Start, this.Length, this.Expression);
			else
				throw new ScriptRuntimeException("Lambda expression not differentiable.", this);
		}

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public override string DefaultVariableName
		{
			get
			{
				if (this.argumentNames.Length == 1)
					return this.argumentNames[0];
				else
					return null;
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return ToString(this);
		}

		/// <summary>
		/// Creates a displayable string for a lambda expression.
		/// </summary>
		/// <param name="Expression">Lambda expression.</param>
		/// <returns>String representation.</returns>
		public static string ToString(ILambdaExpression Expression)
		{
			string[] ArgumentNames = Expression.ArgumentNames;
			ArgumentType[] ArgumentTypes = Expression.ArgumentTypes;
			StringBuilder Result = new StringBuilder();
			int i;

			Result.Append("λ(");

			for (i = 0; i < Expression.NrArguments; i++)
			{
				if (i > 0)
					Result.Append(',');

				switch (ArgumentTypes[i])
				{
					case ArgumentType.Matrix:
						Result.Append(ArgumentNames[i]);
						Result.Append("[[]]");
						break;

					case ArgumentType.Normal:
					default:
						Result.Append(ArgumentNames[i]);
						break;

					case ArgumentType.Scalar:
						Result.Append('[');
						Result.Append(ArgumentNames[i]);
						Result.Append(']');
						break;

					case ArgumentType.Set:
						Result.Append(ArgumentNames[i]);
						Result.Append("{}");
						break;

					case ArgumentType.Vector:
						Result.Append(ArgumentNames[i]);
						Result.Append("[]");
						break;
				}
			}

			Result.Append(')');

			return Result.ToString();
		}
	}
}
