using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Script.Output;

namespace Waher.Script.Persistence.Output
{
	/// <summary>
	/// Converts values of type <see cref="GenericObject"/> to expression strings.
	/// </summary>
	public class GenericObjectOutput : ICustomStringOutput
	{
		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object) => Object == typeof(GenericObject) ? Grade.Ok : Grade.NotAtAll;

		/// <summary>
		/// Gets a string representing a value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>Expression string.</returns>
		public string GetString(object Value)
		{
			GenericObject Obj = (GenericObject)Value;

			StringBuilder sb = new StringBuilder();
			bool First = true;

			sb.Append('{');

			if (!string.IsNullOrEmpty(Obj.CollectionName) && !Obj.TryGetFieldValue("CollectionName", out object _))
				this.Output("CollectionName", Obj.CollectionName, ref First, sb);

			if (!string.IsNullOrEmpty(Obj.TypeName) && !Obj.TryGetFieldValue("TypeName", out object _))
				this.Output("TypeName", Obj.TypeName, ref First, sb);

			if (Obj.ObjectId != Guid.Empty && !Obj.TryGetFieldValue("ObjectId", out object _))
				this.Output("ObjectId", Obj.ObjectId.ToString(), ref First, sb);

			foreach (KeyValuePair<string, object> P in Obj.Properties)
				this.Output(P.Key, P.Value, ref First, sb);

			sb.Append('}');

			return sb.ToString();
		}

		private void Output(string Name, object Value, ref bool First, StringBuilder sb)
		{
			if (First)
				First = false;
			else
				sb.Append(", ");

			sb.Append(Name);
			sb.Append(": ");
			sb.Append(Expression.ToString(Value));
		}
	}
}
