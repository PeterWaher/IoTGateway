using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Waher.Networking.MQTT;
using Waher.Events.MQTT;

namespace Waher.Client.MqttEventViewer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal static MainWindow currentInstance = null;

		private MqttClient mqtt = null;
		private MqttEventReceptor receptor = null;

		public MainWindow()
		{
			currentInstance = this;
			InitializeComponent();
		}

		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Message.Content = string.Empty;

				if (!int.TryParse(this.Port.Text, out int Port))
					throw new Exception("Invalid port number.");

				this.mqtt = new MqttClient(this.Host.Text, Port, this.Tls.IsChecked.HasValue && this.Tls.IsChecked.Value,
					this.UserName.Text, this.Password.Password)
				{
					TrustServer = this.Trust.IsChecked.HasValue && this.Trust.IsChecked.Value,
				};

				this.mqtt.OnStateChanged += Mqtt_OnStateChanged;
				this.mqtt.OnConnectionError += Mqtt_OnConnectionError;
				this.mqtt.OnError += Mqtt_OnError;
				this.mqtt.OnSubscribed += Mqtt_OnSubscribed;

				this.receptor = new MqttEventReceptor(this.mqtt);
				this.receptor.OnEvent += Receptor_OnEvent;

				this.ConnectButton.IsEnabled = false;
				this.Host.IsEnabled = false;
				this.Port.IsEnabled = false;
				this.UserName.IsEnabled = false;
				this.Password.IsEnabled = false;
				this.Topic.IsEnabled = false;
				this.Tls.IsEnabled = false;
				this.Trust.IsEnabled = false;
				this.CloseButton.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private async Task Mqtt_OnStateChanged(object Sender, MqttState NewState)
		{
			this.Dispatcher.Invoke(new Action(() => this.ConnectionState.Content = NewState.ToString()));

			if (NewState == MqttState.Connected)
				await this.mqtt.SUBSCRIBE(this.Topic.Text);
		}

		private Task Mqtt_OnError(object Sender, Exception Exception)
		{
			this.Dispatcher.Invoke(new Action(() => this.Message.Content = Exception.Message));
			return Task.CompletedTask;
		}

		private Task Mqtt_OnConnectionError(object Sender, Exception Exception)
		{
			this.Dispatcher.Invoke(new Action(() => this.Message.Content = Exception.Message));
			return Task.CompletedTask;
		}

		private Task Mqtt_OnSubscribed(object Sender, ushort PacketIdentifier)
		{
			this.Dispatcher.Invoke(new Action(() => this.Message.Content = "Subscribed to " + this.Topic.Text));
			return Task.CompletedTask;
		}

		private void Receptor_OnEvent(object Sender, EventEventArgs e)
		{
			this.Dispatcher.Invoke(new Action(() => this.EventListView.Items.Add(new EventItem(e.Event))));
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.Message.Content = string.Empty;

				if (this.receptor != null)
				{
					this.receptor.Dispose();
					this.receptor = null;
				}

				if (this.mqtt != null)
				{
					this.mqtt.Dispose();
					this.mqtt = null;
				}

				this.ConnectButton.IsEnabled = true;
				this.Host.IsEnabled = true;
				this.Port.IsEnabled = true;
				this.UserName.IsEnabled = true;
				this.Password.IsEnabled = true;
				this.Topic.IsEnabled = true;
				this.Tls.IsEnabled = true;
				this.Trust.IsEnabled = true;
				this.CloseButton.IsEnabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		internal static readonly string registryKey = Registry.CurrentUser + @"\Software\Waher Data AB\Waher.Client.MqttEventViewer";

		private void Window_Closed(object sender, EventArgs e)
		{
			int i;

			Registry.SetValue(registryKey, "WindowLeft", (int)this.Left, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowTop", (int)this.Top, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowWidth", (int)this.Width, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowHeight", (int)this.Height, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowState", this.WindowState.ToString(), RegistryValueKind.String);

			GridView GridView = (GridView)this.EventListView.View;
			for (i = 0; i < 9; i++)
				Registry.SetValue(registryKey, "Col" + i.ToString(), GridView.Columns[i].Width, RegistryValueKind.DWord);

			Registry.SetValue(MainWindow.registryKey, "Host", this.Host.Text, RegistryValueKind.String);
			Registry.SetValue(MainWindow.registryKey, "UserName", this.UserName.Text, RegistryValueKind.String);
			Registry.SetValue(MainWindow.registryKey, "Password", this.Password.Password, RegistryValueKind.String);
			Registry.SetValue(MainWindow.registryKey, "Topic", this.Topic.Text, RegistryValueKind.String);

			if (int.TryParse(this.Port.Text, out i))
				Registry.SetValue(MainWindow.registryKey, "Port", i, RegistryValueKind.DWord);

			if (this.Tls.IsChecked.HasValue)
				Registry.SetValue(MainWindow.registryKey, "Tls", this.Tls.IsChecked.Value ? 1 : 0, RegistryValueKind.DWord);

			if (this.Trust.IsChecked.HasValue)
				Registry.SetValue(MainWindow.registryKey, "Trust", this.Trust.IsChecked.Value ? 1 : 0, RegistryValueKind.DWord);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			object Value;

			try
			{
				Value = Registry.GetValue(registryKey, "WindowLeft", (int)this.Left);
				if (Value != null && Value is int Left)
					this.Left = Left;

				Value = Registry.GetValue(registryKey, "WindowTop", (int)this.Top);
				if (Value != null && Value is int Top)
					this.Top = Top;

				Value = Registry.GetValue(registryKey, "WindowWidth", (int)this.Width);
				if (Value != null && Value is int Width)
					this.Width = Width;

				Value = Registry.GetValue(registryKey, "WindowHeight", (int)this.Height);
				if (Value != null && Value is int Height)
					this.Height = Height;

				Value = Registry.GetValue(registryKey, "WindowState", this.WindowState.ToString());
				if (Value != null && Value is string WindowState)
					this.WindowState = (WindowState)Enum.Parse(typeof(WindowState), WindowState);

				Value = Registry.GetValue(registryKey, "Host", string.Empty);
				if (Value != null && Value is string Host)
					this.Host.Text = Host;

				Value = Registry.GetValue(registryKey, "UserName", string.Empty);
				if (Value != null && Value is string UserName)
					this.UserName.Text = UserName;

				Value = Registry.GetValue(registryKey, "Password", string.Empty);
				if (Value != null && Value is string Password)
					this.Password.Password = Password;

				Value = Registry.GetValue(registryKey, "Topic", string.Empty);
				if (Value != null && Value is string Topic)
					this.Topic.Text = Topic;

				Value = Registry.GetValue(registryKey, "Port", 0);
				if (Value != null && Value is int Port)
					this.Port.Text = Port.ToString();

				Value = Registry.GetValue(registryKey, "Tls", 0);
				if (Value != null && Value is int Tls)
					this.Tls.IsChecked = Tls != 0;

				Value = Registry.GetValue(registryKey, "Trust", 0);
				if (Value != null && Value is int Trust)
					this.Trust.IsChecked = Trust != 0;

				GridView GridView = (GridView)this.EventListView.View;
				int i;

				for (i = 0; i < 9; i++)
				{
					Value = Registry.GetValue(registryKey, "Col" + i.ToString(), GridView.Columns[i].Width);
					if (Value != null && Value is int ColWidth)
						GridView.Columns[i].Width = ColWidth;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Unable to load values from registry.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
