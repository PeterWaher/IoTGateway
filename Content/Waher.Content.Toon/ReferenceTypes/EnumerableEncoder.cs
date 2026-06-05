using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IEnumerable"/> values.
	/// </summary>
	public class EnumerableEncoder : VectorToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IEnumerable"/> values.
		/// </summary>
		public EnumerableEncoder()
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
			if (Vector is Array A)
				return A.Length;
			else if (Vector is ICollection C)
				return C.Count;
			else
			{
				IEnumerable E = (IEnumerable)Vector;
				IEnumerator e = E.GetEnumerator();
				int Count = 0;

				while (e.MoveNext())
					Count++;

				return Count;
			}
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		/// <param name="UseBrackets">If brackets should be used around the vector.</param>
		public override void Encode(object Object, int? Indent, ToonOutput Toon, bool UseBrackets)
		{
			IEnumerable E = (IEnumerable)Object;
			IEnumerator e = E.GetEnumerator();
			bool First = true;

			if (UseBrackets)
			{
				Toon.Append('[');

				if (Indent.HasValue)
					Indent++;
			}

			while (e.MoveNext())
			{
				if (First)
					First = false;
				else
					Toon.AppendDelimiter();

				if (UseBrackets)
				{
					if (Indent.HasValue && Indent.Value > 0)
					{
						Toon.Append('\n');
						Toon.Indent(Indent.Value);
					}

					TOON.Encode(e.Current, Indent, Toon);
				}
				else
					TOON.Encode(e.Current, null, Toon);
			}

			if (UseBrackets)
			{
				if (Indent.HasValue)
				{
					Indent--;

					if (!First && Indent.Value > 0)
					{
						Toon.Append('\n');
						Toon.Indent(Indent.Value);
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
			return typeof(IEnumerable).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Barely : Grade.NotAtAll;
		}
	}
}
