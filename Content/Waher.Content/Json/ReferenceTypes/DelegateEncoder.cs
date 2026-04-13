using System;
using System.Reflection;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Content.Json.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="Delegate"/> values.
	/// </summary>
	public class DelegateEncoder : IJsonEncoder
	{
		/// <summary>
		/// Encodes <see cref="Delegate"/> values.
		/// </summary>
		public DelegateEncoder()
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
			Delegate D = (Delegate)Object;
			StringBuilder sb = new StringBuilder();
			bool First = true;

			if (!(D.Target is null))
			{
				Json.Append(D.Target.ToString());
				Json.Append('.');
			}
			Json.Append(D.Method.Name);
			Json.Append('(');

			foreach (ParameterInfo Arg in D.Method.GetParameters())
			{
				if (First)
					First = false;
				else
					Json.Append(',');

				Json.Append(Arg.Name);
			}

			Json.Append(')');


			Json.Append('"');
			Json.Append(JSON.Encode(sb.ToString()));
			Json.Append('"');
		}

		/// <summary>
		/// How well the JSON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public Grade Supports(Type ObjectType)
		{
			return typeof(Delegate).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
