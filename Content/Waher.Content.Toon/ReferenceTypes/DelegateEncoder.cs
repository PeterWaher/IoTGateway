using System;
using System.Reflection;
using System.Text;
using Waher.Content.Toon.Model;
using Waher.Runtime.Inventory;

namespace Waher.Content.Toon.ReferenceTypes
{
	/// <summary>
	/// Encodes <see cref="Delegate"/> values.
	/// </summary>
	public class DelegateEncoder : SimpleToonEncoder
	{
		/// <summary>
		/// Encodes <see cref="Delegate"/> values.
		/// </summary>
		public DelegateEncoder()
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
			Delegate D = (Delegate)Object;
			StringBuilder sb = new StringBuilder();
			bool First = true;

			if (!(D.Target is null))
			{
				sb.Append(D.Target.ToString());
				sb.Append('.');
			}
			sb.Append(D.Method.Name);
			sb.Append('(');

			foreach (ParameterInfo Arg in D.Method.GetParameters())
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append(Arg.Name);
			}

			sb.Append(')');

			Toon.Append(TOON.Encode(sb.ToString(), !Toon.Empty));
		}

		/// <summary>
		/// How well the TOON encoder encodes objects of type <paramref name="ObjectType"/>.
		/// </summary>
		/// <param name="ObjectType">Type of object to encode.</param>
		/// <returns>How well objects of the given type are encoded.</returns>
		public override Grade Supports(Type ObjectType)
		{
			return typeof(Delegate).IsAssignableFrom(ObjectType.GetTypeInfo()) ? Grade.Ok : Grade.NotAtAll;
		}
	}
}
