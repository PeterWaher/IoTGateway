using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Objects;

namespace Waher.Script
{
	/// <summary>
	/// Contains information about a variable.
	/// </summary>
	public class Variable
	{
		private string name;
		private IElement value;

		/// <summary>
		/// Contains information about a variable.
		/// </summary>
		/// <param name="Name">Name of variable.</param>
		/// <param name="Value">Value of variable.</param>
		public Variable(string Name, IElement Value)
		{
			this.name = Name;
			this.value = Value;
		}

		/// <summary>
		/// Contains information about a variable.
		/// </summary>
		/// <param name="Name">Name of variable.</param>
		/// <param name="Value">Value of variable.</param>
		public Variable(string Name, double Value)
			: this(Name, new DoubleNumber(Value))
		{
		}

		/// <summary>
		/// Contains information about a variable.
		/// </summary>
		/// <param name="Name">Name of variable.</param>
		/// <param name="Value">Value of variable.</param>
		public Variable(string Name, string Value)
			: this(Name, new StringValue(Value))
		{
		}

		/// <summary>
		/// Contains information about a variable.
		/// </summary>
		/// <param name="Name">Name of variable.</param>
		/// <param name="Value">Value of variable.</param>
		public Variable(string Name, bool Value)
			: this(Name, new BooleanValue(Value))
		{
		}

		/// <summary>
		/// Contains information about a variable.
		/// </summary>
		/// <param name="Name">Name of variable.</param>
		/// <param name="Value">Value of variable.</param>
		public Variable(string Name, object Value)
		{
			this.name = Name;
			this.SetValue(Value);
		}

		/// <summary>
		/// Sets the value of the variable.
		/// </summary>
		/// <param name="Value">Value of variable.</param>
		public void SetValue(object Value)
		{
			Type T = Value.GetType();
			switch (Type.GetTypeCode(T))
			{
				case TypeCode.Boolean:
					this.value = new BooleanValue((bool)Value);
					break;

				case TypeCode.Byte:
					this.value = new DoubleNumber((byte)Value);
					break;

				case TypeCode.Char:
					this.value = new StringValue(new string((char)Value, 1));
					break;

				case TypeCode.DateTime:
					this.value = new DateTimeValue((DateTime)Value);
					break;

				case TypeCode.DBNull:
					this.value = new ObjectValue(null);
					break;

				case TypeCode.Decimal:
					this.value = new DoubleNumber((double)((decimal)Value));
					break;

				case TypeCode.Double:
					this.value = new DoubleNumber((double)Value);
					break;

				case TypeCode.Empty:
					this.value = new ObjectValue(null);
					break;

				case TypeCode.Int16:
					this.value = new DoubleNumber((short)Value);
					break;

				case TypeCode.Int32:
					this.value = new DoubleNumber((int)Value);
					break;

				case TypeCode.Int64:
					this.value = new DoubleNumber((long)Value);
					break;

				case TypeCode.SByte:
					this.value = new DoubleNumber((sbyte)Value);
					break;

				case TypeCode.Single:
					this.value = new DoubleNumber((float)Value);
					break;

				case TypeCode.String:
					this.value = new StringValue((string)Value);
					break;

				case TypeCode.UInt16:
					this.value = new DoubleNumber((ushort)Value);
					break;

				case TypeCode.UInt32:
					this.value = new DoubleNumber((uint)Value);
					break;

				case TypeCode.UInt64:
					this.value = new DoubleNumber((ulong)Value);
					break;

				case TypeCode.Object:
				default:
					if (Value is Element)
						this.value = (Element)Value;
					else
						this.value = new ObjectValue(Value);
					break;
			}
		}

		/// <summary>
		/// Name of variable.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Value element of variable.
		/// </summary>
		public IElement ValueElement
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// Object Value of variable.
		/// </summary>
		public object ValueObject
		{
			get { return this.value.AssociatedObjectValue; }
		}

	}
}
