using System;
using System.Collections.Generic;
using System.Xml;

namespace Waher.Networking.XMPP
{
	public class IqResultEventArgs : EventArgs
	{
		private XmlElement response;
		private object state;
		private string id;
		private string to;
		private string from;
		private bool ok;

		public IqResultEventArgs(XmlElement Response, string Id, string To, string From, bool Ok, object State)
		{
			this.response = Response;
			this.id = Id;
			this.to = To;
			this.from = From;
			this.ok = Ok;
			this.state = State;
		}
	}
}
