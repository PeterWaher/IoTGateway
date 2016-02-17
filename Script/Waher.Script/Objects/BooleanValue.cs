using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Boolean-valued number.
	/// </summary>
	public sealed class BooleanValue : FieldElement
	{
		private static readonly BooleanValues associatedField = new BooleanValues();

		private bool value;

		/// <summary>
		/// Boolean-valued number.
		/// </summary>
		/// <param name="Value">Boolean value.</param>
		public BooleanValue(bool Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Boolean value.
		/// </summary>
		public bool Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value ? "⊤" : "⊥";
		}

		/// <summary>
		/// Associated Field.
		/// </summary>
		public override IField AssociatedField
		{
			get { return associatedField; }
		}

		/// <summary>
		/// Associated object value.
		/// </summary>
		public override object AssociatedObjectValue
		{
			get { return this.value; }
		}

		/// <summary>
		/// Tries to multiply an element to the current element.
		/// </summary>
		/// <param name="Element">Element to multiply.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override ICommutativeRingElement Multiply(ICommutativeRingElement Element)
		{
			BooleanValue E = Element as BooleanValue;
			if (E == null)
				return null;
			else
				return new BooleanValue(this.value && E.value);
		}

		/// <summary>
		/// Inverts the element, if possible.
		/// </summary>
		/// <returns>Inverted element, or null if not possible.</returns>
		public override IRingElement Invert()
		{
			return new BooleanValue(this.value);
		}

		/// <summary>
		/// Tries to add an element to the current element.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override IAbelianGroupElement Add(IAbelianGroupElement Element)
		{
			BooleanValue E = Element as BooleanValue;
			if (E == null)
				return null;
			else
				return new BooleanValue(this.value ^ E.value);
		}

		/// Negates the element.
		/// </summary>
		/// <returns>Negation of current element.</returns>
		public override IGroupElement Negate()
		{
			return new BooleanValue(this.value);
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			BooleanValue E = obj as BooleanValue;
			if (E == null)
				return false;
			else
				return this.value == E.value;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		/// <summary>
		/// Constant true value.
		/// </summary>
		public static readonly BooleanValue True = new BooleanValue(true);

		/// <summary>
		/// Constant false value.
		/// </summary>
		public static readonly BooleanValue False = new BooleanValue(false);
	}
}
