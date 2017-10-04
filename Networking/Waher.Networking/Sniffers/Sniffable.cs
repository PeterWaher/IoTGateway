using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Waher.Events;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Delegate for text sniffer events.
	/// </summary>
	/// <param name="Text"></param>
	public delegate void TextSnifferEvent(ref string Text);

	/// <summary>
	/// Simple abstract base class for sniffable nodes.
	/// </summary>
	public class Sniffable : ISniffable
	{
		private List<ISniffer> sniffers;
		private ISniffer[] staticList;
		private bool hasSniffers;

		/// <summary>
		/// Simple abstract base class for sniffable nodes.
		/// </summary>
		/// <param name="Sniffers">Sniffers.</param>
		public Sniffable(params ISniffer[] Sniffers)
		{
			this.sniffers = new List<ISniffer>();
			this.sniffers.AddRange(Sniffers);
			this.staticList = this.sniffers.ToArray();
			this.hasSniffers = this.sniffers.Count > 0;
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public void Add(ISniffer Sniffer)
		{
			lock (this.sniffers)
			{
				this.sniffers.Add(Sniffer);
				this.staticList = this.sniffers.ToArray();
				this.hasSniffers = this.staticList.Length > 0;
			}
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			lock (this.sniffers)
			{
				this.sniffers.AddRange(Sniffers);
				this.staticList = this.sniffers.ToArray();
				this.hasSniffers = this.staticList.Length > 0;
			}
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public bool Remove(ISniffer Sniffer)
		{
			lock (this.sniffers)
			{
				if (this.sniffers.Remove(Sniffer))
				{
					this.staticList = this.sniffers.ToArray();
					this.hasSniffers = this.staticList.Length > 0;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Registered sniffers.
		/// </summary>
		public ISniffer[] Sniffers
		{
			get { return (ISniffer[])this.staticList?.Clone(); }
		}

		/// <summary>
		/// If there are sniffers registered on the object.
		/// </summary>
		public bool HasSniffers
		{
			get { return this.hasSniffers; }
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		public IEnumerator<ISniffer> GetEnumerator()
		{
			return new SnifferEnumerator(this.staticList);
		}

		private class SnifferEnumerator : IEnumerator<ISniffer>
		{
			private ISniffer[] list;
			private int pos = 0;

			public SnifferEnumerator(ISniffer[] List)
			{
				this.list = List;
			}

			public ISniffer Current
			{
				get
				{
					if (this.pos < this.list.Length)
						return this.list[this.pos];
					else
						return null;
				}
			}

			public void Dispose()
			{
			}

			object IEnumerator.Current
			{
				get
				{
					if (this.pos < this.list.Length)
						return this.list[this.pos];
					else
						return null;
				}
			}

			public bool MoveNext()
			{
				this.pos++;
				return this.pos < this.list.Length;
			}

			public void Reset()
			{
				this.pos = 0;
			}
		}

		/// <summary>
		/// <see cref="ISniffable.Add"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.staticList.GetEnumerator();
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void ReceiveBinary(byte[] Data)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.ReceiveBinary(Data);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public void TransmitBinary(byte[] Data)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.TransmitBinary(Data);
			}
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public void ReceiveText(string Text)
		{
			if (this.hasSniffers)
			{
				this.Transform(this.OnReceiveText, ref Text);

				if (!string.IsNullOrEmpty(Text))
				{
					foreach (ISniffer Sniffer in this.staticList)
						Sniffer.ReceiveText(Text);
				}
			}
		}

		private void Transform(TextSnifferEvent Callback, ref string s)
		{
			if (Callback != null)
			{
				try
				{
					Callback(ref s);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event received when a block of text has been received. Can be used to modify output.
		/// </summary>
		public event TextSnifferEvent OnReceiveText = null;

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public void TransmitText(string Text)
		{
			if (this.hasSniffers)
			{
				this.Transform(this.OnTransmitText, ref Text);

				if (!string.IsNullOrEmpty(Text))
				{
					if (Text == " ")
					{
						foreach (ISniffer Sniffer in this.staticList)
							Sniffer.Information("Heart beat");
					}
					else
					{
						foreach (ISniffer Sniffer in this.staticList)
							Sniffer.TransmitText(Text);
					}
				}
			}
		}

		/// <summary>
		/// Event received when a block of text has been sent. Can be used to modify output.
		/// </summary>
		public event TextSnifferEvent OnTransmitText = null;

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Comment">Comment.</param>
		public void Information(string Comment)
		{
			if (this.hasSniffers)
			{
				this.Transform(this.OnInformation, ref Comment);

				if (!string.IsNullOrEmpty(Comment))
				{
					foreach (ISniffer Sniffer in this.staticList)
						Sniffer.Information(Comment);
				}
			}
		}

		/// <summary>
		/// Event received when information is logged.
		/// </summary>
		public event TextSnifferEvent OnInformation = null;

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Warning">Warning.</param>
		public void Warning(string Warning)
		{
			if (this.hasSniffers)
			{
				this.Transform(this.OnWarning, ref Warning);

				if (!string.IsNullOrEmpty(Warning))
				{
					foreach (ISniffer Sniffer in this.staticList)
						Sniffer.Warning(Warning);
				}
			}
		}

		/// <summary>
		/// Event received when a warning is logged.
		/// </summary>
		public event TextSnifferEvent OnWarning = null;

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Error">Error.</param>
		public void Error(string Error)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					Sniffer.Error(Error);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public void Exception(Exception Exception)
		{
			AggregateException ex;

			Exception = Log.UnnestException(Exception);

			if ((ex = Exception as AggregateException) != null)
			{
				foreach (Exception ex2 in ex.InnerExceptions)
					this.Exception(ex2);
			}
			else
			{
				string Msg = Exception.Message + "\r\n\r\n" + Exception.StackTrace;

				if (this.hasSniffers)
				{
					foreach (ISniffer Sniffer in this.staticList)
						Sniffer.Exception(Msg);
				}
			}
		}

	}
}
