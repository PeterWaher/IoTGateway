using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events.Files;

namespace Waher.Events.Pipe
{
	/// <summary>
	/// Delegate for methods that create object instances of <see cref="NamedPipeServerStream"/>.
	/// </summary>
	/// <param name="Name">Pipe name.</param>
	/// <returns>Object instance.</returns>
	public delegate NamedPipeServerStream NamedPipeServerStreamFactory(string Name);

	/// <summary>
	/// Receives events from an operating system pipe
	/// </summary>
	public class PipeEventRecipient : IDisposable
	{
		private const int bufferSize = 65536;

		private readonly StringBuilder fragment = new StringBuilder();
		private readonly string pipeName;
		private readonly byte[] inputBuffer = new byte[bufferSize];
		private readonly bool logIncoming;
		private readonly NamedPipeServerStreamFactory pipeStreamFactory;
		private NamedPipeServerStream pipe = null;
		private bool disposed = false;
		private int inputState = 0;
		private int inputDepth = 0;

		/// <summary>
		/// Receives events from an operating system pipe
		/// </summary>
		/// <param name="PipeName">Name if pipe to listen on.</param>
		public PipeEventRecipient(string PipeName)
			: this(PipeName, true, DefaultFactory)
		{
		}

		/// <summary>
		/// Receives events from an operating system pipe
		/// </summary>
		/// <param name="PipeName">Name if pipe to listen on.</param>
		/// <param name="LogIncomingEvents">If incoming events should be logged to <see cref="Log"/> automatically.</param>
		public PipeEventRecipient(string PipeName, bool LogIncomingEvents)
			: this(PipeName, LogIncomingEvents, DefaultFactory)
		{
		}

		/// <summary>
		/// Receives events from an operating system pipe
		/// </summary>
		/// <param name="PipeName">Name if pipe to listen on.</param>
		/// <param name="LogIncomingEvents">If incoming events should be logged to <see cref="Log"/> automatically.</param>
		/// <param name="StreamFactory">Method used to create a pipe stream object.</param>
		public PipeEventRecipient(string PipeName, bool LogIncomingEvents, NamedPipeServerStreamFactory StreamFactory)
		{
			if (StreamFactory is null)
				throw new ArgumentNullException(nameof(StreamFactory), "Pipe stream factory cannot be null.");

			this.pipeStreamFactory = StreamFactory;
			this.pipeName = PipeName;
			this.logIncoming = LogIncomingEvents;
			this.pipe = StreamFactory(this.pipeName);
			this.pipe.BeginWaitForConnection(this.PipeClientConnected, null);
		}

		private static NamedPipeServerStream DefaultFactory(string Name)
		{
			return new NamedPipeServerStream(Name, PipeDirection.In, 1, PipeTransmissionMode.Message,
				PipeOptions.Asynchronous, 65536, 65536);
		}

		/// <summary>
		/// Pipe object.
		/// </summary>
		public NamedPipeServerStream Pipe => this.pipe;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (!this.disposed)
			{
				this.disposed = true;
				this.pipe?.Dispose();
				this.pipe = null;
			}
		}

		private void RestartPipe()
		{
			if (this.pipe is null || this.disposed)
				return;

			try
			{
				this.pipe?.Disconnect();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				this.pipe?.Close();
				this.pipe?.Dispose();
				this.pipe = null;

				this.pipe = this.pipeStreamFactory(this.pipeName);
			}

			this.inputState = 0;
			this.inputDepth = 0;
			this.fragment.Clear();

			this.pipe.BeginWaitForConnection(this.PipeClientConnected, null);
		}

		private void PipeClientConnected(IAsyncResult ar)
		{
			try
			{
				if (this.pipe is null)
					return;

				this.pipe.EndWaitForConnection(ar);
				this.pipe.BeginRead(this.inputBuffer, 0, bufferSize, this.PipeReadComplete, null);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				this.RestartPipe();
			}
		}

		private async void PipeReadComplete(IAsyncResult ar)
		{
			try
			{
				if (this.pipe is null)
					return;

				int NrRead = this.pipe.EndRead(ar);
				if (NrRead <= 0)
					this.pipe.BeginWaitForConnection(this.PipeClientConnected, null);
				else
				{
					string s = Encoding.UTF8.GetString(this.inputBuffer, 0, NrRead);
					bool Continue;

					try
					{
						Continue = await this.ParseIncoming(s);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						Continue = true;
					}

					if (Continue)
						this.pipe.BeginRead(this.inputBuffer, 0, bufferSize, this.PipeReadComplete, null);
				}
			}
			catch (IOException)
			{
				Log.Error("Pipe-connection down.", this.pipeName);
				this.RestartPipe();
			}
			catch (Exception ex)
			{
				Log.Critical(ex, this.pipeName);
				this.RestartPipe();
			}
		}

		private async Task<bool> ParseIncoming(string s)
		{
			bool Result = true;

			foreach (char ch in s)
			{
				switch (this.inputState)
				{
					case 0: // Waiting for <
						if (ch == '<')
						{
							this.fragment.Append(ch);
							this.inputState++;
						}
						else if (this.inputDepth > 0)
							this.fragment.Append(ch);
						else if (ch > ' ')
						{
							this.RestartPipe();
							return false;
						}
						break;

					case 1: // Second character in tag
						this.fragment.Append(ch);
						if (ch == '/')
							this.inputState++;
						else
							this.inputState += 2;
						break;

					case 2: // Waiting for end of closing tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth--;
							if (this.inputDepth < 0)
							{
								this.RestartPipe();
								return false;
							}
							else
							{
								if (this.inputDepth == 0)
								{
									if (!await this.ProcessFragment(this.fragment.ToString()))
										Result = false;

									this.fragment.Clear();
								}

								if (this.inputState > 0)
									this.inputState = 0;
							}
						}
						break;

					case 3: // Wait for end of start tag
						this.fragment.Append(ch);
						if (ch == '>')
						{
							this.inputDepth++;
							this.inputState = 0;
						}
						else if (ch == '/')
							this.inputState++;
						break;

					case 4: // Check for end of childless tag.
						this.fragment.Append(ch);
						if (ch == '>')
						{
							if (this.inputDepth == 0)
							{
								if (!await this.ProcessFragment(this.fragment.ToString()))
									Result = false;

								this.fragment.Clear();
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
