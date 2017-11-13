using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for SensorDataView.xaml
	/// </summary>
	public partial class SearchResultView : UserControl, ITabView
	{
		private Field[] headers;
		private Dictionary<string, string>[] records;
		private DataTable table;

		public SearchResultView()
		{
			this.headers = null;
			this.records = null;
			this.table = null;
		}

		public SearchResultView(Field[] Headers, Dictionary<string, string>[] Records)
		{
			this.Init(Headers, Records);
		}

		private void Init(Field[] Headers, Dictionary<string, string>[] Records)
		{
			this.headers = Headers;
			this.records = Records;

			InitializeComponent();

			if (this.table != null)
			{
				this.table.Dispose();
				this.table = null;
			}

			this.table = new DataTable("SearchResult");

			foreach (Field Header in Headers)
				this.table.Columns.Add(Header.Var);

			foreach (Dictionary<string, string> Record in Records)
			{
				DataRow Row = this.table.NewRow();

				foreach (KeyValuePair<string, string> P in Record)
					Row[P.Key] = P.Value;

				this.table.Rows.Add(Row);
			}

			this.table.AcceptChanges();

			this.SearchResultListView.ItemsSource = this.table.DefaultView;

			if (this.SearchResultListView.View is GridView GridView)
			{
				GridView.Columns.Clear();

				foreach (Field Header in this.headers)
				{
					GridView.Columns.Add(new GridViewColumn()
					{
						Header = Header.Label,
						DisplayMemberBinding = new Binding(Header.Var)
					});
				}
			}
		}

		public void Dispose()
		{
			if (this.table != null)
			{
				this.table.Dispose();
				this.table = null;
			}
		}

		public void NewButton_Click(object sender, RoutedEventArgs e)
		{
			this.table.Clear();
		}

		public void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			this.SaveAsButton_Click(sender, e);
		}

		public void SaveAsButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog Dialog = new SaveFileDialog()
			{
				AddExtension = true,
				CheckPathExists = true,
				CreatePrompt = false,
				DefaultExt = "html",
				Filter = "XML Files (*.xml)|*.xml|HTML Files (*.html,*.htm)|*.html,*.htm|All Files (*.*)|*.*",
				Title = "Save Search Result"
			};

			bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

			if (Result.HasValue && Result.Value)
			{
				try
				{
					if (Dialog.FilterIndex == 2)
					{
						StringBuilder Xml = new StringBuilder();
						using (XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(true, true)))
						{
							this.SaveAsXml(w);
						}

						string Html = XSL.Transform(Xml.ToString(), searchResultToHtml);

						File.WriteAllText(Dialog.FileName, Html, System.Text.Encoding.UTF8);
					}
					else
					{
						using (FileStream f = File.Create(Dialog.FileName))
						{
							using (XmlWriter w = XmlWriter.Create(f, XML.WriterSettings(true, false)))
							{
								this.SaveAsXml(w);
							}
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(MainWindow.FindWindow(this), ex.Message, "Unable to save file.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private static readonly XslCompiledTransform searchResultToHtml = XSL.LoadTransform("Waher.Client.WPF.Transforms.SearchResultToHTML.xslt");
		private static readonly XmlSchema schema = XSL.LoadSchema("Waher.Client.WPF.Schema.SearchResult.xsd");
		private const string searchResultNamespace = "http://waher.se/Schema/SearchResult.xsd";
		private const string searchResultRoot = "SearchResult";

		private void SaveAsXml(XmlWriter w)
		{
			w.WriteStartElement(searchResultRoot, searchResultNamespace);
			w.WriteStartElement("Headers");

			foreach (Field Header in this.headers)
			{
				w.WriteStartElement("Header");
				w.WriteAttributeString("var", Header.Var);
				w.WriteAttributeString("label", Header.Label);
				w.WriteEndElement();
			}

			w.WriteEndElement();
			w.WriteStartElement("Records");

			foreach (Dictionary<string, string> Record in this.records)
			{
				w.WriteStartElement("Record");

				foreach (KeyValuePair<string, string> Field in Record)
				{
					w.WriteStartElement("Field");
					w.WriteAttributeString("var", Field.Key);
					w.WriteAttributeString("value", Field.Value);
					w.WriteEndElement();
				}

				w.WriteEndElement();
			}

			w.WriteEndElement();
			w.WriteEndElement();
			w.Flush();
		}

		public void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog Dialog = new OpenFileDialog()
				{
					AddExtension = true,
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = "xml",
					Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
					Multiselect = false,
					ShowReadOnly = true,
					Title = "Open Search Result"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

				if (Result.HasValue && Result.Value)
				{
					XmlDocument Xml = new XmlDocument();
					Xml.Load(Dialog.FileName);

					this.Load(Xml, Dialog.FileName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Unable to load file.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Load(XmlDocument Xml, string FileName)
		{
			XSL.Validate(FileName, Xml, searchResultRoot, searchResultNamespace, schema);

			List<Field> Headers = new List<Field>();
			List<Dictionary<string, string>> Records = new List<Dictionary<string, string>>();

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "Headers":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "Header")
								{
									string Var = XML.Attribute(E2, "var");
									string Label = XML.Attribute(E2, "label");

									Headers.Add(new TextSingleField(null, Var, Label, false, null, null, string.Empty,
										new StringDataType(), new BasicValidation(), string.Empty, false, false, false));
								}
							}
							break;

						case "Records":
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "Record")
								{
									Dictionary<string, string> Record = new Dictionary<string, string>();

									foreach (XmlNode N3 in E2.ChildNodes)
									{
										if (N3 is XmlElement E3 && E3.LocalName == "Field")
										{
											string Var = XML.Attribute(E3, "var");
											string Value = XML.Attribute(E3, "value");

											Record[Var] = Value;
										}
									}

									Records.Add(Record);
								}
							}
							break;
					}
				}
			}

			this.Init(Headers.ToArray(), Records.ToArray());
		}
	}
}
