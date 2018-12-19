using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Waher.Events;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Waher.IoTGateway.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
		private static MainPage instance = null;
		private Events events;

		public MainPage()
        {
            this.InitializeComponent();

			this.events = new MainPage.Events();
			Log.Register(this.events);

			if (instance is null)
				instance = this;

			Hyperlink Link = new Hyperlink();

			Link.Inlines.Add(new Run()
			{
				Text = Windows.Storage.ApplicationData.Current.LocalFolder.Path
			});

			Link.Click += Link_Click;

			ToolTip ToolTip = new ToolTip()
			{
				Content = "If provisioning is used, you will find the iotdisco URI in this folder. Use this URI to claim the device. " +
					"Erase content of this folder when application is closed, and then restart, to reconfigure the application."
			};

			ToolTipService.SetToolTip(Link, ToolTip);

			this.LocalFolder.Inlines.Add(new Run() { Text = " " });
			this.LocalFolder.Inlines.Add(Link);
		}

		private async void Link_Click(Hyperlink sender, HyperlinkClickEventArgs args)
		{
			try
			{
				await Windows.System.Launcher.LaunchFolderAsync(Windows.Storage.ApplicationData.Current.LocalFolder);
			}
			catch (Exception ex)
			{
				MessageDialog Dialog = new MessageDialog(ex.Message, "Error");
				await MainPage.Instance.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
					async () => await Dialog.ShowAsync());
			}
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			Log.Unregister(this.events);
			this.events = null;

			if (instance == this)
				instance = null;
		}

		public static MainPage Instance
		{
			get { return instance; }
		}

		public async void AddLogMessage(string Message)
		{
			DateTime TP = DateTime.Now;

			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.EventsPanel.Children.Insert(0, new TextBlock() { Text = Message, TextWrapping = TextWrapping.Wrap });

				while (this.EventsPanel.Children.Count > 1000)
					this.EventsPanel.Children.RemoveAt(1000);
			});
		}

		private class Events : EventSink
		{
			public Events() : base(string.Empty)
			{
			}

			public override Task Queue(Event Event)
			{
				StringBuilder sb = new StringBuilder(Event.Message);

				if (!string.IsNullOrEmpty(Event.Object))
				{
					sb.Append(' ');
					sb.Append(Event.Object);
				}

				if (!string.IsNullOrEmpty(Event.Actor))
				{
					sb.Append(' ');
					sb.Append(Event.Actor);
				}

				foreach (KeyValuePair<string, object> Parameter in Event.Tags)
				{
					sb.Append(" [");
					sb.Append(Parameter.Key);
					sb.Append("=");
					if (Parameter.Value != null)
						sb.Append(Parameter.Value.ToString());
					sb.Append("]");
				}

				if (Event.Type >= EventType.Critical && !string.IsNullOrEmpty(Event.StackTrace))
				{
					sb.Append("\r\n\r\n");
					sb.Append(Event.StackTrace);
				}

				MainPage.Instance.AddLogMessage(sb.ToString());

				return Task.CompletedTask;
			}
		}

	}
}
