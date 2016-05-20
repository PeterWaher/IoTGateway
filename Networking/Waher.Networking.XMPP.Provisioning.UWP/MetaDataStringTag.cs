using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Meta-data string tag.
	/// </summary>
	public class MetaDataStringTag : MetaDataTag
	{
		private string value;

		/// <summary>
		/// Meta-data string tag.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public MetaDataStringTag(string Name, string Value)
			: base(Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// String-representation of meta-data tag value.
		/// </summary>
		public override string StringValue
		{
			get { return this.value; }
		}

		/// <summary>
		/// Meta-data tag value.
		/// </summary>
		public override object Value
		{
			get { return this.value; }
		}
	}
}
