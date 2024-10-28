using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;

namespace Waher.Script
{
	/// <summary>
	/// Contains information about a variable.
	/// </summary>
	public class Variable
	{
		private readonly string name;
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
            this.value = Expression.Encapsulate(Value);
        }

		/// <summary>
		/// Name of variable.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Value element of variable.
		/// </summary>
		public IElement ValueElement => this.value;

		/// <summary>
		/// Object Value of variable.
		/// </summary>
		public object ValueObject => this.value.AssociatedObjectValue;

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.value is ObjectValue v)
			{
				object Obj = v.Value;

				if (Obj == this)
					return this.name + "=self";
				else if (Obj is Variable Var)
					return this.name + "=" + Var.name;
			}

			return this.name + "=" + this.value.ToString();
		}

	}
}
