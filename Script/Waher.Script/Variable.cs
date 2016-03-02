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
        internal void SetValue(object Value)
        {
            this.value = Expression.Encapsulate(Value);
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
		}

		/// <summary>
		/// Object Value of variable.
		/// </summary>
		public object ValueObject
		{
			get { return this.value.AssociatedObjectValue; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			ObjectValue v = this.value as ObjectValue;
			if (v != null && v.Value == this)
				return this.name + "=self";
			else
				return this.name + "=" + this.value.ToString();
		}

	}
}
