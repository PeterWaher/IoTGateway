using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace Waher.Client.WPF.Model
{
	public class EventLog : XmppComponent
	{
		/// <summary>
		/// urn:xmpp:eventlog
		/// </summary>
		internal const string NamespaceEventLogging = "urn:xmpp:eventlog";

		public EventLog(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent, JID, Name, Node, Features)
		{
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		public override ImageSource ImageResource => XmppAccountNode.log;

		public override string ToolTip
		{
			get
			{
				return "Event Log";
			}
		}

	}
}
