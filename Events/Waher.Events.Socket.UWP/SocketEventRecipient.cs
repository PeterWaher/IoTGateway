using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events.Files;
using Waher.Networking;
using Waher.Networking.Sniffers;

namespace Waher.Events.Socket
{
	/// <summary>
	/// Receives events from a socket.
	/// </summary>
	public class SocketEventRecipient : IDisposable
	{
		private readonly Dictionary<Guid, StringBuilder> fragments = new Dictionary<Guid, StringBuilder>();
		private readonly bool logIncoming;
		private BinaryTcpServer server;
		private bool disposed = false;
		private int inputState = 0;
		private int inputDepth = 0;

#if WINDOWS_UWP
		/// <summary>
		/// Receives events from a socket.
		/// </summary>
		private SocketEventRecipient(int Port, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			this.server = new BinaryTcpServer(Port, TimeSpan.FromSeconds(10), Sniffers);
			this.logIncoming = LogIncomingEvents;
		}

		/// <summary>
		/// Creates and opens a socket-based event recipient.
		/// </summary>
		/// <param name="Port">Port to open.</param>
		/// <param name="LogIncomingEvents">If incoming events should be logged to <see cref="Log"/>.</param>
		/// <param name="Sniffers">Any sniffers to output communucation to.</param>
		/// <returns>Event recipient.</returns>
		public static async Task<SocketEventRecipient> Create(int Port, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			SocketEventRecipient Result = null;

			try
			{
				Result = new SocketEventRecipient(Port, LogIncomingEvents, Sniffers);

				Result.server.OnAccept += Result.Server_OnAccept;
				Result.server.OnClientConnected += Result.Server_OnClientConnected;
				Result.server.OnClientDisconnected += Result.Server_OnClientDisconnected;
				Result.server.OnDataReceived += Result.Server_OnDataReceived;

				await Result.server.Open();
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return Result;
		}
#else
		/// <summary>
		/// Receives events from a socket.
		/// </summary>
		private SocketEventRecipient(int Port, X509Certificate Certificate, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			this.server = new BinaryTcpServer(Port, TimeSpan.FromSeconds(10), Certificate, Sniffers);
			this.logIncoming = LogIncomingEvents;
		}

		/// <summary>
		/// Creates and opens a socket-based event recipient.
		/// </summary>
		/// <param name="Port">Port to open.</param>
		/// <param name="LogIncomingEvents">If incoming events should be logged to <see cref="Log"/>.</param>
		/// <param name="Sniffers">Any sniffers to output communucation to.</param>
		/// <returns>Event recipient.</returns>
		public static Task<SocketEventRecipient> Create(int Port, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			return Create(Port, null, LogIncomingEvents, Sniffers);
		}

		/// <summary>
		/// Creates and opens a socket-based event recipient.
		/// </summary>
		/// <param name="Port">Port to open.</param>
		/// <param name="Certificate">Certificate to use for TLS encryption.</param>
		/// <param name="LogIncomingEvents">If incoming events should be logged to <see cref="Log"/>.</param>
		/// <param name="Sniffers">Any sniffers to output communucation to.</param>
		/// <returns>Event recipient.</returns>
		public static async Task<SocketEventRecipient> Create(int Port, X509Certificate Certificate, bool LogIncomingEvents, params ISniffer[] Sniffers)
		{
			SocketEventRecipient Result = null;

			try
			{
				Result = new SocketEventRecipient(Port, Certificate, LogIncomingEvents, Sniffers);

				Result.server.OnAccept += Result.Server_OnAccept;
				Result.server.OnClientConnected += Result.Server_OnClientConnected;
				Result.server.OnClientDisconnected += Result.Server_OnClientDisconnected;
				Result.server.OnDataReceived += Result.Server_OnDataReceived;

				await Result.server.Open();
			}
			catch (Exception ex)
			{
				Result?.Dispose();
				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return Result;
		}
#endif

		private async Task Server_OnAccept(object Sender, ServerConnectionAcceptEventArgs e)
		{
			EventHandlerAsync<ServerConnectionAcceptEventArgs> h = this.OnAccept;

			if (!(h is null))
			{
				try
				{
					await h(this, e);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
					e.Accept = false;
				}
			}
		}

		public event EventHandlerAsync<ServerConnectionAcceptEventArgs> OnAccept;

		private Task Server_OnClientConnected(object Sender, ServerConnectionEventArgs e)
		{
			lock (this.fragments)
			{
				this.fragments.Remove(e.Id);
			}

			return Task.CompletedTask;
		}

		private Task Server_OnClientDisconnected(object Sender, ServerConnectionEventArgs e)
		{
			lock (this.fragments)
			{
				this.fragments.Remove(e.Id);
			}

			return Task.CompletedTask;
		}

		private async Task Server_OnDataReceived(object Sender, ServerConnectionDataEventArgs e)
		{
			StringBuilder Fragment;

			lock (this.fragments)
			{
				if (!this.fragments.TryGetValue(e.Id, out Fragment))
				{
					Fragment = new StringBuilder();
					this.fragments[e.Id] = Fragment;
				}
			}

			string s = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.Count);
			bool Continue;

			try
			{
				Continue = await this.ParseIncoming(Fragment, s);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				Continue = false;
			}

			if (!Continue)
				e.CloseConnection();
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.server?.Dispose();
				this.server = null;
			}
		}

