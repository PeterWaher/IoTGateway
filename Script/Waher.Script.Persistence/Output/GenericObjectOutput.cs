using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence;
using Waher.Persistence.Serialization;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Output;

namespace Waher.Script.Persistence.Output
{
	/// <summary>
	/// Converts values of type <see cref="GenericObject"/> to expression strings.
	/// </summary>
	public class GenericObjectOutput : ICustomStringOutput
	{
		/// <summary>
		/// Type
		/// </summary>
		public Type Type => typeof(GenericObject);

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
