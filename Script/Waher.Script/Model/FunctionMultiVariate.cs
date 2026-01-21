using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Base class for multivariate funcions.
	/// </summary>
	public abstract class FunctionMultiVariate : Function
	{
		/// <summary>
		/// Zero parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes0 = Array.Empty<ArgumentType>();

		/// <summary>
		/// One scalar parameter.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes1Normal = new ArgumentType[] { ArgumentType.Normal };

		/// <summary>
		/// One vector parameter.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes1Vector = new ArgumentType[] { ArgumentType.Vector };

		/// <summary>
		/// One matrix parameter.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes1Matrix = new ArgumentType[] { ArgumentType.Matrix };

		/// <summary>
		/// One set parameter.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes1Set = new ArgumentType[] { ArgumentType.Set };

		/// <summary>
		/// Two normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes2Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// Three normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes3Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// Four normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes4Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// Five normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes5Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// Six normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes6Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// Seven normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes7Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// Eight normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes8Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// Nine normal parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes9Normal = new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal };

		/// <summary>
		/// One scalar parameter.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes1Scalar = new ArgumentType[] { ArgumentType.Scalar };

		/// <summary>
		/// Two scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes2Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Three scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes3Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Four scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes4Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Five scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes5Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Six scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes6Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Seven scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes7Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Eight scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes8Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };

		/// <summary>
		/// Nine scalar parameters.
		/// </summary>
		protected static readonly ArgumentType[] argumentTypes9Scalar = new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar };


		private readonly ScriptNode[] arguments;
		private readonly ArgumentType[] argumentTypes;
		private readonly bool allNormal;
		private readonly int nrArguments;
		private bool isAsync;

		/// <summary>
		/// Base class for funcions of one variable.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="ArgumentTypes">Argument Types.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public FunctionMultiVariate(ScriptNode[] Arguments, ArgumentType[] ArgumentTypes, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			if (Arguments.Length != ArgumentTypes.Length)
				throw new ArgumentException("Size of ArgumentTypes must match the size of Arguments.", nameof(ArgumentTypes));

			this.arguments = Arguments;
			this.arguments?.SetParent(this);

			this.argumentTypes = ArgumentTypes;
			this.nrArguments = this.arguments.Length;

			this.allNormal = true;
			foreach (ArgumentType Type in this.argumentTypes)
			{
				if (Type != ArgumentType.Normal)
				{
					this.allNormal = false;
					break;
				}
			}

			this.CalcIsAsync();
		}

		private void CalcIsAsync()
		{
			this.isAsync = false;

			for (int i = 0; i < this.nrArguments; i++)
			{
				if (this.arguments[i]?.IsAsynchronous ?? false)
				{
					this.isAsync = true;
					break;
				}
			}
		}

		/// <summary>
		/// Function arguments.
		/// </summary>
		public ScriptNode[] Arguments => this.arguments;

		/// <summary>
		/// Function argument types.
		/// </summary>
		public ArgumentType[] ArgumentTypes => this.argumentTypes;

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => this.isAsync;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement[] Arg = new IElement[this.nrArguments];
			ScriptNode Node;
			int i;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (Node is null)
					Arg[i] = ObjectValue.Null;
				else
					Arg[i] = Node.Evaluate(Variables);
			}

			if (this.allNormal)
				return this.Evaluate(Arg, Variables);
			else
				return this.EvaluateCanonicalExtension(Arg, Variables);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.IsAsynchronous)
				return this.Evaluate(Variables);

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

			if (this.allNormal)
				return await this.EvaluateAsync(Arg, Variables);
			else
				return await this.EvaluateCanonicalExtensionAsync(Arg, Variables);
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
				return this.Evaluate(Arguments, Variables);
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
								throw new ArgumentDimensionsInconsistentScriptException(this);

							e[i] = ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Argument.Encapsulate;
						}
						break;

					case ArgumentType.Vector:
						if (Argument is IVector)
							e[i] = null;
						else if (!((M = Argument as IMatrix) is null))
						{
							if (Dimension < 0)
								Dimension = M.Rows;
							else if (M.Rows != Dimension)
								throw new ArgumentDimensionsInconsistentScriptException(this);

							ChunkedList<IElement> Vectors = new ChunkedList<IElement>();

							for (j = 0; j < Dimension; j++)
								Vectors.Add(M.GetRow(j));

							e[i] = Vectors.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Operators.LambdaDefinition.EncapsulateToVector;
						}
						else if (!((S = Argument as ISet) is null))
						{
							int? Size = S.Size;
							if (!Size.HasValue)
								throw new ArgumentDimensionsInconsistentScriptException(this);

							if (Dimension < 0)
								Dimension = Size.Value;
							else if (Size.Value != Dimension)
								throw new ArgumentDimensionsInconsistentScriptException(this);

							e[i] = S.ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Argument.Encapsulate;
						}
						else if (Argument.AssociatedObjectValue is IEnumerable Enumerable &&
							!(Argument.AssociatedObjectValue is string) &&
							!(Argument.AssociatedObjectValue is IDictionary<string, IElement>) &&
							!(Argument.AssociatedObjectValue is IDictionary<string, object>))
						{
							Arguments[i] = Operators.Vectors.VectorDefinition.Encapsulate(Enumerable, false, this);
							e[i] = null;
						}
						else
						{
							Arguments[i] = Operators.Vectors.VectorDefinition.Encapsulate(new IElement[] { Argument }, false, this);
							e[i] = null;
						}
						break;

					case ArgumentType.Set:
						if (Argument is ISet)
							e[i] = null;
						else if (!((V = Argument as IVectorSpaceElement) is null))
						{
							Arguments[i] = Operators.Sets.SetDefinition.Encapsulate(V.ChildElements);
							e[i] = null;
						}
						else if (!((M = Argument as IMatrix) is null))
						{
							if (Dimension < 0)
								Dimension = M.Rows;
							else if (M.Rows != Dimension)
								throw new ArgumentDimensionsInconsistentScriptException(this);

							ChunkedList<IElement> Vectors = new ChunkedList<IElement>();

							for (j = 0; j < Dimension; j++)
								Vectors.Add(M.GetRow(j));

							Arguments[i] = Argument = Operators.Sets.SetDefinition.Encapsulate(Vectors);
							ChildElements = Argument.ChildElements;

							e[i] = ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Operators.LambdaDefinition.EncapsulateToVector;
						}
						else if (Argument.AssociatedObjectValue is IEnumerable Enumerable &&
							!(Argument.AssociatedObjectValue is string) &&
							!(Argument.AssociatedObjectValue is IDictionary<string, IElement>) &&
							!(Argument.AssociatedObjectValue is IDictionary<string, object>))
						{
							Arguments[i] = Operators.Sets.SetDefinition.Encapsulate(Enumerable);
							e[i] = null;
						}
						else
						{
							Arguments[i] = Operators.Sets.SetDefinition.Encapsulate(new IElement[] { Argument });
							e[i] = null;
						}
						break;

					case ArgumentType.Matrix:
						if (Argument is IMatrix)
							e[i] = null;
						else if (!((V = Argument as IVectorSpaceElement) is null))
						{
							Arguments[i] = Operators.Matrices.MatrixDefinition.Encapsulate(V.ChildElements, 1, V.Dimension, this);
							e[i] = null;
						}
						else if (!((S = Argument as ISet) is null))
						{
							int? Size = S.Size;
							if (!Size.HasValue)
								throw new ArgumentDimensionsInconsistentScriptException(this);

							if (Dimension < 0)
								Dimension = Size.Value;
							else if (Size.Value != Dimension)
								throw new ArgumentDimensionsInconsistentScriptException(this);

							e[i] = S.ChildElements.GetEnumerator();
							if (Encapsulation is null)
								Encapsulation = Argument.Encapsulate;
						}
						else
						{
							Arguments[i] = Operators.Matrices.MatrixDefinition.Encapsulate(new IElement[] { Argument }, 1, 1, this);
							e[i] = null;
						}
						break;

					default:
						throw new ScriptRuntimeException("Unhandled argument type.", this);
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
				return await this.EvaluateAsync(Arguments, Variables);
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public abstract IElement Evaluate(IElement[] Arguments, Variables Variables);

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public virtual Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			return Task.FromResult(this.Evaluate(Arguments, Variables));
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
			int i;

			if (Order == SearchMethod.DepthFirst)
			{
				if (!this.arguments.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			ScriptNode Node;
			bool RecalcIsAsync = false;

			for (i = 0; i < this.nrArguments; i++)
			{
				Node = this.arguments[i];
				if (!(Node is null))
				{
					bool b = !Callback(Node, out ScriptNode NewNode, State);
					if (!(NewNode is null))
					{
						this.arguments[i] = NewNode;
						NewNode.SetParent(this);
						Node = NewNode;

						RecalcIsAsync = true;
					}

					if (b || (Order == SearchMethod.TreeOrder && !Node.ForAllChildNodes(Callback, State, Order)))
					{
						if (RecalcIsAsync)
							this.CalcIsAsync();

						return false;
					}
				}
			}

			if (RecalcIsAsync)
				this.CalcIsAsync();

			if (Order == SearchMethod.BreadthFirst)
			{
				if (!this.arguments.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is FunctionMultiVariate O &&
				AreEqual(this.arguments, O.arguments) &&
				AreEqual(this.argumentTypes, O.argumentTypes) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.arguments);
			Result ^= Result << 5 ^ GetHashCode(this.argumentTypes);
			return Result;
		}

	}
}
