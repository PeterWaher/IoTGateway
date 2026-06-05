using System;
using System.Reflection;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IVector"/> values.
	/// </summary>
	public class VectorEncoder : VectorToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IVector"/> values.
		/// </summary>
		public VectorEncoder()
		{
		}

		/// <summary>
		/// Gets the number of elements in the vector.
		/// </summary>
		/// <param name="Vector">Vector object.</param>
		/// <returns>Number of elements in the vector. If null is returned, the
		/// <paramref name="Vector"/> item should not be considered a vector.</returns>
		public override int? GetCount(object Vector)
		{
			IVector V = (IVector)Vector;
			return V.Dimension;
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="UseBrackets">If brackets should be used around the vector.</param>
		public override void Encode(object Object, int? Indent, StringBuilder Toon, bool UseBrackets)
		{
			IVector V = (IVector)Object;
			bool First = true;

			if (UseBrackets)
			{
				Toon.Append('[');

				if (Indent.HasValue)
					Indent++;
			}

			foreach (IElement Element in V.VectorElements)
			{
				if (First)
					First = false;
				else
					Toon.Append(',');

				if (UseBrackets)
				{
					if (Indent.HasValue && Indent.Value > 0)
					{
						Toon.Append('\n');
						JSON.Indent(Toon, Indent.Value);
					}

					TOON.Encode(Element.AssociatedObjectValue, Indent, Toon);
				}
				else
					TOON.Encode(Element.AssociatedObjectValue, null, Toon);
			}

			if (UseBrackets)
			{
				if (Indent.HasValue)
				{
					Indent--;

					if (!First && Indent.Value > 0)
					{
						Toon.Append('\n');
						JSON.Indent(Toon, Indent.Value);
					}
				}

				Toon.Append(']');
			}
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return typeof(IVector).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
