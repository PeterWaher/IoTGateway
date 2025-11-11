using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Waher.Client.WPF.Controls.Logs;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using System.Threading.Tasks;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for LogView.xaml
	/// </summary>
	public partial class LogView : UserControl, ITabView
	{
		private readonly string? identifier;
		private LogSink? sink;

		/// <summary>
		/// Interaction logic for LogView.xaml
		/// </summary>
		/// <param name="Identifier">Identifier</param>
		/// <param name="Register">If the log view should be registered with <see cref="Log"/>.</param>
		public LogView(string? Identifier, bool Register)
		{
			this.identifier = Identifier;

			this.InitializeComponent();

			if (Register)
			{
				this.sink = new LogSink(this);
				Log.Register(this.sink);
			}
		}

		/// <summary>
		/// Identifier of log view.
		/// </summary>
		public string? Identifier => this.identifier;

		/// <summary>
		/// Log sink registered with <see cref="Log"/>, if registered, null otherwise.
		/// </summary>
		public LogSink? Sink => this.sink;

		public void Dispose()
		{
			if (this.sink is not null)
			{
				Log.Unregister(this.sink);
				this.sink = null;
			}
		}

		public void Add(LogItem Item)
		{
			MainWindow.UpdateGui(this.AddItem, Item);
		}

		private Task AddItem(object P)
		{
			this.LogListView.Items.Add(P);
			this.LogListView.ScrollIntoView(P);

			return Task.CompletedTask;
		}

		public void NewButton_Click(object Sender, RoutedEventArgs e)
		{
			this.LogListView.Items.Clear();
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
				Title = "Save log file"
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

						string Html = XSL.Transform(Xml.ToString(), eventsToHtml);

						File.WriteAllText(Dialog.FileName, Html, System.Text.Encoding.UTF8);
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

		private static readonly XslCompiledTransform eventsToHtml = XSL.LoadTransform("Waher.Client.WPF.Transforms.EventXmlToHtml.xslt");
		private static readonly XmlSchema schema = XSL.LoadSchema(typeof(MainWindow).Namespace + ".Schema.EventOutput.xsd");
		private const string logNamespace = "http://waher.se/Schema/EventOutput.xsd";
		private const string logRoot = "EventOutput";

		private void SaveAsXml(XmlWriter w)
		{
			w.WriteStartElement(logRoot, logNamespace);

			foreach (LogItem Item in this.LogListView.Items)
			{
				Event Event = Item.Event;

				w.WriteStartElement(Event.Type.ToString());
				w.WriteAttributeString("timestamp", Encode(Event.Timestamp));
				w.WriteAttributeString("level", Event.Level.ToString());


				if (!string.IsNullOrEmpty(Event.EventId))
					w.WriteAttributeString("id", Event.EventId);

				if (!string.IsNullOrEmpty(Event.Object))
					w.WriteAttributeString("object", Event.Object);

				if (!string.IsNullOrEmpty(Event.Actor))
					w.WriteAttributeString("actor", Event.Actor);

				if (!string.IsNullOrEmpty(Event.Module))
					w.WriteAttributeString("module", Event.Module);

				if (!string.IsNullOrEmpty(Event.Facility))
					w.WriteAttributeString("facility", Event.Facility);

				w.WriteStartElement("Message");

				foreach (string Row in GetRows(Event.Message))
					w.WriteElementString("Row", Row);

				w.WriteEndElement();

				if (Event.Tags is not null && Event.Tags.Length > 0)
				{
					foreach (KeyValuePair<string, object> Tag in Event.Tags)
					{
						w.WriteStartElement("Tag");
						w.WriteAttributeString("key", Tag.Key);

						if (Tag.Value is not null)
							w.WriteAttributeString("value", Tag.Value.ToString());

						w.WriteEndElement();
					}
				}

				if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
				{
					w.WriteStartElement("StackTrace");

					foreach (string Row in GetRows(Event.StackTrace))
						w.WriteElementString("Row", Row);

					w.WriteEndElement();
				}

				w.WriteEndElement();
				w.Flush();
			}

			w.WriteEndElement();
			w.Flush();
		}

		private static string[] GetRows(string s)
		{
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

		internal static string Encode(DateTime DT)
		{
			StringBuilder sb = new();

			sb.Append(DT.Year.ToString("D4"));
			sb.Append('-');
			sb.Append(DT.Month.ToString("D2"));
			sb.Append('-');
			sb.Append(DT.Day.ToString("D2"));
			sb.Append('T');
			sb.Append(DT.Hour.ToString("D2"));
			sb.Append(':');
			sb.Append(DT.Minute.ToString("D2"));
			sb.Append(':');
			sb.Append(DT.Second.ToString("D2"));
			sb.Append('.');
			sb.Append(DT.Millisecond.ToString("D3"));

			if (DT.Kind == DateTimeKind.Utc)
				sb.Append('Z');

			return sb.ToString();
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
					Title = "Open log file"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

				if (Result.HasValue && Result.Value)
				{
					XmlDocument Xml = new()
					{
						PreserveWhitespace = true
					};
					Xml.Load(Dialog.FileName);

					this.Load(Xml, Dialog.FileName);
				}
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MessageBox.Show(ex.Message, "Unable to load file.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Load(XmlDocument Xml, string FileName)
		{
			XmlElement E, E2, E3;

			XSL.Validate(FileName, Xml, logRoot, logNamespace, schema);

			this.LogListView.Items.Clear();

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (!Enum.TryParse(E.LocalName, out EventType Type))
					continue;

				DateTime Timestamp = XML.Attribute(E, "timestamp", DateTime.MinValue);
				EventLevel Level = XML.Attribute(E, "level", EventLevel.Minor);
				string EventId = XML.Attribute(E, "id");
				string Object = XML.Attribute(E, "object");
				string Actor = XML.Attribute(E, "actor");
				string Module = XML.Attribute(E, "module");
				string Facility = XML.Attribute(E, "facility");
				StringBuilder Message = new();
				StringBuilder StackTrace = null;
				List<KeyValuePair<string, object>> Tags = [];

				foreach (XmlNode N2 in E.ChildNodes)
				{
					E2 = N2 as XmlElement;
					if (E2 is null)
						continue;

					switch (E2.LocalName)
					{
						case "Message":
							foreach (XmlNode N3 in E2.ChildNodes)
							{
								E3 = N3 as XmlElement;
								if (E3 is null)
									continue;

								if (E3.LocalName == "Row")
									Message.AppendLine(E3.InnerText);
							}
							break;

						case "Tag":
							string Key = XML.Attribute(E2, "key");
							string Value = XML.Attribute(E2, "value");

							Tags.Add(new KeyValuePair<string, object>(Key, Value));
							break;

						case "StackTrace":
							StackTrace ??= new StringBuilder();

							foreach (XmlNode N3 in E2.ChildNodes)
							{
								E3 = N3 as XmlElement;
								if (E3 is null)
									continue;

								if (E3.LocalName == "Row")
									StackTrace.AppendLine(E3.InnerText);
							}
							break;
					}
				}

				Event Event = new(Timestamp, Type, Message.ToString(), Object, Actor, EventId, Level, Facility, Module,
					StackTrace?.ToString() ?? string.Empty, [.. Tags]);

				this.Add(new LogItem(Event));
			}
		}

		private void UserControl_SizeChanged(object Sender, SizeChangedEventArgs e)
		{
			if (this.LogListView.View is GridView GridView)
			{
				double w = this.ActualWidth;
				int i;

				for (i = 0; i < 6; i++)
					w -= (GridView.Columns[i].ActualWidth + SystemParameters.VerticalScrollBarWidth + 8);

				GridView.Columns[6].Width = Math.Max(w, 10);
			}
		}

	}
}
