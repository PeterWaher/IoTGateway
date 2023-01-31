using System;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IVector"/> values.
	/// </summary>
	public class VectorEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IVector"/> values.
		/// </summary>
		public VectorEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to JSON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Json">JSON output.</param>
		public void Encode(object Object, int? Indent, StringBuilder Json)
		{
			IVector V = (IVector)Object;
			bool First = true;

			Json.Append('[');

			if (Indent.HasValue)
				Indent++;

			foreach (IElement Element in V.VectorElements)
			{
				if (First)
					First = false;
				else
					Json.Append(',');

				if (Indent.HasValue)
				{
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
				}

				Encode(Element.AssociatedObjectValue, Indent, Json);
			}

			if (Indent.HasValue)
			{
				Indent--;

				if (!First)
				{
					Json.AppendLine();
					Json.Append(new string('\t', Indent.Value));
				}
			}

			Json.Append(']');
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return typeof(IVector).GetTypeInfo().IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
