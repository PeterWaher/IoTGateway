using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Client.WPF.Model;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.P2P.SOCKS5;
using Waher.Networking.XMPP.RDP;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for RemoteDesktopView.xaml
	/// </summary>
	public partial class RemoteDesktopView : UserControl, ITabView
	{
		private readonly LinkedList<(Pending, int, int)> queue = new LinkedList<(Pending, int, int)>();
		private readonly XmppContact node;
		private readonly XmppClient client;
		private readonly RemoteDesktopClient rdpClient;
		private readonly object synchObj = new object();
		private RemoteDesktopSession session;
		private Pending[,] pendingTiles = null;
		private WriteableBitmap desktop = null;
		private Timer timer;
		private int columns;
		private int rows;
		private bool drawing = false;
		private bool disposeRdpClient;

		private class Pending
		{
			public string Base64;
			public byte[] Bin;

			public Pending(string Base64)
			{
				this.Base64 = Base64;
			}

			public Pending(byte[] Bin)
			{
				this.Bin = Bin;
			}
		}

		public RemoteDesktopView(XmppContact Node, XmppClient Client, RemoteDesktopClient RdpClient, bool DisposeRdpClient)
		{
			this.node = Node;
			this.client = Client;
			this.rdpClient = RdpClient;
			this.disposeRdpClient = DisposeRdpClient;

			InitializeComponent();

			this.Focusable = true;
			Keyboard.Focus(this);
		}

		public RemoteDesktopSession Session
		{
			get => this.session;
			internal set
			{
				if (!(this.session is null))
				{
					this.session.StateChanged -= Session_StateChanged;
					this.session.TileUpdated -= Session_TileUpdated;
				}

				this.session = value;

				this.session.StateChanged += Session_StateChanged;
				this.session.TileUpdated += Session_TileUpdated;
			}
		}

		private void Session_StateChanged(object sender, EventArgs e)
		{
			if (this.session.State == RemoteDesktopSessionState.Started && this.desktop is null)
			{
				int ScreenWidth = this.session.Width;
				int ScreenHeight = this.session.Height;
				this.columns = (ScreenWidth + this.session.TileSize - 1) / this.session.TileSize;
				this.rows = (ScreenHeight + this.session.TileSize - 1) / this.session.TileSize;

				lock (this.synchObj)
				{
					this.pendingTiles = new Pending[this.rows, this.columns];

					foreach ((Pending Tile, int X, int Y) in this.queue)
						this.pendingTiles[Y, X] = Tile;

					this.queue.Clear();
				}

				this.timer = new Timer(this.UpdateScreen, null, 250, 250);
			}
		}

		private void Session_TileUpdated(object sender, TileEventArgs e)
		{
			lock (this.synchObj)
			{
				if (this.pendingTiles is null)
					this.queue.AddLast((new Pending(e.TileBase64), e.X, e.Y));
				else
					this.pendingTiles[e.Y, e.X] = new Pending(e.TileBase64);
			}
		}

		private byte[] buffer;
		private int state = 0;
		private byte command = 0;
		private int len = 0;
		private int left = 0;
		private int pos = 0;
		private int x = 0;
		private int y = 0;

		internal Task Socks5DataReceived(object Sender, DataReceivedEventArgs e)
		{
			byte[] Data = e.Buffer;
			int i = e.Offset;
			int c = e.Count;
			int j;
			byte b;

			while (c > 0)
			{
				b = Data[i++];
				c--;

				switch (this.state)
				{
					case 0:
						this.command = b;
						this.state++;
						break;

					case 1:
						this.command = b;
						this.state++;
						break;

					case 2:
						this.len = b;
						this.state++;
						break;

					case 3:
						this.len |= b << 8;
						this.state++;
						break;

					case 4:
						this.len |= b << 16;
						this.left = len;
						this.buffer = new byte[len];
						this.pos = 0;
						this.state++;
						break;

					case 5:
						this.x = b;
						this.state++;
						break;

					case 6:
						this.x |= b << 8;
						this.state++;
						break;

					case 7:
						this.y = b;
						this.state++;
						break;

					case 8:
						this.y |= b << 8;
						if (this.left > 0)
							this.state++;
						else
						{
							this.ProcessCommand();
							this.state = 0;
						}
						break;

					case 9:
						j = Math.Min(this.left, c + 1);
						Array.Copy(Data, i - 1, this.buffer, this.pos, j);
						this.pos += j;
						this.left -= j;
						j--;
						i += j;
						c -= j;
						this.ProcessCommand();
						this.state = 0;
						break;

					default:
						c = 0;
						break;
				}
			}

			return Task.CompletedTask;
		}

		private void ProcessCommand()
		{
			switch (this.command)
			{
				case 0:
					lock (this.synchObj)
					{
						if (this.pendingTiles is null)
							this.queue.AddLast((new Pending(this.buffer), this.x, this.y));
						else
							this.pendingTiles[this.y, this.x] = new Pending(this.buffer);
					}
					break;
			}
		}

		internal Task Socks5StreamClosed(object Sender, StreamEventArgs e)
		{
			return Task.CompletedTask;  // TODO
		}

		private void UpdateScreen(object _)
		{
			MainWindow.UpdateGui(this.UpdateScreenGuiThread);
		}

		private void UpdateScreenGuiThread()
		{
			MemoryStream ms = null;
			Bitmap Tile = null;
			BitmapData Data = null;
			Pending PendingTile;
			bool Locked = false;
			int Size = this.session.TileSize;
			int x, y;

			try
			{
				if (this.drawing)
					return;

				this.drawing = true;

				for (y = 0; y < this.rows; y++)
				{
					for (x = 0; x < this.columns; x++)
					{
						lock (this.synchObj)
						{
							PendingTile = this.pendingTiles[y, x];
							if (PendingTile is null)
								continue;

							this.pendingTiles[y, x] = null;
						}

						if (this.desktop is null)
						{
							this.desktop = new WriteableBitmap(this.session.Width, this.session.Height, 96, 96, PixelFormats.Bgra32, null);
							this.DesktopImage.Source = this.desktop;
						}

						ms?.Dispose();
						ms = null;
						ms = new MemoryStream(PendingTile.Bin ?? Convert.FromBase64String(PendingTile.Base64));

						Tile = (Bitmap)Bitmap.FromStream(ms);
						Data = Tile.LockBits(new Rectangle(0, 0, Tile.Width, Tile.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

						if (!Locked)
						{
							this.desktop.Lock();
							Locked = true;
						}

						this.desktop.WritePixels(new Int32Rect(0, 0, Tile.Width, Tile.Height), Data.Scan0, Data.Stride * Data.Height,
							Data.Stride, x * Size, y * Size);

						Tile.UnlockBits(Data);
						Data = null;

						Tile.Dispose();
						Tile = null;
					}
				}
			}
			finally
			{
				this.drawing = false;

				if (Locked)
					this.desktop.Unlock();

				if (!(Data is null))
					Tile.UnlockBits(Data);

				ms?.Dispose();
				Tile?.Dispose();
			}
		}

		public async void Dispose()
		{
			try
			{
				this.node?.UnregisterView(this);

				this.timer?.Dispose();
				this.timer = null;

				if (!(this.session is null) &&
					this.session.State != RemoteDesktopSessionState.Stopped &&
					this.session.State != RemoteDesktopSessionState.Stopping)
				{
					await this.rdpClient.StopSessionAsync(this.session.RemoteJid, this.session.SessionId);
				}

				if (this.disposeRdpClient)
				{
					this.rdpClient.Dispose();
					this.disposeRdpClient = false;
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			this.node?.ViewClosed();
		}

		public XmppContact Node => this.node;
		public XmppClient Client => this.client;
		public RemoteDesktopClient RdpClient => this.rdpClient;

		private void UserControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (!(this.session is null))
			{
				this.GetPosition(e, out int X, out int Y);
				this.session.MouseMoved(X, Y);
				e.Handled = true;
			}
		}

		private void GetPosition(MouseEventArgs e, out int X, out int Y)
		{
			System.Windows.Point P = e.GetPosition(this.DesktopImage);

			X = (int)(this.session.Width * P.X / this.DesktopImage.ActualWidth + 0.5);
			Y = (int)(this.session.Height * P.Y / this.DesktopImage.ActualHeight + 0.5);
		}

		private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!(this.session is null))
			{
				this.GetPosition(e, out int X, out int Y);

				switch (e.ChangedButton)
				{
					case System.Windows.Input.MouseButton.Left:
						this.session.MouseDown(X, Y, Networking.XMPP.RDP.MouseButton.Left);
						e.Handled = true;
						break;

					case System.Windows.Input.MouseButton.Middle:
						this.session.MouseDown(X, Y, Networking.XMPP.RDP.MouseButton.Middle);
						e.Handled = true;
						break;

					case System.Windows.Input.MouseButton.Right:
						this.session.MouseDown(X, Y, Networking.XMPP.RDP.MouseButton.Right);
						e.Handled = true;
						break;

					default:
						e.Handled = false;
						break;
				}
			}
		}

		private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!(this.session is null))
			{
				this.GetPosition(e, out int X, out int Y);

				switch (e.ChangedButton)
				{
					case System.Windows.Input.MouseButton.Left:
						this.session.MouseUp(X, Y, Networking.XMPP.RDP.MouseButton.Left);
						e.Handled = true;
						break;

					case System.Windows.Input.MouseButton.Middle:
						this.session.MouseUp(X, Y, Networking.XMPP.RDP.MouseButton.Middle);
						e.Handled = true;
						break;

					case System.Windows.Input.MouseButton.Right:
						this.session.MouseUp(X, Y, Networking.XMPP.RDP.MouseButton.Right);
						e.Handled = true;
						break;

					default:
						e.Handled = false;
						break;
				}
			}
		}

		private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (!(this.session is null))
			{
				this.GetPosition(e, out int X, out int Y);
				this.session.MouseWheel(X, Y, e.Delta);
				e.Handled = true;
			}
		}

		private void UserControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (!(this.session is null))
			{
				int KeyCode = KeyInterop.VirtualKeyFromKey(e.Key);
				this.session.KeyDown(KeyCode);
				e.Handled = true;
			}
		}

		private void UserControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (!(this.session is null))
			{
				int KeyCode = KeyInterop.VirtualKeyFromKey(e.Key);
				this.session.KeyUp(KeyCode);
				e.Handled = true;
			}
		}

		private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible)
				Keyboard.Focus(this);
		}

		public void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: screen capture?
		}

		public void SaveAsButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: screen capture?
		}

		public void NewButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Refresh screen?
		}

		public void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: ?
		}
	}
}
