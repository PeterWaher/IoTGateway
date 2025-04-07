using System;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
	/// evaluate to types corresponding to the concretization that is to be created.
	/// </summary>
	public class CreateType : Function
	{
		private ScriptNode type;
		private readonly ScriptNode[] parameters;
		private readonly int nrParameters;
		private bool isAsync;

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Parameters">Constructor parameters.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode[] Parameters, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.type = Type;
			this.type?.SetParent(this);

			this.parameters = Parameters;
			this.parameters?.SetParent(this);

			this.nrParameters = Parameters.Length;

			this.CalcIsAsync();
		}

		private void CalcIsAsync()
		{
			this.isAsync = this.type?.IsAsynchronous ?? false;

			if (!this.isAsync)
			{
				for (int i = 0; i < this.nrParameters; i++)
				{
					if (this.parameters[i]?.IsAsynchronous ?? false)
					{
						this.isAsync = true;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, int Start, int Length, Expression Expression)
			: this(Type, Array.Empty<ScriptNode>(), Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Argument4">Constructor argument 4.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
			int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Argument4">Constructor argument 4.</param>
		/// <param name="Argument5">Constructor argument 5.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
			ScriptNode Argument5, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Argument4">Constructor argument 4.</param>
		/// <param name="Argument5">Constructor argument 5.</param>
		/// <param name="Argument6">Constructor argument 6.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
			ScriptNode Argument5, ScriptNode Argument6, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Argument4">Constructor argument 4.</param>
		/// <param name="Argument5">Constructor argument 5.</param>
		/// <param name="Argument6">Constructor argument 6.</param>
		/// <param name="Argument7">Constructor argument 7.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
			ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7 }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Argument4">Constructor argument 4.</param>
		/// <param name="Argument5">Constructor argument 5.</param>
		/// <param name="Argument6">Constructor argument 6.</param>
		/// <param name="Argument7">Constructor argument 7.</param>
		/// <param name="Argument8">Constructor argument 8.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
			ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, ScriptNode Argument8, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7, Argument8 },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Argument4">Constructor argument 4.</param>
		/// <param name="Argument5">Constructor argument 5.</param>
		/// <param name="Argument6">Constructor argument 6.</param>
		/// <param name="Argument7">Constructor argument 7.</param>
		/// <param name="Argument8">Constructor argument 8.</param>
		/// <param name="Argument9">Constructor argument 9.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
			ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, ScriptNode Argument8, ScriptNode Argument9,
			int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7, Argument8, Argument9 },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a concrete type from a generic type. The first argument must evaluate to the generic type, and following arguments must
		/// evaluate to types corresponding to the concretization that is to be created.
		/// </summary>
		/// <param name="Type">Type.</param>
		/// <param name="Argument1">Constructor argument 1.</param>
		/// <param name="Argument2">Constructor argument 2.</param>
		/// <param name="Argument3">Constructor argument 3.</param>
		/// <param name="Argument4">Constructor argument 4.</param>
		/// <param name="Argument5">Constructor argument 5.</param>
		/// <param name="Argument6">Constructor argument 6.</param>
		/// <param name="Argument7">Constructor argument 7.</param>
		/// <param name="Argument8">Constructor argument 8.</param>
		/// <param name="Argument9">Constructor argument 9.</param>
		/// <param name="Argument10">Constructor argument 10.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateType(ScriptNode Type, ScriptNode Argument1, ScriptNode Argument2, ScriptNode Argument3, ScriptNode Argument4,
			ScriptNode Argument5, ScriptNode Argument6, ScriptNode Argument7, ScriptNode Argument8, ScriptNode Argument9,
			ScriptNode Argument10, int Start, int Length, Expression Expression)
			: this(Type, new ScriptNode[] { Argument1, Argument2, Argument3, Argument4, Argument5, Argument6, Argument7, Argument8, Argument9, Argument10 },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(CreateType);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "GenType", "Type1" };

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
			IElement E = this.type.Evaluate(Variables);
			if (!(E is TypeValue TV))
				throw new ScriptRuntimeException("First argument must evaluate to the type to be created.", this);

			Type GenericType = TV.Value;
			TypeInfo TI = GenericType.GetTypeInfo();
			if (!TI.ContainsGenericParameters)
				throw new ScriptRuntimeException("First argument must evaluate to a generic type.", this);

			Type[] GenericTypeParameters = TI.GenericTypeParameters;
			int i;

			if (GenericTypeParameters.Length != this.nrParameters)
				throw new ScriptRuntimeException("Generic type contains " + GenericTypeParameters.Length + " parameter(s).", this);

			Type[] TypeArguments = new Type[this.nrParameters];
			IElement Argument;

			for (i = 0; i < this.nrParameters; i++)
			{
				Argument = this.parameters[i].Evaluate(Variables);
				if (!(Argument.AssociatedObjectValue is Type T))
					throw new ScriptRuntimeException("Expected type-valued argument.", this.parameters[i]);

				TypeArguments[i] = T;
			}

			Type ConcreteType = GenericType.MakeGenericType(TypeArguments);

			return new TypeValue(ConcreteType);
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			if (!this.isAsync)
				return this.Evaluate(Variables);

			IElement E = await this.type.EvaluateAsync(Variables);
			if (!(E is TypeValue TV))
				throw new ScriptRuntimeException("First argument must evaluate to the type to be created.", this);

			Type GenericType = TV.Value;
			TypeInfo TI = GenericType.GetTypeInfo();
			if (!TI.ContainsGenericParameters)
				throw new ScriptRuntimeException("First argument must evaluate to a generic type.", this);

			Type[] GenericTypeParameters = TI.GenericTypeParameters;
			int i;

			if (GenericTypeParameters.Length != this.nrParameters)
				throw new ScriptRuntimeException("Generic type contains " + GenericTypeParameters.Length + " parameter(s).", this);

			Type[] TypeArguments = new Type[this.nrParameters];
			IElement Argument;

			for (i = 0; i < this.nrParameters; i++)
			{
				Argument = await this.parameters[i].EvaluateAsync(Variables);
				if (!(Argument.AssociatedObjectValue is Type T))
					throw new ScriptRuntimeException("Expected type-valued argument.", this.parameters[i]);

				TypeArguments[i] = T;
			}

			Type ConcreteType = GenericType.MakeGenericType(TypeArguments);

			return new TypeValue(ConcreteType);
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
				if (!(this.type?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!this.parameters.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			ScriptNode Node;
			ScriptNode NewNode;
			bool RecalcIsAsync = false;
			bool b;

			if (!(this.type is null))
			{
				b = !Callback(this.type, out NewNode, State);
				if (!(NewNode is null))
				{
					this.type = NewNode;
					NewNode.SetParent(this);

					RecalcIsAsync = true;
				}

				if (b || (Order == SearchMethod.TreeOrder && !this.type.ForAllChildNodes(Callback, State, Order)))
				{
					if (RecalcIsAsync)
						this.CalcIsAsync();

					return false;
				}
			}

			for (i = 0; i < this.nrParameters; i++)
			{
				Node = this.parameters[i];
				if (!(Node is null))
				{
					b = !Callback(Node, out NewNode, State);
					if (!(NewNode is null))
					{
						this.parameters[i] = Node = NewNode;
						NewNode.SetParent(this);

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
				if (!(this.type?.ForAllChildNodes(Callback, State, Order) ?? true))
					return false;

				if (!this.parameters.ForAllChildNodes(Callback, State, Order))
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is CreateType O &&
				this.type.Equals(O.type) &&
				AreEqual(this.parameters, O.parameters) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.type.GetHashCode();
			Result ^= Result << 5 ^ GetHashCode(this.parameters);
			return Result;
		}
	}
}
