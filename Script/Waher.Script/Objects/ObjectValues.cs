using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Objects
{
	/// <summary>
	/// Set of object values.
	/// </summary>
	public sealed class ObjectValues : SemiGroup
	{
		private static readonly int hashCode = typeof(ObjectValues).FullName.GetHashCode();

		/// <summary>
		/// Set of object values.
		/// </summary>
		public ObjectValues()
		{
		}

		/// <summary>
		/// Checks if the set contains an element.
		/// </summary>
		/// <param name="Element">Element.</param>
		/// <returns>If the element is contained in the set.</returns>
		public override bool Contains(Element Element)
		{
			return Element is ObjectValue;
		}

		/// <summary>
		/// <see cref="Object.Equals"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ObjectValues;
		}

		/// <summary>
		/// <see cref="Object.GetHashCode"/>
		/// </summary>
		public override int GetHashCode()
		{
			return hashCode;
		}
	}
}
