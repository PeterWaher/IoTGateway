using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Type value.
	/// </summary>
	public sealed class TypeValue : Element
	{
        private static readonly TypeValues associatedSet = new TypeValues();

        private Type value;

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
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// <see cref="Type.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value.FullName;
		}

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override ISet AssociatedSet
		{
			get { return associatedSet; }
		}

		/// <summary>
		/// Associated Type value.
		/// </summary>
		public override object AssociatedObjectValue
		{
			get { return this.value; }
		}

		/// <summary>
		/// <see cref="Type.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			TypeValue E = obj as TypeValue;
			if (E is null)
				return false;
			else
				return this.value.Equals(E.value);
		}

		/// <summary>
		/// <see cref="Type.GetHashCode"/>
		/// </summary>
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
            {
                Value = null;
                return false;
            }
        }

    }
}
