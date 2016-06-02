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
	/// Named member operator
	/// </summary>
	public class NamedMember : UnaryOperator
	{
		private string name;

		/// <summary>
		/// Named member operator
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public NamedMember(ScriptNode Operand, string Name, int Start, int Length)
			: base(Operand, Start, Length)
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
			Type T;

			T = Value as Type;
			if (T == null)
			{
				Instance = Value;
				T = Value.GetType();
			}
			else
				Instance = null;

			lock (this.synchObject)
			{
				if (T != this.type)
				{
					this.type = T;
					this.property = T.GetProperty(this.name);
					if (this.property != null)
					{
						this.field = null;
						this.nameIndex = null;
					}
					else
					{
						this.field = T.GetField(this.name);
						if (this.field != null)
							this.nameIndex = null;
						else
						{
#if WINDOWS_UWP
							this.property = T.GetProperty("Item", typeof(object), stringType);
#else
							this.property = T.GetProperty("Item", stringType);
#endif
							if (this.property == null)
								this.nameIndex = null;
							else if (this.nameIndex == null)
								this.nameIndex = new string[] { this.name };
						}
					}
				}

				if (this.property != null)
				{
					if (this.nameIndex != null)
						return Expression.Encapsulate(this.property.GetValue(Instance, this.nameIndex));
					else
						return Expression.Encapsulate(this.property.GetValue(Instance, null));
				}
				else if (this.field != null)
					return Expression.Encapsulate(this.field.GetValue(Instance));
				else if (Operand.IsScalar)
					throw new ScriptRuntimeException("Member not found.", this);
			}

			LinkedList<IElement> Elements = new LinkedList<IElement>();

			foreach (IElement E in Operand.ChildElements)
				Elements.AddLast(EvaluateDynamic(E, this.name, this));

			return Operand.Encapsulate(Elements, this);
		}

		private Type type = null;
		private PropertyInfo property = null;
		private FieldInfo field = null;
		private string[] nameIndex = null;
		private object synchObject = new object();

		internal static readonly Type[] stringType = new Type[] { typeof(string) };

		/// <summary>
		/// Evaluates the member operator dynamically on an operand.
		/// </summary>
		/// <param name="Operand">Operand.</param>
		/// <param name="Name">Name of member.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Result.</returns>
		public static IElement EvaluateDynamic(IElement Operand, string Name, ScriptNode Node)
		{
			object Value = Operand.AssociatedObjectValue;
			object Instance;
			Type T;

			T = Value as Type;
			if (T == null)
			{
				Instance = Value;
				T = Value.GetType();
			}
			else
				Instance = null;

			PropertyInfo Property = T.GetProperty(Name);
			if (Property != null)
				return Expression.Encapsulate(Property.GetValue(Instance, null));

			FieldInfo Field = T.GetField(Name);
			if (Field != null)
				return Expression.Encapsulate(Field.GetValue(Instance));

#if WINDOWS_UWP
			Property = T.GetProperty("Item", typeof(object), stringType);
#else
			Property = T.GetProperty("Item", stringType);
#endif
			if (Property != null)
				return Expression.Encapsulate(Property.GetValue(Instance, new string[] { Name }));

			if (Operand.IsScalar)
				throw new ScriptRuntimeException("Member not found.", Node);

			LinkedList<IElement> Elements = new LinkedList<IElement>();

			foreach (IElement E in Operand.ChildElements)
				Elements.AddLast(EvaluateDynamic(E, Name, Node));

			return Operand.Encapsulate(Elements, Node);
		}
	}
}
