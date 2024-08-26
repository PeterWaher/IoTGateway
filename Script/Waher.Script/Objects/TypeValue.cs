using System;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Operators.Vectors;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Type value.
	/// </summary>
	public sealed class TypeValue : Element, IToVector
	{
        private static readonly TypeValues associatedSet = new TypeValues();

        private Type @value;
		private Type arrayType = null;

		/// <summary>
		/// Type value.
		/// </summary>
		/// <param name="Value">Type value.</param>
		public TypeValue(Type Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Type value.
		/// </summary>
		public Type Value
		{
			get => this.@value;
			set => this.@value = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.value.FullName;
		}

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet => associatedSet;

		/// <summary>
		/// Associated Type value.
		/// </summary>
		public override object AssociatedObjectValue => this.value;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is TypeValue E))
				return false;
			else
				return this.value.Equals(E.value);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			if (this.value is null)
				return 0;
			else
				return this.value.GetHashCode();
		}

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        public override bool TryConvertTo(Type DesiredType, out object Value)
        {
			TypeInfo TI = DesiredType.GetTypeInfo();

			if (TI.IsAssignableFrom(typeof(Type).GetTypeInfo()))
            {
                Value = this.value;
                return true;
            }
			else if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(Type).GetTypeInfo()))
			{
				Value = this.value;
				return true;
			}
			else if (TI.IsAssignableFrom(typeof(TypeValue).GetTypeInfo()))
            {
                Value = this;
                return true;
            }
            else
				return Expression.TryConvert(this.value, DesiredType, out Value);
		}

		/// <summary>
		/// Converts the object to a vector.
		/// </summary>
		/// <returns>Matrix.</returns>
		public IElement ToVector()
		{
			if (this.arrayType is null)
			{
				if (this.value.IsArray)
					this.arrayType = this.value;
				else
				{
					Array A = Array.CreateInstance(this.value, 0);
					this.arrayType = A.GetType();
				}
			}

			return new TypeValue(this.arrayType);
		}

	}
}
