using System;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IVector"/> values.
	/// </summary>
	public class VectorEncoder : IToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IVector"/> values.
		/// </summary>
		public VectorEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Toon)
		{
			IVector V = (IVector)Object;
			bool First = true;

			Toon.Append('[');

			if (Indent.HasValue)
				Indent++;

			foreach (IElement Element in V.VectorElements)
			{
				if (First)
					First = false;
				else
					Toon.Append(',');

				if (Indent.HasValue)
				{
					Toon.AppendLine();
					Toon.Append(new string('\t', Indent.Value));
				}

				TOON.Encode(Element.AssociatedObjectValue, Indent, Toon);
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First)
				{
					Toon.AppendLine();
					Toon.Append(new string('\t', Indent.Value));
				}
			}

			Toon.Append(']');
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return typeof(IVector).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
