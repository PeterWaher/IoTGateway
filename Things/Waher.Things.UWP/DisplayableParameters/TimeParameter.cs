using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// TimeSpan-valued parameter.
	/// </summary>
	public class TimeSpanParameter : Parameter
	{
		private TimeSpan value;

		/// <summary>
		/// TimeSpan-valued parameter.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		/// <param name="Value">Parameter Value</param>
		public TimeSpanParameter(string Id, string Name, TimeSpan Value)
			: base(Id, Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// Parameter Value.
		/// </summary>
		public TimeSpan Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Exports the parameters to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Export(StringBuilder Xml)
		{
			Xml.Append("<time");
			base.Export(Xml);
			Xml.Append(" value='");
			Xml.Append(this.value.ToString());
			Xml.Append("'/>");
		}
	}
}
