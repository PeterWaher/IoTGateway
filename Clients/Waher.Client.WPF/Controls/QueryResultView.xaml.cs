using System;
using System.Data;
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
		private readonly Dictionary<string, (DataTable, Column[], ListView)> tables = new Dictionary<string, (DataTable, Column[], ListView)>();
		private readonly LinkedList<ThreadStart> guiQueue = new LinkedList<ThreadStart>();
		private readonly Node node;
		private readonly TextBlock headerLabel;
		private NodeQuery query;
		private StackPanel currentPanel;

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
			this.query.ObjectAdded += Query_ObjectAdded;

			InitializeComponent();

			this.currentPanel = this.ReportPanel;
		}

		private void UpdateGui(ThreadStart P)
		{
			bool Call;

			lock (this.guiQueue)
			{
				Call = this.guiQueue.First is null;
				this.guiQueue.AddLast(P);
			}

			if (Call)
				MainWindow.UpdateGui(this.UpdateGuiSta);
		}

		private void UpdateGuiSta()
		{
			ThreadStart P;
			bool More;

			do
			{
				lock (this.guiQueue)
				{
					if (this.guiQueue.First is null)
						return;

					P = this.guiQueue.First.Value;
					this.guiQueue.RemoveFirst();
					More = this.guiQueue.First != null;
				}

				try
				{
					P();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
			while (More);
		}

		private void StatusMessage(string Message)
		{
			this.UpdateGui(new ThreadStart(() =>
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
			this.UpdateGui(new ThreadStart(() =>
			{
				this.headerLabel.Text = this.query.Title;
			}));
		}

		private void Query_SectionAdded(object Sender, NodeQuerySectionEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				StackPanel Section = new StackPanel()
				{
					Margin = new Thickness(16, 8, 16, 8)
				};

				this.currentPanel.Children.Add(Section);
				this.currentPanel = Section;

				Section.Children.Add(new TextBlock()
				{
					Text = e.Section.Header,
					FontSize = 20,
					FontWeight = FontWeights.Bold,
					Margin = new Thickness(0, 0, 0, 12)
				});
			}));
		}

		private void Query_SectionCompleted(object Sender, NodeQuerySectionEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.currentPanel = this.currentPanel.Parent as StackPanel;
				if (this.currentPanel is null)
					this.currentPanel = this.ReportPanel;
			}));
		}

		private void Query_TableAdded(object Sender, NodeQueryTableEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				try
				{
					if (!this.tables.ContainsKey(e.Table.TableDefinition.TableId))
					{
						DataTable Table = new DataTable(e.Table.TableDefinition.Name);
						GridView GridView = new GridView();

						foreach (Column Column in e.Table.TableDefinition.Columns)
						{
							Table.Columns.Add(Column.ColumnId);

							// TODO: Alignment 

							GridView.Columns.Add(new GridViewColumn()
							{
								Header = Column.Header,
								DisplayMemberBinding = new Binding(Column.ColumnId)
							});
						}

						ListView TableView = new ListView()
						{
							ItemsSource = Table.DefaultView,
							View = GridView
						};

						this.tables[e.Table.TableDefinition.TableId] = (Table, e.Table.TableDefinition.Columns, TableView);

						this.currentPanel.Children.Add(TableView);
					}
				}
				catch (Exception ex)
				{
					this.StatusMessage(ex.Message);
				}
			}));
		}

		private void Query_TableUpdated(object Sender, NodeQueryTableUpdatedEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				if (this.tables.TryGetValue(e.Table.TableDefinition.TableId, out (DataTable, Column[], ListView) P))
				{
					DataTable Table = P.Item1;
					Column[] Columns = P.Item2;
					ListView TableView = P.Item3;
					Column Column;
					object Obj;
					int i, c = Columns.Length;
					int d;

					foreach (Record Record in e.NewRecords)
					{
						DataRow Row = Table.NewRow();

						d = Math.Min(c, Record.Elements.Length);
						for (i = 0; i < d; i++)
						{
							Obj = Record.Elements[i];
							if (Obj is null)
								continue;

							Column = Columns[i];

							if (Obj is bool b)
								Row[Column.ColumnId] = b ? "✓" : string.Empty;
							/*else if (Obj is SKColor)	TODO
							{
							}*/
							else if (Obj is double dbl)
							{
								if (Column.NrDecimals.HasValue)
									Row[Column.ColumnId] = dbl.ToString("F" + Column.NrDecimals.Value.ToString());
								else
									Row[Column.ColumnId] = dbl.ToString();

							}
							/*else if (Obj is Image)	TODO
							{
							}*/
							else
								Row[Column.ColumnId] = Obj.ToString();
						}

						Table.Rows.Add(Row);
					}

					//Table.AcceptChanges();
				}
			}));
		}

		private void Query_TableCompleted(object Sender, NodeQueryTableEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.tables.Remove(e.Table.TableDefinition.TableId);
			}));
		}

		private void Query_ObjectAdded(object Sender, NodeQueryObjectEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				object Obj = e.Object.Object;
				if (Obj is null)
					return;

				// TODO: Images

				this.currentPanel.Children.Add(new TextBlock()
				{
					Text = Obj.ToString(),
					Margin = new Thickness(0, 0, 0, 6)
				});
			}));
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
