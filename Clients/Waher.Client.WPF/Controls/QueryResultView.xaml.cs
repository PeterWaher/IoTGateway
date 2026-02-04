using Microsoft.Win32;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using Waher.Client.WPF.Controls.Report;
using Waher.Client.WPF.Model.Concentrator;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Wpf;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Script.Graphs;
using Waher.Things.Queries;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for QueryResultView.xaml
	/// </summary>
	public partial class QueryResultView : UserControl, ITabView
	{
		private readonly Dictionary<string, (DataTable, Column[], ListView)> tables = [];
		private readonly LinkedList<ThreadStart> guiQueue = [];
		private readonly LinkedList<ReportElement> elements = [];
		private readonly TextBlock headerLabel;
		private Node node;
		private NodeQuery query;
		private StackPanel currentPanel;

		private QueryResultView(Node Node, NodeQuery Query, TextBlock HeaderLabel)
		{
			this.node = Node;
			this.query = Query;
			this.headerLabel = HeaderLabel;
		
			this.InitializeComponent();
		}

		public static async Task<QueryResultView> CreateAsync(Node Node, NodeQuery Query, TextBlock HeaderLabel)
		{
			QueryResultView Result = new(Node, Query, HeaderLabel);

			if (Result.query is not null)
			{
				Result.query.Aborted += Result.Query_Aborted;
				Result.query.EventMessageReceived += Result.Query_EventMessageReceived;
				Result.query.NewTitle += Result.Query_NewTitle;
				Result.query.SectionAdded += Result.Query_SectionAdded;
				Result.query.SectionCompleted += Result.Query_SectionCompleted;
				Result.query.Started += Result.Query_Started;
				Result.query.Done += Result.Query_Done;
				Result.query.StatusMessageReceived += Result.Query_StatusMessageReceived;
				Result.query.TableAdded += Result.Query_TableAdded;
				Result.query.TableCompleted += Result.Query_TableCompleted;
				Result.query.TableUpdated += Result.Query_TableUpdated;
				Result.query.ObjectAdded += Result.Query_ObjectAdded;

				await Result.query.ResumeAsync();
			}

			Result.currentPanel = Result.ReportPanel;

			return Result;
		}

		public Node Node => this.node;
		public NodeQuery Query => this.query;

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

		private Task UpdateGuiSta()
		{
			ThreadStart P;
			bool More;

			do
			{
				lock (this.guiQueue)
				{
					if (this.guiQueue.First is null)
						return Task.CompletedTask;

					P = this.guiQueue.First.Value;
					this.guiQueue.RemoveFirst();
					More = this.guiQueue.First is not null;
				}

				try
				{
					P();
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
			while (More);
	
			return Task.CompletedTask;
		}

		private void StatusMessage(string Message)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.Status.Content = Message;
			}));
		}

		private Task Query_Started(object Sender, NodeQueryEventArgs e)
		{
			this.StatusMessage("Execution started.");
			return Task.CompletedTask;
		}

		private Task Query_Done(object Sender, NodeQueryEventArgs e)
		{
			this.StatusMessage("Execution completed.");
			return Task.CompletedTask;
		}

		private Task Query_Aborted(object Sender, NodeQueryEventArgs e)
		{
			this.StatusMessage("Execution aborted.");
			return Task.CompletedTask;
		}

		private Task Query_StatusMessageReceived(object Sender, NodeQueryStatusMessageEventArgs e)
		{
			this.StatusMessage(e.StatusMessage);
			return Task.CompletedTask;
		}

		private Task Query_EventMessageReceived(object Sender, NodeQueryEventMessageEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.Add(new ReportEvent(e.EventType, e.EventLevel, e.EventMessage));
			}));

			return Task.CompletedTask;
		}

		private void Add(ReportEvent Event)
		{
			lock (this.elements)
			{
				this.elements.AddLast(Event);
			}

			Brush FgColor;
			Brush BgColor;

			switch (Event.EventType)
			{
				case QueryEventType.Information:
				default:
					FgColor = Brushes.Black;
					BgColor = Brushes.White;
					break;

				case QueryEventType.Warning:
					FgColor = Brushes.Black;
					BgColor = Brushes.Yellow;
					break;

				case QueryEventType.Error:
					FgColor = Brushes.Yellow;
					BgColor = Brushes.Red;
					break;

				case QueryEventType.Exception:
					FgColor = Brushes.Yellow;
					BgColor = Brushes.DarkRed;
					break;
			}

			this.currentPanel.Children.Add(new TextBlock()
			{
				Text = Event.EventMessage,
				Margin = new Thickness(0, 0, 0, 6),
				Foreground = FgColor,
				Background = BgColor,
				FontFamily = new FontFamily("Courier New")
			});
		}

		private Task Query_NewTitle(object Sender, NodeQueryEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.headerLabel.Text = this.query.Title;
			}));

			return Task.CompletedTask;
		}

		private Task Query_SectionAdded(object Sender, NodeQuerySectionEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.Add(new ReportSectionCreated(e.Section.Header));
			}));

			return Task.CompletedTask;
		}

		private void Add(ReportSectionCreated Event)
		{
			lock (this.elements)
			{
				this.elements.AddLast(Event);
			}

			StackPanel Section = new()
			{
				Margin = new Thickness(16, 8, 16, 8)
			};

			this.currentPanel.Children.Add(Section);
			this.currentPanel = Section;

			Section.Children.Add(new TextBlock()
			{
				Text = Event.Header,
				FontSize = 20,
				FontWeight = FontWeights.Bold,
				Margin = new Thickness(0, 0, 0, 12)
			});
		}

		private Task Query_SectionCompleted(object Sender, NodeQuerySectionEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.Add(new ReportSectionCompleted());
			}));

			return Task.CompletedTask;
		}

		private void Add(ReportSectionCompleted Event)
		{
			lock (this.elements)
			{
				this.elements.AddLast(Event);
			}

			this.currentPanel = this.currentPanel.Parent as StackPanel;
			this.currentPanel ??= this.ReportPanel;
		}

		private Task Query_TableAdded(object Sender, NodeQueryTableEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				Table Table = e.Table.TableDefinition;
				this.Add(new ReportTableCreated(Table.TableId, Table.Name, Table.Columns));
			}));

			return Task.CompletedTask;
		}

		private void Add(ReportTableCreated Event)
		{
			lock (this.elements)
			{
				this.elements.AddLast(Event);
			}

			try
			{
				if (!this.tables.ContainsKey(Event.TableId))
				{
					DataTable Table = new(Event.Name);
					GridView GridView = new();

					foreach (Column Column in Event.Columns)
					{
						Table.Columns.Add(Column.ColumnId);

						// TODO: Alignment 

						GridView.Columns.Add(new GridViewColumn()
						{
							Header = Column.Header,
							DisplayMemberBinding = new Binding(Column.ColumnId)
						});
					}

					ListView TableView = new()
					{
						ItemsSource = Table.DefaultView,
						View = GridView
					};

					this.tables[Event.TableId] = (Table, Event.Columns, TableView);

					this.currentPanel.Children.Add(TableView);
				}
			}
			catch (Exception ex)
			{
				this.StatusMessage(ex.Message);
			}
		}

		private Task Query_TableUpdated(object Sender, NodeQueryTableUpdatedEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				if (this.tables.TryGetValue(e.Table.TableDefinition.TableId, out (DataTable, Column[], ListView) P))
					this.Add(new ReportTableRecords(e.Table.TableDefinition.TableId, e.NewRecords, P.Item2));
			}));

			return Task.CompletedTask;
		}

		private void Add(ReportTableRecords Event)
		{
			if (this.tables.TryGetValue(Event.TableId, out (DataTable, Column[], ListView) P))
			{
				lock (this.elements)
				{
					this.elements.AddLast(Event);
				}

				DataTable Table = P.Item1;
				Column[] Columns = P.Item2;
				Column Column;
				object Obj;
				int i, c = Columns.Length;
				int d;

				foreach (Record Record in Event.Records)
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
						else if (Obj is decimal dec)
						{
							if (Column.NrDecimals.HasValue)
								Row[Column.ColumnId] = dec.ToString("F" + Column.NrDecimals.Value.ToString());
							else
								Row[Column.ColumnId] = dec.ToString();
						}
						else if (Obj is float f)
						{
							if (Column.NrDecimals.HasValue)
								Row[Column.ColumnId] = f.ToString("F" + Column.NrDecimals.Value.ToString());
							else
								Row[Column.ColumnId] = f.ToString();
						}
						else if (Obj is DateTime DT)
						{
							if (DT.TimeOfDay == TimeSpan.Zero)
								Row[Column.ColumnId] = DT.ToShortDateString();
							else
								Row[Column.ColumnId] = DT.ToShortDateString() + ", " + DT.ToLongTimeString();
						}
						/*else if (Obj is Image)	TODO
						{
						}*/
						else
							Row[Column.ColumnId] = Obj.ToString();
					}

					Table.Rows.Add(Row);
				}
			}

			//Table.AcceptChanges();
		}

		private Task Query_TableCompleted(object Sender, NodeQueryTableEventArgs e)
		{
			this.UpdateGui(new ThreadStart(() =>
			{
				this.Add(new ReportTableCompleted(e.Table.TableDefinition.TableId));
			}));

			return Task.CompletedTask;
		}

		private void Add(ReportTableCompleted Event)
		{
			lock (this.elements)
			{
				this.elements.AddLast(Event);
			}

			this.tables.Remove(Event.TableId);
		}

		private Task Query_ObjectAdded(object Sender, NodeQueryObjectEventArgs e)
		{
			object Obj = e.Object.Object;
			if (Obj is null)
				return Task.CompletedTask;

			this.UpdateGui(new ThreadStart(async () =>
			{
				try
				{
					await this.Add(new ReportObject(Obj, e.Object.Binary, e.Object.ContentType));
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}));

			return Task.CompletedTask;
		}

		private async Task Add(ReportObject Event)
		{
			lock (this.elements)
			{
				this.elements.AddLast(Event);
			}

			if (Event.Object is SKImage Image)
			{
				PixelInformation Pixels = PixelInformation.FromImage(Image);

				BitmapImage BitmapImage;
				byte[] Bin = Pixels.EncodeAsPng();

				using (MemoryStream ms = new(Bin))
				{
					BitmapImage = new BitmapImage();
					BitmapImage.BeginInit();
					BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					BitmapImage.StreamSource = ms;
					BitmapImage.EndInit();
				}

				this.currentPanel.Children.Add(new Image()
				{
					Source = BitmapImage,
					Width = Pixels.Width,
					Height = Pixels.Height
				});
			}
			else if (Event.Object is MarkdownDocument Markdown)
			{
				string Xaml = await Markdown.GenerateXAML();
				object Parsed = XamlReader.Parse(Xaml);

				if (Parsed is UIElement Element)
					this.currentPanel.Children.Add(Element);
				else
				{
					this.currentPanel.Children.Add(new TextBlock()
					{
						Text = Parsed?.ToString() ?? string.Empty,
						Margin = new Thickness(0, 0, 0, 6)
					});
				}
			}
			else
			{
				this.currentPanel.Children.Add(new TextBlock()
				{
					Text = Event.Object.ToString(),
					Margin = new Thickness(0, 0, 0, 6)
				});
			}
		}

		public void Dispose()
		{
			this.query?.Dispose();
			this.query = null;

			GC.SuppressFinalize(this);
		}

		public void NewButton_Click(object Sender, RoutedEventArgs e)
		{
			this.tables.Clear();
			this.guiQueue.Clear();
			this.elements.Clear();
			this.node = null;
			this.query = null;
			this.currentPanel = null;

			this.ReportPanel.Children.Clear();
			this.currentPanel = this.ReportPanel;
		}

		public void SaveButton_Click(object Sender, RoutedEventArgs e)
		{
			this.SaveAsButton_Click(Sender, e);
		}

		public void SaveAsButton_Click(object Sender, RoutedEventArgs e)
		{
			SaveFileDialog Dialog = new()
			{
				AddExtension = true,
				CheckPathExists = true,
				CreatePrompt = false,
				DefaultExt = "xml",
				Filter = "XML Files (*.xml)|*.xml|HTML Files (*.html,*.htm)|*.html,*.htm|All Files (*.*)|*.*",
				Title = "Save report file"
			};

			bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

			if (Result.HasValue && Result.Value)
			{
				try
				{
					if (Dialog.FilterIndex == 2)
					{
						StringBuilder Xml = new();
						using (XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(true, true)))
						{
							this.SaveAsXml(w);
						}

						string Html = XSL.Transform(Xml.ToString(), reportToHtml);

						File.WriteAllText(Dialog.FileName, Html, Encoding.UTF8);
					}
					else
					{
						using FileStream f = File.Create(Dialog.FileName);
						using XmlWriter w = XmlWriter.Create(f, XML.WriterSettings(true, false));
						
						this.SaveAsXml(w);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(MainWindow.FindWindow(this), ex.Message, "Unable to save file.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private static readonly XslCompiledTransform reportToHtml = XSL.LoadTransform("Waher.Client.WPF.Transforms.ReportToHTML.xslt");
		private static readonly XmlSchema schema = XSL.LoadSchema(typeof(MainWindow).Namespace + ".Schema.Report.xsd");
		private const string reportNamespace = "http://waher.se/Schema/Report.xsd";
		private const string reportRoot = "Report";

		private void SaveAsXml(XmlWriter w)
		{
			w.WriteStartElement(reportRoot, reportNamespace);
			w.WriteAttributeString("title", this.headerLabel.Text);

			foreach (ReportElement Item in this.elements)
				Item.ExportXml(w);

			w.WriteEndElement();
			w.Flush();
		}

		public void OpenButton_Click(object Sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog Dialog = new()
				{
					AddExtension = true,
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = "xml",
					Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
					Multiselect = false,
					ShowReadOnly = true,
					Title = "Open report file"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

				if (Result.HasValue && Result.Value)
				{
					XmlDocument Xml = XML.LoadFromFile(Dialog.FileName, true);
					this.Load(Xml, Dialog.FileName);
				}
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MessageBox.Show(ex.Message, "Unable to load file.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public async void Load(XmlDocument Xml, string FileName)
		{
			try
			{
				XSL.Validate(FileName, Xml, reportRoot, reportNamespace, schema);

				this.NewButton_Click(null, null);
				this.headerLabel.Text = XML.Attribute(Xml.DocumentElement, "title");

				Dictionary<string, Column[]> ColumnsByTableId = [];

				foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
				{
					if (N is not XmlElement E)
						continue;

					switch (E.LocalName)
					{
						case "Event":
							this.Add(new ReportEvent(E));
							break;

						case "Object":
							await this.Add(await ReportObject.CreateAsync(E));
							break;

						case "SectionStart":
							this.Add(new ReportSectionCreated(E));
							break;

						case "SectionEnd":
							this.Add(new ReportSectionCompleted());
							break;

						case "TableStart":
							ReportTableCreated Table = new(E);
							ColumnsByTableId[Table.TableId] = Table.Columns;

							this.Add(Table);
							break;

						case "TableEnd":
							this.Add(new ReportTableCompleted(E));
							break;

						case "Records":
							this.Add(new ReportTableRecords(E, ColumnsByTableId));
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

	}
}
