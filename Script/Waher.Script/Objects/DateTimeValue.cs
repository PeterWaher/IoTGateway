﻿using System;
using System.Reflection;
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
			get => this.value;
			set => this.value = value;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
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
		public override object AssociatedObjectValue => this.value;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is IElement E) || !(E.AssociatedObjectValue is DateTime TP))
				return false;
			else
				return this.value == TP;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
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
			if (DesiredType == typeof(DateTime))
			{
				Value = this.value;
				return true;
			}
			else if (DesiredType == typeof(DateTimeOffset))
			{
				Value = (DateTimeOffset)this.value;
				return true;
			}
			else if (DesiredType == typeof(DateTimeValue))
			{
				Value = this;
				return true;
			}
			else if (DesiredType.IsAssignableFrom(typeof(DateTime).GetTypeInfo()))
			{
				Value = this.value;
				return true;
			}
			else
				return Expression.TryConvert(this.value, DesiredType, out Value);
		}
	}
}
