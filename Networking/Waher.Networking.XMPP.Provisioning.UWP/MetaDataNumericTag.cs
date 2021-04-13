using System;
using Waher.Content;

namespace Waher.Networking.XMPP.Provisioning
{
	/// <summary>
	/// Meta-data numeric tag.
	/// </summary>
	public class MetaDataNumericTag : MetaDataTag
	{
		private readonly double value;

		/// <summary>
		/// Meta-data numeric tag.
		/// </summary>
		/// <param name="Name">Tag name.</param>
		/// <param name="Value">Tag value.</param>
		public MetaDataNumericTag(string Name, double Value)
			: base(Name)
		{
			this.value = Value;
		}

		/// <summary>
		/// String-representation of meta-data tag value.
		/// </summary>
		public override string StringValue
		{
			get { return CommonTypes.Encode(this.value); }
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
