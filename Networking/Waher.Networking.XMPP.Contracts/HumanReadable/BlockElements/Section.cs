using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// A section consisting of a header and a body.
	/// </summary>
	public class Section : Blocks
	{
		private InlineElement[] header;

		/// <summary>
		/// Header elements
		/// </summary>
		public InlineElement[] Header
		{
			get => this.header;
			set => this.header = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		public override bool IsWellDefined
		{
			get
			{
				foreach (InlineElement E in this.header)
				{
					if (E == null || !E.IsWellDefined)
						return false;
				}

				return base.IsWellDefined;
			}
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<section>");
			Serialize(Xml, this.Header, "header");
			Serialize(Xml, this.Body, "body");
			Xml.Append("</section>");
		}

	}
}
