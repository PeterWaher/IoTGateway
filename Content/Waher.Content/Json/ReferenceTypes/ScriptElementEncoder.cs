using System;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="IElement"/> values.
	/// </summary>
	public class ScriptElementEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="IElement"/> values.
		/// </summary>
		public ScriptElementEncoder()
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
			IElement E = (IElement)Object;
			object Obj = E.AssociatedObjectValue;

			if (E.Equals(Obj))
			{
				string s = Expression.ToString(Obj);
				JSON.Encode(s, Indent, Json);
			}
			else
				JSON.Encode(Obj, Indent, Json);
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			if (typeof(IElement).GetTypeInfo().IsAssignableFrom(ObjectType.GetTypeInfo()) &&
				!typeof(IVector).GetTypeInfo().IsAssignableFrom(ObjectType.GetTypeInfo()))
			{
				return Grade.Ok;
			}
			else
				return Grade.NotAtAll;
		}
	}
}
