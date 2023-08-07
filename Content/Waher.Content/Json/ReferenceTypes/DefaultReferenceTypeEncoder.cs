using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encoder used for reference types, if no other encoder is found.
	/// </summary>
	public class DefaultReferenceTypeEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encoder used for reference types, if no other encoder is found.
		/// </summary>
		public DefaultReferenceTypeEncoder()
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
			Type T = Object.GetType();
			LinkedList<KeyValuePair<string, object>> Properties = new LinkedList<KeyValuePair<string, object>>();
			object Value;

			foreach (FieldInfo FI in T.GetRuntimeFields())
			{
				if (FI.IsPublic && !FI.IsStatic)
				{
					Value = FI.GetValue(Object);

					if (!Object.Equals(Value))
						Properties.AddLast(new KeyValuePair<string, object>(FI.Name, Value));
				}
			}

			foreach (PropertyInfo PI in T.GetRuntimeProperties())
			{
				if (PI.CanRead && 
					PI.GetMethod.IsPublic && 
					PI.GetIndexParameters().Length == 0)
				{
					Value = PI.GetValue(Object, null);

					if (!Object.Equals(Value))
						Properties.AddLast(new KeyValuePair<string, object>(PI.Name, Value));
				}
			}

			JSON.Encode(Properties, Indent, Json);
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return !typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Barely : Grade.NotAtAll;
		}
	}
}
