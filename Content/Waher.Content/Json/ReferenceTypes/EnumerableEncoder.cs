using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IEnumerable"/> values.
	/// </summary>
	public class EnumerableEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IEnumerable"/> values.
		/// </summary>
		public EnumerableEncoder()
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
			IEnumerable E = (IEnumerable)Object;
			IEnumerator e = E.GetEnumerator();
			bool First = true;

			Json.Append('[');

			if (Indent.HasValue)
				Indent++;

			while (e.MoveNext())
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

				Encode(e.Current, Indent, Json);
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
			return typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Barely : Grade.NotAtAll;
		}
	}
}
