using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Int64-valued parameter.
	/// </summary>
	public class Int64Parameter : Parameter
	{
		private long value;

		/// <summary>
		/// Int64-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public Int64Parameter(string Id, string Name, long Value)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public long Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Untyped parameter value
		/// </summary>
		public override object UntypedValue
		{
			get { return this.value; }
		}

		/// <summary>
		/// Exports the parameters to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Export(StringBuilder Xml)
		{
			Xml.Append("<long");
			base.Export(Xml);
			Xml.Append(" value='");
			Xml.Append(this.value.ToString());
			Xml.Append("'/>");
		}
	}
}
