using System;
using System.Reflection;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IElement"/> values.
	/// </summary>
	public class ScriptElementEncoder : SimpleToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IElement"/> values.
		/// </summary>
		public ScriptElementEncoder()
		{
		}

		/// <summary>
		/// Encodes the <paramref name="Object"/> to TOON.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Indent">Any indentation to apply.</param>
		/// <param name="Toon">TOON output.</param>
		public override void Encode(object Object, int? Indent, ToonOutput Toon)
		{
			IElement E = (IElement)Object;
			object Obj = E.AssociatedObjectValue;

			if (E.Equals(Obj))
			{
				string s = Expression.ToExpressionString(Obj);
				TOON.Encode(s, Indent, Toon);
			}
			else
				TOON.Encode(Obj, Indent, Toon);
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			if (typeof(IElement).IsAssignableFrom(ObjectType.GetTypeInfo()) &&
				!typeof(IVector).IsAssignableFrom(ObjectType.GetTypeInfo()))
			{
				return Grade.Ok;
			}
			else
				return Grade.NotAtAll;
		}
	}
}
