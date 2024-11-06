using System.Xml;

namespace Waher.Networking.XMPP.Events
{
	/// <summary>
	/// Event arguments for Private XML Storage callback methods.
	/// </summary>
	public class PrivateXmlEventArgs : IqResultEventArgs
	{
		private readonly XmlElement element;

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
