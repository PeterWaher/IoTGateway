using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Waher.Events;
using Waher.Mock;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Waher.Mock.Lamp.UWP
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

			if (sniffer == null)
				sniffer = new ListViewSniffer(this.SnifferListView);

			if (eventSink == null)
			{
				eventSink = new ListViewEventSink("List View Event Sink.", this.EventsListView);
				Log.Register(eventSink);
			}

			if (instance == null)
				instance = this;
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
	}
}
