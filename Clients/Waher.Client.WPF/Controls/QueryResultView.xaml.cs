using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Things.Queries;
using Waher.Client.WPF.Controls.Chat;
using Waher.Client.WPF.Model;
using Waher.Client.WPF.Model.Concentrator;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for QueryResultView.xaml
	/// </summary>
	public partial class QueryResultView : UserControl, ITabView
	{
		private Node node;
		private NodeQuery query;
		private TextBlock headerLabel;

		public QueryResultView(Node Node, NodeQuery Query, TextBlock HeaderLabel)
		{
			this.node = Node;
			this.query = Query;
			this.headerLabel = HeaderLabel;

			this.query.Aborted += Query_Aborted;
			this.query.EventMessageReceived += Query_EventMessageReceived;
			this.query.NewTitle += Query_NewTitle;
			this.query.SectionAdded += Query_SectionAdded;
			this.query.SectionCompleted += Query_SectionCompleted;
			this.query.Started += Query_Started;
			this.query.Done += Query_Done;
			this.query.StatusMessageReceived += Query_StatusMessageReceived;
			this.query.TableAdded += Query_TableAdded;
			this.query.TableCompleted += Query_TableCompleted;
			this.query.TableUpdated += Query_TableUpdated;

			InitializeComponent();
		}

		private void StatusMessage(string Message)
		{
			this.Dispatcher.BeginInvoke(new ThreadStart(() =>
			{
				this.Status.Content = Message;
			}));
		}

		private void Query_Started(object Sender, NodeQueryEventArgs e)
		{
			this.StatusMessage("Execution started.");
		}

		private void Query_Done(object Sender, NodeQueryEventArgs e)
		{
			this.StatusMessage("Execution completed.");
		}

		private void Query_Aborted(object Sender, NodeQueryEventArgs e)
		{
			this.StatusMessage("Execution aborted.");
		}

		private void Query_StatusMessageReceived(object Sender, NodeQueryStatusMessageEventArgs e)
		{
			this.StatusMessage(e.StatusMessage);
		}

		private void Query_EventMessageReceived(object Sender, NodeQueryEventMessageEventArgs e)
		{
			this.StatusMessage(e.EventMessage);
		}

		private void Query_NewTitle(object Sender, NodeQueryEventArgs e)
		{
			this.Dispatcher.BeginInvoke(new ThreadStart(() =>
			{
				this.headerLabel.Text = this.query.Title;
			}));
		}

		private void Query_TableAdded(object Sender, NodeQueryTableEventArgs e)
		{
			// TODO
		}

		private void Query_TableUpdated(object Sender, NodeQueryTableEventArgs e)
		{
			// TODO
		}

		private void Query_TableCompleted(object Sender, NodeQueryTableEventArgs e)
		{
			// TODO
		}

		private void Query_SectionAdded(object Sender, NodeQuerySectionEventArgs e)
		{
			// TODO
		}

		private void Query_SectionCompleted(object Sender, NodeQuerySectionEventArgs e)
		{
			// TODO
		}

		public void Dispose()
		{
			if (this.query != null)
			{
				this.query.Dispose();
				this.query = null;
			}
		}

		public void NewButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void SaveAsButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

		public void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

		public Node Node
		{
			get { return this.node; }
		}

		public NodeQuery Query
		{
			get { return this.query; }
		}

	}
}
