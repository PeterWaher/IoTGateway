using System;
using System.Collections.Generic;
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

		private string value;

		/// <summary>
		/// String value.
		/// </summary>
		/// <param name="Value">String value.</param>
		public StringValue(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// String value.
		/// </summary>
		public string Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return "\"" + this.value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\b", "\\b").Replace("\"", "\\\"") + "\"";
		}

		/// <summary>
		/// Associated Semi-Group.
		/// </summary>
		public override SemiGroup AssociatedSemiGroup
		{
			get { return associatedSemiGroup; }
		}

		/// <summary>
		/// Tries to add an element to the current element, from the left.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override SemiGroupElement AddLeft(SemiGroupElement Element)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Tries to add an element to the current element, from the right.
		/// </summary>
		/// <param name="Element">Element to add.</param>
		/// <returns>Result, if understood, null otherwise.</returns>
		public override SemiGroupElement AddRight(SemiGroupElement Element)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			StringValue E = obj as StringValue;
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