		private async Task<bool> ParseIncoming(StringBuilder Fragment, string s)
		{
			bool Result = true;

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0: // Waiting for <
						if (ch == '<')
						{
							Fragment.Append(ch);
							this.inputState++;
						}
						else if (this.inputDepth > 0)
							Fragment.Append(ch);
						else if (ch > ' ')
							return false;
						break;

					case 1: // Second character in tag
						Fragment.Append(ch);
						if (ch == '/')
							this.inputState++;
						else
							this.inputState += 2;
						break;

					case 2: // Waiting for end of closing tag
						Fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth--;
							if (this.inputDepth < 0)
								return false;
							else
							{
								if (this.inputDepth == 0)
								{
									if (!await this.ProcessFragment(Fragment.ToString()))
										Result = false;

									Fragment.Clear();
								}

								if (this.inputState > 0)
									this.inputState = 0;
							}
						}
						break;

					case 3: // Wait for end of start tag
						Fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 0;
						}
						else if (ch == '/')
							this.inputState++;
						break;

					case 4: // Check for end of childless tag.
						Fragment.Append(ch);
						if (ch == '>')
						{
							if (this.inputDepth == 0)
							{
								if (!await this.ProcessFragment(Fragment.ToString()))
									Result = false;

								Fragment.Clear();
							}

							if (this.inputState != 0)
								this.inputState = 0;
						}
						else
							this.inputState--;
						break;

					default:
						break;
				}
			}

			return Result;
		}

		private async Task<bool> ProcessFragment(string Xml)
		{
			try
			{
				XmlDocument Doc = new XmlDocument();

				Doc.LoadXml(Xml);

				if (EventExtensions.TryParse(Doc.DocumentElement, out Event Event))
				{
					if (this.logIncoming)
						Log.Event(Event);

					EventEventHandler h = this.EventReceived;

					if (!(h is null))
						await h(this, new EventEventArgs(Event));
				}
				else
				{
					CustomFragmentEventHandler h = this.CustomFragmentReceived;

					if (!(h is null))
						await h(this, new CustomFragmentEventArgs(Doc));
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return true;
		}

		/// <summary>
		/// Event raised when an event has been received.
		/// </summary>
		public event EventEventHandler EventReceived;

		/// <summary>
		/// Event raised when a custom XML fragment has been received.
		/// </summary>
		public event CustomFragmentEventHandler CustomFragmentReceived;
	}
}
