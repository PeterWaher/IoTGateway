using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Namespace.
	/// </summary>
	public sealed class Namespace : Element
	{
		private static readonly Namespaces associatedSet = new Namespaces();

		private string value;

		/// <summary>
		/// Namespace value.
		/// </summary>
		/// <param name="Value">Namespace value.</param>
		public Namespace(string Value)
		{
			this.value = Value;
		}

		/// <summary>
		/// Namespace.
		/// </summary>
		public string Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// <see cref="Object.ToNamespace()"/>
		/// </summary>
		public override string ToString()
		{
			return this.value;
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
			get { return this; }
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			Namespace E = obj as Namespace;
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
