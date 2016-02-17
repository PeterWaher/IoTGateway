using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// DateTime-valued number.
	/// </summary>
	public sealed class DateTimeValue : Element
	{
		private static readonly DateTimeValues associatedSet = new DateTimeValues();

		private DateTime value;

		/// <summary>
		/// DateTime-valued number.
		/// </summary>
		/// <param name="Value">DateTime value.</param>
		public DateTimeValue(DateTime Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// DateTime value.
		/// </summary>
		public DateTime Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value.ToString();
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
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			DateTimeValue E = obj as DateTimeValue;
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
	}
}
