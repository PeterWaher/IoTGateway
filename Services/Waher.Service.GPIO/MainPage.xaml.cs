using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Waher.Events;
using Waher.Mock;

namespace Waher.Service.GPIO
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private static ListViewSniffer sniffer = null;
		private static ListViewEventSink eventSink = null;
		private static MainPage instance = null;

		public MainPage()
		{
			this.InitializeComponent();

			if (sniffer is null)
				sniffer = new ListViewSniffer(this.SnifferListView);

			if (eventSink is null)
			{
				eventSink = new ListViewEventSink("List View Event Sink.", this.EventsListView);
				Log.Register(eventSink);
			}

			if (instance is null)
				instance = this;

			App.OwnershipChanged += App_OwnershipChanged;
		}

		private void App_OwnershipChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(App.OwnerJid))
			{
				this.OwnerLabel.Visibility = Visibility.Collapsed;
				this.Owner.Visibility = Visibility.Collapsed;
				this.QrCodeLabel.Visibility = Visibility.Visible;
				this.QrCode.Source = new BitmapImage(new Uri(App.QrCodeUrl));
				this.QrCode.Visibility = Visibility.Visible;
			}
			else
			{
				this.OwnerLabel.Visibility = Visibility.Visible;
				this.Owner.Text = App.OwnerJid;
				this.Owner.Visibility = Visibility.Visible;
				this.QrCodeLabel.Visibility = Visibility.Collapsed;
				this.QrCode.Visibility = Visibility.Collapsed;
			}
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			if (sniffer != null && sniffer.ListView == this.SnifferListView)
				sniffer = null;

			if (eventSink != null && eventSink.ListView == this.EventsListView)
			{
				Log.Unregister(eventSink);
				eventSink.Dispose();
				eventSink = null;
			}

			if (instance == this)
				instance = null;
		}

		public static ListViewSniffer Sniffer
		{
			get { return sniffer; }
		}

		public static MainPage Instance
		{
			get { return instance; }
		}

		private int currentRow = 0;

		public KeyValuePair<TextBlock, TextBlock> AddPin(string PinName, Enum Drive, string Value)
		{
			TextBlock Cell1 = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 0, 0, 5)
			};

			Cell1.Text = PinName;
			Grid.SetColumn(Cell1, 0);
			Grid.SetRow(Cell1, ++this.currentRow);

			this.GpioGrid.RowDefinitions.Add(new RowDefinition());
			this.GpioGrid.Children.Add(Cell1);

			TextBlock Cell2 = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 0, 0, 5)
			};

			Cell2.Text = Drive.ToString();
			Grid.SetColumn(Cell2, 1);
			Grid.SetRow(Cell2, this.currentRow);

			this.GpioGrid.Children.Add(Cell2);

			TextBlock Cell3 = new TextBlock()
			{
				TextWrapping = TextWrapping.Wrap,
				Margin = new Thickness(0, 0, 0, 5)
			};

			Cell3.Text = Value;
			Grid.SetColumn(Cell3, 2);
			Grid.SetRow(Cell3, this.currentRow);

			this.GpioGrid.Children.Add(Cell3);

			return new KeyValuePair<TextBlock, TextBlock>(Cell2, Cell3);
		}

		public async void UpdateValue(TextBlock Block, string Value)
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Block.Text = Value);
		}
	}
}
