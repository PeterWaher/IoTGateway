using System;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="Assembly"/> values.
	/// </summary>
	public class AssemblyEncoder : IToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="Assembly"/> values.
		/// </summary>
		public AssemblyEncoder()
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
			Assembly A = (Assembly)Object;

			Toon.Append(TOON.Encode(A.FullName));
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return typeof(Assembly).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
