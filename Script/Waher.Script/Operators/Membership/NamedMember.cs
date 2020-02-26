using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Functions.Runtime;
using Waher.Script.Model;
using Waher.Script.Objects;

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
		}

		/// <summary>
		/// Name of method.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement Operand = this.op.Evaluate(Variables);
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

			lock (this.synchObject)
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
								List<MethodLambda> Methods = null;

								foreach (MethodInfo MI in T.GetRuntimeMethods())
								{
									if (MI.Name == Name)
									{
										if (Methods is null)
											Methods = new List<MethodLambda>();

										Methods.Add(new MethodLambda(Instance, MI));
									}
								}

								this.methods = Methods?.ToArray();
								if (this.methods is null)
								{
									this.property = T.GetRuntimeProperty("Item");
									if (!(this.property is null))
										this.nameIndex = new string[] { this.name };
								}
							}
						}
					}
				}

				object Result = null;

				if (!(this.property is null))
				{
					try
					{
						if (!(this.nameIndex is null))
							Result = this.property.GetValue(Instance, this.nameIndex);
						else
							Result = this.property.GetValue(Instance, null);
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
						Result = this.field.GetValue(Instance);
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

			LinkedList<IElement> Elements = new LinkedList<IElement>();

			foreach (IElement E in Operand.ChildElements)
				Elements.AddLast(EvaluateDynamic(E, this.name, this.nullCheck, this));

			return Operand.Encapsulate(Elements, this);
		}

		private Type type = null;
		private PropertyInfo property = null;
		private FieldInfo field = null;
		private EventInfo _event = null;
		private MethodLambda[] methods = null;
		private string[] nameIndex = null;
		private readonly object synchObject = new object();

		internal static readonly Type[] stringType = new Type[] { typeof(string) };

		/// <summary>
		/// Evaluates the member operator dynamically on an operand.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name of member.</param>
		/// <param name="NullCheck">If null should be returned if left operand is null.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Result.</returns>
		public static IElement EvaluateDynamic(IElement Operand, string Name, bool NullCheck, ScriptNode Node)
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
				return Expression.Encapsulate(Property.GetValue(Instance, null));

			FieldInfo Field = T.GetRuntimeField(Name);
			if (!(Field is null))
				return Expression.Encapsulate(Field.GetValue(Instance));

			Property = T.GetRuntimeProperty("Item");
			if (!(Property is null))
				return Expression.Encapsulate(Property.GetValue(Instance, new string[] { Name }));

			if (Operand.IsScalar)
				throw new ScriptRuntimeException("Member '" + Name + "' not found on type '" + T.FullName + "'.", Node);

			LinkedList<IElement> Elements = new LinkedList<IElement>();

			foreach (IElement E in Operand.ChildElements)
				Elements.AddLast(EvaluateDynamic(E, Name, NullCheck, Node));

			return Operand.Encapsulate(Elements, Node);
		}
	}
}
