using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using Waher.Networking.XMPP;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents an unspecialized XMPP contact.
	/// </summary>
	public class XmppOther : XmppContact 
	{
		public XmppOther(TreeNode Parent, RosterItem RosterItem)
			: base(Parent, RosterItem)
		{
		}
	}
}
