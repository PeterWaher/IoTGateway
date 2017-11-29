using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Provisioning.SearchOperators;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Questions;
using Waher.Client.WPF.Dialogs;

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

		public override ImageSource ImageResource => XmppAccountNode.database;

		public override string ToolTip
		{
			get
			{
				return "Event Log";
			}
		}

	}
}
