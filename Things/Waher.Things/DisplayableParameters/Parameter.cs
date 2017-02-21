using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Things.DisplayableParameters
{
	/// <summary>
	/// Base class for all node parameters.
	/// </summary>
	public abstract class Parameter
	{
		private string id;
		private string name;

		/// <summary>
		/// Base class for all node parameters.
		/// </summary>
		/// <param name="Id">Parameter ID.</param>
		/// <param name="Name">Parameter Name.</param>
		public Parameter(string Id, string Name)
		{
			this.id = Id;
			this.name = Name;
		}

		/// <summary>
		/// Parameter ID.
		/// </summary>
		public string Id
		{
			get { return this.id; }
		}

		/// <summary>
		/// Parameter Name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}

		/// <summary>
		/// Exports the parameters to XML.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public virtual void Export(StringBuilder Xml)
		{
			Xml.Append(" id='");
			Xml.Append(XML.Encode(this.id));
			Xml.Append("' name='");
			Xml.Append(XML.Encode(this.name));
			Xml.Append("'");
		}
	}
}
