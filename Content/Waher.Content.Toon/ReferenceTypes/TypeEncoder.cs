using System;
using System.Reflection;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="Type"/> values.
	/// </summary>
	public class TypeEncoder : SimpleToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="Type"/> values.
		/// </summary>
		public TypeEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public override void Encode(object Object, int? Indent, StringBuilder Toon)
		{
			Type T = (Type)Object;
			Toon.Append(TOON.Encode(T.FullName));
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return typeof(Type).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
