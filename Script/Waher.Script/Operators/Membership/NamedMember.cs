using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Constants;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Runtime;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Operators.Membership
{
	/// <summary>
	/// Named member operator
	/// </summary>
	public class NamedMember : NullCheckUnaryOperator
	{
		private readonly string name;

		/// <summary>
		/// Named member operator
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name</param>
		/// <param name="NullCheck">If null should be returned if operand is null.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public NamedMember(ScriptNode Operand, string Name, bool NullCheck, int Start, int Length, Expression Expression)
			: base(Operand, NullCheck, Start, Length, Expression)
		{
			this.name = Name;
			this.isAsync = true;
		}

		/// <summary>
		/// Name of method.
		/// </summary>
		public string Name => this.name;

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
			object Value = Operand.AssociatedObjectValue;
			object Instance;

			if (Value is null && this.nullCheck)
				return ObjectValue.Null;

			Type T;

			T = Value as Type;
			if (T is null)
			{
				Instance = Value;
				T = Value?.GetType() ?? typeof(object);
			}
			else
				Instance = null;

			await this.synchObject.WaitAsync();
			try
			{
				if (T != this.type)
				{
					this.type = T;
					this.field = null;
					this._event = null;
					this.methods = null;
					this.nameIndex = null;
					this.property = T.GetRuntimeProperty(this.name);

					if (this.property is null)
					{
						this.field = T.GetRuntimeField(this.name);
						if (this.field is null)
						{
							this._event = T.GetRuntimeEvent(this.name);
							if (this._event is null)
							{
								ChunkedList<MethodLambda> Methods = null;

								foreach (MethodInfo MI in T.GetRuntimeMethods())
								{
									if (!MI.IsAbstract && MI.IsPublic && MI.Name == this.name)
									{
										if (Methods is null)
											Methods = new ChunkedList<MethodLambda>();

										Methods.Add(new MethodLambda(Instance, MI));
									}
								}

								this.methods = Methods?.ToArray();
								if (this.methods is null)
								{
									if (VectorIndex.TryGetIndexProperty(T, true, false, out this.property,
										out ParameterInfo[] IndexArguments) &&
										(IndexArguments?.Length ?? 0) == 1)
									{
										if (IndexArguments[0].ParameterType == typeof(string))
											this.nameIndex = new object[] { this.name };
										else
											this.nameIndex = new object[] { Expression.ConvertTo(this.name, IndexArguments[0].ParameterType, this) };
									}
								}
							}
							else
							{
								if (!this._event.AddMethod.IsPublic)
									throw new ScriptRuntimeException("Event not accessible: " + this.name, this);
							}
						}
						else
						{
							if (!this.field.IsPublic)
								throw new ScriptRuntimeException("Field not accessible: " + this.name, this);
						}
					}
					else
					{
						if (!this.property.CanRead)
							throw new ScriptRuntimeException("Property cannot be read: " + this.name, this);
						else if (!this.property.GetMethod.IsPublic)
							throw new ScriptRuntimeException("Property not accessible: " + this.name, this);
					}
				}

				object Result = null;

				if (!(this.property is null))
				{
					try
					{
						Result = await WaitPossibleTask(this.property.GetValue(Instance, this.nameIndex));
					}
					catch (Exception ex)
					{
						if (Instance is null)
							Result = this.property;
						else
							ExceptionDispatchInfo.Capture(ex).Throw();
					}

					return Expression.Encapsulate(Result);
				}
				else if (!(this.field is null))
				{
					try
					{
						Result = await WaitPossibleTask(this.field.GetValue(Instance));
					}
					catch (Exception ex)
					{
						if (Instance is null)
							Result = this.field;
						else
							ExceptionDispatchInfo.Capture(ex).Throw();
					}

					return Expression.Encapsulate(Result);
				}
				else if (!(this._event is null))
					return Expression.Encapsulate(this._event);
				else if (!(this.methods is null))
				{
					if (this.methods.Length == 1)
						return Expression.Encapsulate(this.methods[0]);
					else
						return Expression.Encapsulate(this.methods);
				}
				else if (Operand.IsScalar)
					throw new ScriptRuntimeException("Member '" + this.name + "' not found on type '" + T.FullName + "'.", this);
			}
			finally
			{
				this.synchObject.Release();
			}

			ChunkedList<IElement> Elements = new ChunkedList<IElement>();

			foreach (IElement E in Operand.ChildElements)
				Elements.Add(await EvaluateDynamic(E, this.name, this.nullCheck, this));

			return Operand.Encapsulate(Elements, this);
		}

		private Type type = null;
		private PropertyInfo property = null;
		private FieldInfo field = null;
		private EventInfo _event = null;
		private MethodLambda[] methods = null;
		private object[] nameIndex = null;
		private readonly SemaphoreSlim synchObject = new SemaphoreSlim(1);

		internal static readonly Type[] stringType = new Type[] { typeof(string) };

		/// <summary>
		/// Evaluates the member operator dynamically on an operand.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name of member.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Result.</returns>
		public static async Task<IElement> EvaluateDynamic(IElement Operand, string Name, bool NullCheck, ScriptNode Node)
		{
			object Value = Operand.AssociatedObjectValue;
			object Instance;
			Type T;

			if (Value is null && NullCheck)
				return ObjectValue.Null;

			T = Value as Type;
			if (T is null)
			{
				Instance = Value;
				T = Value.GetType();
			}
			else
				Instance = null;

			PropertyInfo Property = T.GetRuntimeProperty(Name);
			if (!(Property is null))
			{
				if (!Property.CanRead)
					throw new ScriptRuntimeException("Property cannot be read: " + Name, Node);
				else if (!Property.GetMethod.IsPublic)
					throw new ScriptRuntimeException("Property not accessible: " + Name, Node);
				else
					return Expression.Encapsulate(await WaitPossibleTask(Property.GetValue(Instance, null)));
			}

			FieldInfo Field = T.GetRuntimeField(Name);
			if (!(Field is null))
			{
				if (!Field.IsPublic)
					throw new ScriptRuntimeException("Field not accessible: " + Name, Node);
				else
					return Expression.Encapsulate(await WaitPossibleTask(Field.GetValue(Instance)));
			}

			EventInfo Event = T.GetRuntimeEvent(Name);
			if (!(Event is null))
				return Expression.Encapsulate(Event);

			ChunkedList<MethodLambda> Methods = null;

			foreach (MethodInfo MI in T.GetRuntimeMethods())
			{
				if (!MI.IsAbstract && MI.IsPublic && MI.Name == Name)
				{
					if (Methods is null)
						Methods = new ChunkedList<MethodLambda>();

					Methods.Add(new MethodLambda(Instance, MI));
				}
			}

			if (!(Methods is null))
			{
				if (Methods.Count == 1)
					return Expression.Encapsulate(Methods[0]);
				else
					return Expression.Encapsulate(Methods.ToArray());
			}

			if (VectorIndex.TryGetIndexProperty(T, true, false, out Property,
				out ParameterInfo[] IndexArguments) &&
				(IndexArguments?.Length ?? 0) == 1)
			{
				object[] Index;

				if (IndexArguments[0].ParameterType == typeof(string))
					Index = new object[] { Name };
				else
					Index = new object[] { Expression.ConvertTo(Name, IndexArguments[0].ParameterType, Node) };

				return Expression.Encapsulate(await WaitPossibleTask(Property.GetValue(Instance, Index)));
			}

			if (Operand.IsScalar)
				throw new ScriptRuntimeException("Member '" + Name + "' not found on type '" + T.FullName + "'.", Node);

			ChunkedList<IElement> Elements = new ChunkedList<IElement>();

			foreach (IElement E in Operand.ChildElements)
				Elements.Add(await EvaluateDynamic(E, Name, NullCheck, Node));

			return Operand.Encapsulate(Elements, Node);
		}
	}
}
