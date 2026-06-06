using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encoder used for reference types, if no other encoder is found.
	/// </summary>
	public class DefaultReferenceTypeEncoder : ObjectToonEncoder
	{
		/// <summary>
		/// Encoder used for reference types, if no other encoder is found.
		/// </summary>
		public DefaultReferenceTypeEncoder()
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
			Dictionary<string, object> Typed = Prepare((IDictionary)Object);
			Toon.AppendAsObject(Typed, Indent, Typed.ContainsKey);
		}

		/// <summary>
		/// Gets available parameters to encode from an object.
		/// </summary>
		/// <param name="Object">Object to get parameters from.</param>
		/// <returns>Enumerator for the parameters, or null if not applicable.</returns>
		public override IEnumerator<KeyValuePair<string, object>> GetParameters(object Object)
		{
			return Prepare(Object).GetEnumerator();
		}

		private static Dictionary<string, object> Prepare(object Object)
		{
			Type T = Object.GetType();
			Dictionary<string, object> Properties = new Dictionary<string, object>();
			object Value;

			foreach (FieldInfo FI in T.GetRuntimeFields())
			{
				if (FI.IsPublic && !FI.IsStatic)
				{
					Value = FI.GetValue(Object);

					if (!Object.Equals(Value))
						Properties[FI.Name] = Value;
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
						Properties[PI.Name] = Value;
				}
			}

			return Properties;
		}

		/// <summary>
		/// Checks if an object can be folded to a shorter representation, and if so, 
		/// returns the folded name and value.
		/// </summary>
		/// <param name="Object">Object to encode</param>
		/// <param name="FoldedName">Folded name</param>
		/// <param name="FoldedValue">Folded value.</param>
		/// <returns>True if the object can be folded, otherwise false.</returns>
		public override bool CanFold(object Object, out string FoldedName, out object FoldedValue)
		{
			bool Found = false;

			FoldedName = null;
			FoldedValue = null;

			Type T = Object.GetType();
			object Value;

			foreach (FieldInfo FI in T.GetRuntimeFields())
			{
				if (FI.IsPublic && !FI.IsStatic)
				{
					Value = FI.GetValue(Object);

					if (!Object.Equals(Value))
					{
						if (Found)
							return false;

						FoldedName = FI.Name;
						FoldedValue = Value;
					}
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
					{
						if (Found)
							return false;

						FoldedName = PI.Name;
						FoldedValue = Value;
					}
				}
			}

			return Found;
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			TypeInfo TI = ObjectType.GetTypeInfo();
			if (TI.IsValueType)
			{
				MethodInfo MI = ObjectType.GetRuntimeMethod(nameof(ToString), Types.NoTypes);
				if (MI.DeclaringType != typeof(object))
					return Grade.NotAtAll;	// Will be encoded by DefaultValueTypeEncoder, encoding the value type as a string.
			}

			if (!typeof(IEnumerable).IsAssignableFrom(TI))
				return Grade.Barely;

			return Grade.NotAtAll;
		}
	}
}
