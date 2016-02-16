using System;
using System.Collections.Generic;
using System.Text;
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
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			if (this.value == null)
				return "null";
			else
				return this.value.ToString();
		}

		/// <summary>
		/// Associated Set.
		/// </summary>
		public override Set AssociatedSet
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
			ObjectValue E = obj as ObjectValue;
			if (E == null)
				return false;
			else
				return this.value.Equals(E.value);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			if (this.value == null)
				return 0;
			else
				return this.value.GetHashCode();
		}
	}
}
