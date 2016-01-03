using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
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
using Waher.Networking;
using Waher.Client.WPF.Controls.SensorData;
using Waher.Networking.XMPP.Sensor;
using Waher.Things;
using Waher.Things.SensorData;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for SensorDataView.xaml
	/// </summary>
	public partial class SensorDataView : UserControl, IDisposable
	{
		private SensorDataClientRequest request;
		private TreeNode node;
		private Window owner;
		private Dictionary<string, bool> nodes = new Dictionary<string, bool>();
		private Dictionary<string, bool> failed = new Dictionary<string, bool>();

		public SensorDataView(SensorDataClientRequest Request, TreeNode Node, Window Owner)
		{
			this.request = Request;
			this.node = Node;
			this.owner = Owner;

			InitializeComponent();

			this.request.OnStateChanged += new SensorDataReadoutStateChangedEventHandler(request_OnStateChanged);
			this.request.OnFieldsReceived += new SensorDataReadoutFieldsReportedEventHandler(request_OnFieldsReceived);
			this.request.OnErrorsReceived += new SensorDataReadoutErrorsReportedEventHandler(request_OnErrorsReceived);
		}

		public void Dispose()
		{
		}

		public TreeNode Node
		{
			get { return this.node; }
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			GridView GridView = this.SensorDataListView.View as GridView;
			if (GridView != null)
			{
				GridView.Columns[2].Width = this.ActualWidth - GridView.Columns[0].ActualWidth - GridView.Columns[1].ActualWidth -
					GridView.Columns[3].ActualWidth - GridView.Columns[4].ActualWidth - GridView.Columns[5].ActualWidth -
					SystemParameters.VerticalScrollBarWidth - 8;
			}
		}

		private void request_OnErrorsReceived(object Sender, IEnumerable<ThingError> NewErrors)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.OnErrorsReceived), NewErrors);
		}

		private void OnErrorsReceived(object P)
		{
			IEnumerable<ThingError> NewErrors = (IEnumerable<ThingError>)P;
			string Key;

			lock (this.failed)
			{
				foreach (ThingError Error in NewErrors)
				{
					Key = Error.Key;
					this.failed[Key] = true;
					this.nodes[Key] = true;
				}

				this.NodesFailedLabel.Content = this.failed.Count.ToString();
				this.NodesTotalLabel.Content = this.nodes.Count.ToString();
			}

			// TODO: Display errors.
		}

		private void request_OnFieldsReceived(object Sender, IEnumerable<Field> NewFields)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.OnFieldsReceived), NewFields);
		}

		private void OnFieldsReceived(object P)
		{
			IEnumerable<Field> NewFields = (IEnumerable<Field>)P;
			string LastKey = null;
			string Key;
			FieldQoS QoS;
			Color Fg, Bg;

			lock (this.nodes)
			{
				foreach (Field Field in NewFields)
				{
					Key = Field.Thing.Key;
					if (LastKey == null || Key != LastKey)
					{
						LastKey = Key;
						this.nodes[Key] = true;
					}

					QoS = Field.QoS;
					Fg = Colors.Black;
					Bg = Colors.White;

					if (QoS.HasFlag(FieldQoS.InvoiceConfirmed) || QoS.HasFlag(FieldQoS.Invoiced))
						Bg = Colors.Gold;
					else if (QoS.HasFlag(FieldQoS.EndOfSeries))
						Bg = Colors.LightBlue;
					else if (QoS.HasFlag(FieldQoS.Signed))
						Bg = Colors.LightGreen;
					else if (QoS.HasFlag(FieldQoS.Error))
						Bg = Colors.LightPink;
					else if (QoS.HasFlag(FieldQoS.PowerFailure) || QoS.HasFlag(FieldQoS.TimeOffset) || QoS.HasFlag(FieldQoS.Warning))
						Bg = Colors.LightYellow;
					else if (QoS.HasFlag(FieldQoS.Missing) || QoS.HasFlag(FieldQoS.InProgress))
						Bg = Colors.LightGray;
					else if (QoS.HasFlag(FieldQoS.AutomaticEstimate) || QoS.HasFlag(FieldQoS.ManualEstimate))
						Bg = Colors.WhiteSmoke;

					this.SensorDataListView.Items.Add(new FieldItem(Field, Fg, Bg));
				}

				this.NodesTotalLabel.Content = this.nodes.Count.ToString();
				this.FieldsLabel.Content = this.SensorDataListView.Items.Count.ToString();
			}
		}

		private void request_OnStateChanged(object Sender, SensorDataReadoutState NewState)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.OnStateChanged), NewState);
		}

		private void OnStateChanged(object P)
		{
			SensorDataReadoutState NewState = (SensorDataReadoutState)P;

			switch (NewState)
			{
				case SensorDataReadoutState.Requested:
					this.StateLabel.Content = "Requested";
					break;

				case SensorDataReadoutState.Accepted:
					this.StateLabel.Content = "Accepted";
					break;

				case SensorDataReadoutState.Started:
					this.StateLabel.Content = "Started";
					break;

				case SensorDataReadoutState.Receiving:
					this.StateLabel.Content = "Receiving";
					break;

				case SensorDataReadoutState.Done:
					this.StateLabel.Content = "Done";
					this.Done();
					break;

				case SensorDataReadoutState.Cancelled:
					this.StateLabel.Content = "Cancelled";
					break;

				case SensorDataReadoutState.Failure:
					this.StateLabel.Content = "Failure";
					this.Done();
					break;

				default:
					this.StateLabel.Content = NewState.ToString();
					break;
			}
		}

		private void Done()
		{
			lock (this.nodes)
			{
				this.NodesTotalLabel.Content = this.nodes.Count;
				this.NodesOkLabel.Content = this.nodes.Count - this.failed.Count;
			}
		}

		private void NewButton_Click(object sender, RoutedEventArgs e)
		{
			this.SensorDataListView.Items.Clear();
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog Dialog = new SaveFileDialog();
			Dialog.AddExtension = true;
			Dialog.CheckPathExists = true;
			Dialog.CreatePrompt = false;
			Dialog.DefaultExt = "html";
			Dialog.Filter = "XML Files (*.xml)|*.xml|HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";
			Dialog.Title = "Save Sensor data readout";

			bool? Result = Dialog.ShowDialog(this.owner);

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

						/*string Html = XML.Transform(Xml.ToString(), sensorDataToHtml);

						File.WriteAllText(Dialog.FileName, Html, System.Text.Encoding.UTF8);*/
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
					MessageBox.Show(this.owner, ex.Message, "Unable to save file.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		/*private static readonly XslCompiledTransform sensorDataToHtml = XML.LoadTransform("Waher.Client.WPF.Transforms.SensorDataToHTML.xslt", typeof(SensorDataView).Assembly);
		private static readonly XmlSchema schema = XML.LoadSchema("Waher.Client.WPF.Schema.SensorData.xsd", typeof(SensorDataView).Assembly);*/
		private const string sensorDataNamespace = "http://waher.se/SensorData.xsd";
		private const string sensorDataRoot = "SensorData";

		private void SaveAsXml(XmlWriter w)
		{
			List<Field> Fields = new List<Field>();

			w.WriteStartElement(sensorDataRoot, sensorDataNamespace);

			foreach (FieldItem Item in this.SensorDataListView.Items)
				Fields.Add(Item.Field);

			SensorDataServerRequest.OutputFields(w, Fields, this.request.SeqNr, true, null);

			w.WriteEndElement();
			w.Flush();
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO
		}

	}
}
