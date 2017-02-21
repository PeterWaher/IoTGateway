using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Boolean-valued parameter.
	/// </summary>
	public class BooleanParameter : Parameter
	{
		private bool value;

		/// <summary>
		/// Boolean-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public BooleanParameter(string Id, string Name, bool Value)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public bool Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Exports the parameters to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Export(StringBuilder Xml)
		{
			Xml.Append("<boolean");
			base.Export(Xml);
			Xml.Append(" value='");
			Xml.Append(CommonTypes.Encode(this.value));
			Xml.Append("'/>");
		}
	}
}
