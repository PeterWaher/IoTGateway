using System;
using System.Windows;
using System.Windows.Controls;
using Waher.Client.WPF.Model;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.RDP;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for RemoteDesktopView.xaml
	/// </summary>
	public partial class RemoteDesktopView : UserControl, ITabView
	{
		private readonly TreeNode node;
		private readonly XmppClient client;
		private readonly RemoteDesktopClient rdpClient;
		private readonly RemoteDesktopSession session;

		public RemoteDesktopView(TreeNode Node, XmppClient Client, RemoteDesktopClient RdpClient, RemoteDesktopSession Session)
		{
			this.node = Node;
			this.client = Client;
			this.rdpClient = RdpClient;
			this.session = Session;

			this.session.StateChanged += Session_StateChanged;

			InitializeComponent();
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			// TODO
		}

		public async void Dispose()
		{
			try
			{
				if (this.session.State != RemoteDesktopSessionState.Stopped && this.session.State != RemoteDesktopSessionState.Stopping)
					await this.rdpClient.StopSessionAsync(this.session.RemoteJid, this.session.SessionId);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		
			this.Node?.ViewClosed();
		}

		public TreeNode Node => this.node;
		public XmppClient Client => this.client;
		public RemoteDesktopClient RdpClient => this.rdpClient;
		public RemoteDesktopSession Session => this.session;

		public void NewButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Refresh screen?
		}

		public void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: screen capture?
		}

		public void SaveAsButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: screen capture?
		}

		public void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: ?
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			// TODO: ?
		}

	}
}
