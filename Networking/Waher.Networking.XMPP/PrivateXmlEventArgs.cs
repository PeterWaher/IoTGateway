using System;
using System.Collections.Generic;
using System.Xml;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP
{
	/// <summary>
	/// Delegate for Private XML callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void PrivateXmlEventHandler(object Sender, PrivateXmlEventArgs e);

	/// <summary>
	/// Event arguments for Private XML Storage callback methods.
	/// </summary>
	public class PrivateXmlEventArgs : IqResultEventArgs
	{
		private XmlElement element;

		/// <summary>
		/// Event arguments for Private XML Storage callback methods.
		/// </summary>
		/// <param name="Element">XML Element found.</param>
		/// <param name="e">IQ event arguments.</param>
		public PrivateXmlEventArgs(XmlElement Element, IqResultEventArgs e)
			: base(e)
		{
			this.element = Element;
		}

		/// <summary>
		/// XML Element if found, null otherwise.
		/// </summary>
		public XmlElement Element => this.element;
	}
}
