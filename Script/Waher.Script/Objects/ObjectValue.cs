using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
    /// <summary>
    /// Object value.
    /// </summary>
    public sealed class ObjectValue : Element
    {
        private static readonly ObjectValues associatedSet = new ObjectValues();

        private object value;

        /// <summary>
        /// Object value.
        /// </summary>
        /// <param name="Value">Object value.</param>
        public ObjectValue(object Value)
        {
            this.value = Value;
        }

        /// <summary>
        /// Object value.
        /// </summary>
        public object Value
        {
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
        {
			if (this.value == null)
				return "null";
			else
				return Expression.ToString(this.value);
        }

        /// <summary>
        /// Associated Set.
        /// </summary>
        public override ISet AssociatedSet
        {
            get { return associatedSet; }
        }

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue
        {
            get { return this.value; }
        }

        /// <summary>
        /// <see cref="Object.Equals(object)"/>
        /// </summary>
        public override bool Equals(object obj)
        {
			if (obj is ObjectValue E)
			{
				if (this.value == null)
					return E.value == null;
				else if (E.value == null)
					return false;
				else
					return this.value.Equals(E.value);
			}
			else
				return false;
        }

        /// <summary>
        /// <see cref="Object.GetHashCode()"/>
        /// </summary>
        public override int GetHashCode()
        {
            if (this.value == null)
                return 0;
            else
                return this.value.GetHashCode();
        }

        /// <summary>
        /// Null value.
        /// </summary>
        public static readonly ObjectValue Null = new ObjectValue(null);

        /// <summary>
        /// Converts the value to a .NET type.
        /// </summary>
        /// <param name="DesiredType">Desired .NET type.</param>
        /// <param name="Value">Converted value.</param>
        /// <returns>If conversion was possible.</returns>
        public override bool TryConvertTo(Type DesiredType, out object Value)
        {
            if (this.value == null)
            {
                Value = null;
                return true;
            }
            else if (this.value.GetType() == DesiredType)
            {
                Value = this.value;
                return true;
            }
			else if (DesiredType == typeof(object))
			{
				Value = this.value;
				return true;
			}
			else if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(ObjectValue).GetTypeInfo()))
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
