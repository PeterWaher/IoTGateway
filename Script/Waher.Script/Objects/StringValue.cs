using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
    /// <summary>
    /// String value.
    /// </summary>
    public sealed class StringValue : SemiGroupElement
    {
        private static readonly StringValues associatedSemiGroup = new StringValues();
        private static readonly CaseInsensitiveStringValues associatedSemiGroupCis = new CaseInsensitiveStringValues();

        private string value;
        private string valueLower = null;
        private bool caseInsensitive;

        /// <summary>
        /// String value.
        /// </summary>
        /// <param name="Value">String value.</param>
        public StringValue(string Value)
        {
            this.value = Value;
            this.caseInsensitive = false;
        }

        /// <summary>
        /// String value.
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="CaseInsensitive">If the string value is case insensitive or not.</param>
        public StringValue(string Value, bool CaseInsensitive)
        {
            this.value = Value;
            this.caseInsensitive = CaseInsensitive;
        }

        /// <summary>
        /// String value.
        /// </summary>
        public string Value
        {
			get { return this.value; }
			set { this.value = value; }
		}

        /// <summary>
        /// If the string value is case insensitive or not.
        /// </summary>
        public bool CaseInsensitive
        {
            get { return this.caseInsensitive; }
            set { this.caseInsensitive = value; }
        }

        /// <summary>
        /// <see cref="Object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
			return Expression.ToString(this.value);
        }

        /// <summary>
        /// Associated Semi-Group.
        /// </summary>
        public override ISemiGroup AssociatedSemiGroup
        {
            get
            {
                if (this.caseInsensitive)
                    return associatedSemiGroupCis;
                else
                    return associatedSemiGroup;
            }
        }

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue
        {
            get { return this.value; }
        }

        /// <summary>
        /// Tries to add an element to the current element, from the left.
        /// </summary>
        /// <param name="Element">Element to add.</param>
        /// <returns>Result, if understood, null otherwise.</returns>
        public override ISemiGroupElement AddLeft(ISemiGroupElement Element)
        {
            if (Element.IsScalar)
                return new StringValue(Element.AssociatedObjectValue.ToString() + this.value, this.caseInsensitive);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();
                ISemiGroupElement SE;

                foreach (IElement E in Element.ChildElements)
                {
                    SE = E as ISemiGroupElement;
                    if (SE is null)
                        Elements.AddLast(new StringValue(E.AssociatedObjectValue.ToString() + this.value, this.caseInsensitive));
                    else
                        Elements.AddLast(this.AddLeft(SE));
                }

                return (ISemiGroupElement)Element.Encapsulate(Elements, null);
            }
        }

        /// <summary>
        /// Tries to add an element to the current element, from the right.
        /// </summary>
        /// <param name="Element">Element to add.</param>
        /// <returns>Result, if understood, null otherwise.</returns>
        public override ISemiGroupElement AddRight(ISemiGroupElement Element)
        {
            if (Element.IsScalar)
                return new StringValue(this.value + Element.AssociatedObjectValue.ToString(), this.caseInsensitive);
            else
            {
                LinkedList<IElement> Elements = new LinkedList<IElement>();
                ISemiGroupElement SE;

                foreach (IElement E in Element.ChildElements)
                {
                    SE = E as ISemiGroupElement;
                    if (SE is null)
                        Elements.AddLast(new StringValue(this.value + E.AssociatedObjectValue.ToString(), this.caseInsensitive));
                    else
                        Elements.AddLast(this.AddRight(SE));
                }

                return (ISemiGroupElement)Element.Encapsulate(Elements, null);
            }
        }

        /// <summary>
        /// <see cref="Object.Equals(object)"/>
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is StringValue E))
                return false;
            else
                return string.Compare(this.value, E.value, this.caseInsensitive || E.caseInsensitive) == 0;
        }

        /// <summary>
        /// <see cref="Object.GetHashCode()"/>
        /// </summary>
        public override int GetHashCode()
        {
            if (this.caseInsensitive)
            {
                if (this.valueLower is null)
                    this.valueLower = this.value.ToLower();

                return this.valueLower.GetHashCode();
            }
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
            if (DesiredType == typeof(string))
            {
                Value = this.value;
                return true;
            }
            else if (DesiredType == typeof(char))
            {
                if (this.value.Length == 1)
                {
                    Value = this.value[0];
                    return true;
                }
                else
                {
                    Value = null;
                    return false;
                }
            }
			else if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(string).GetTypeInfo()))
			{
				Value = this.value;
				return true;
			}
			else if (DesiredType.GetTypeInfo().IsAssignableFrom(typeof(StringValue).GetTypeInfo()))
            {
                Value = this;
                return true;
            }
            else
				return Expression.TryConvert(this.value, DesiredType, out Value);
		}

		/// <summary>
		/// The empty string.
		/// </summary>
		public static readonly StringValue Empty = new StringValue(string.Empty);
    }
}
