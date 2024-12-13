using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking
{
	/// <summary>
	/// Delegate for text sniffer events.
	/// </summary>
	/// <param name="Text">Text</param>
	/// <return>Text, possibly modified.</return>
	public delegate Task<string> TextSnifferEvent(string Text);

	/// <summary>
	/// Simple base class for classes implementing communication protocols.
	/// </summary>
	public class CommunicationLayer : ICommunicationLayer
	{
		private readonly List<ISniffer> sniffers;
		private readonly bool decoupledEvents;
		private ISniffer[] staticList;
		private bool hasSniffers;

		/// <summary>
		/// Simple base class for classes implementing communication protocols.
		/// </summary>
		/// <param name="DecoupledEvents">If events raised from the communication 
		/// layer are decoupled, i.e. executed in parallel with the source that raised 
		/// them.</param>
		/// <param name="Sniffers">Sniffers.</param>
		public CommunicationLayer(bool DecoupledEvents, params ISniffer[] Sniffers)
		{
			this.decoupledEvents = DecoupledEvents;
			this.sniffers = new List<ISniffer>();
			this.sniffers.AddRange(Sniffers);
			this.staticList = this.sniffers.ToArray();
			this.hasSniffers = this.sniffers.Count > 0;
		}

		/// <summary>
		/// If events raised from the communication layer are decoupled, i.e. executed
		/// in parallel with the source that raised them.
		/// </summary>
		public bool DecoupledEvents => this.decoupledEvents;

		/// <summary>
		/// <see cref="ICommunicationLayer.Add"/>
		/// </summary>
		public virtual void Add(ISniffer Sniffer)
		{
			if (!(Sniffer is null))
			{
				lock (this.sniffers)
				{
					if (!this.sniffers.Contains(Sniffer))
					{
						this.sniffers.Add(Sniffer);
						this.staticList = this.sniffers.ToArray();
						this.hasSniffers = this.staticList.Length > 0;
					}
				}
			}
		}

		/// <summary>
		/// <see cref="ICommunicationLayer.AddRange"/>
		/// </summary>
		public virtual void AddRange(IEnumerable<ISniffer> Sniffers)
		{
			if (!(Sniffers is null))
			{
				lock (this.sniffers)
				{
					foreach (ISniffer Sniffer in Sniffers)
					{
						if (!(Sniffer is null) && !this.sniffers.Contains(Sniffer))
							this.sniffers.Add(Sniffer);
					}

					this.staticList = this.sniffers.ToArray();
					this.hasSniffers = this.staticList.Length > 0;
				}
			}
		}

		/// <summary>
		/// <see cref="ICommunicationLayer.Remove"/>
		/// </summary>
		public virtual bool Remove(ISniffer Sniffer)
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
		public bool HasSniffers => this.hasSniffers;

		/// <summary>
		/// Gets a typed enumerator.
		/// </summary>
		public IEnumerator<ISniffer> GetEnumerator()
		{
			return new SnifferEnumerator(this.staticList);
		}

		/// <summary>
		/// Gets an untyped enumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.staticList.GetEnumerator();
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public Task ReceiveBinary(byte[] Data)
		{
			return this.ReceiveBinary(DateTime.Now, Data);
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public async Task ReceiveBinary(DateTime Timestamp, byte[] Data)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					await Sniffer.ReceiveBinary(Timestamp, Data);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Data">Binary Data.</param>
		public Task TransmitBinary(byte[] Data)
		{
			return this.TransmitBinary(DateTime.Now, Data);
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public async Task TransmitBinary(DateTime Timestamp, byte[] Data)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					await Sniffer.TransmitBinary(Timestamp, Data);
			}
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Text">Text</param>
		public Task ReceiveText(string Text)
		{
			return this.ReceiveText(DateTime.Now, Text);
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public async Task ReceiveText(DateTime Timestamp, string Text)
		{
			if (this.hasSniffers)
			{
				Text = await this.Transform(this.OnReceiveText, Text);

				if (!string.IsNullOrEmpty(Text))
				{
					foreach (ISniffer Sniffer in this.staticList)
						await Sniffer.ReceiveText(Timestamp, Text);
				}
			}
		}

		private async Task<string> Transform(TextSnifferEvent Callback, string s)
		{
			if (!(Callback is null))
			{
				try
				{
					return await Callback(s);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}

			return s;
		}

		/// <summary>
		/// Event received when a block of text has been received. Can be used to modify output.
		/// </summary>
		public event TextSnifferEvent OnReceiveText = null;

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Text">Text</param>
		public Task TransmitText(string Text)
		{
			return this.TransmitText(DateTime.Now, Text);
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public async Task TransmitText(DateTime Timestamp, string Text)
		{
			if (this.hasSniffers)
			{
				Text = await this.Transform(this.OnTransmitText, Text);

				if (!string.IsNullOrEmpty(Text))
				{
					if (Text == " ")
					{
						foreach (ISniffer Sniffer in this.staticList)
							await Sniffer.Information(Timestamp, "Heart beat");
					}
					else
					{
						foreach (ISniffer Sniffer in this.staticList)
							await Sniffer.TransmitText(Timestamp, Text);
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
		public Task Information(string Comment)
		{
			return this.Information(DateTime.Now, Comment);
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public async Task Information(DateTime Timestamp, string Comment)
		{
			if (this.hasSniffers)
			{
				Comment = await this.Transform(this.OnInformation, Comment);

				if (!string.IsNullOrEmpty(Comment))
				{
					foreach (ISniffer Sniffer in this.staticList)
						await Sniffer.Information(Timestamp, Comment);
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
		public Task Warning(string Warning)
		{
			return this.Warning(DateTime.Now, Warning);
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public async Task Warning(DateTime Timestamp, string Warning)
		{
			if (this.hasSniffers)
			{
				Warning = await this.Transform(this.OnWarning, Warning);

				if (!string.IsNullOrEmpty(Warning))
				{
					foreach (ISniffer Sniffer in this.staticList)
						await Sniffer.Warning(Timestamp, Warning);
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
		public Task Error(string Error)
		{
			return this.Error(DateTime.Now, Error);
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public async Task Error(DateTime Timestamp, string Error)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					await Sniffer.Error(Timestamp, Error);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public Task Exception(Exception Exception)
		{
			return this.Exception(DateTime.Now, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public virtual async Task Exception(DateTime Timestamp, Exception Exception)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					await Sniffer.Exception(Timestamp, Exception);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Exception">Exception.</param>
		public Task Exception(string Exception)
		{
			return this.Exception(DateTime.Now, Exception);
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public virtual async Task Exception(DateTime Timestamp, string Exception)
		{
			if (this.hasSniffers)
			{
				foreach (ISniffer Sniffer in this.staticList)
					await Sniffer.Exception(Timestamp, Exception);
			}
		}

		/// <summary>
		/// Checks if the string contains control characters.
		/// </summary>
		/// <param name="Text">String</param>
		/// <returns>If the string contains control characters</returns>
		public static bool ContainsControlCharacters(string Text)
		{
			return Text.IndexOfAny(controlCharacters) >= 0;
		}

		/// <summary>
		/// Encodes control characters in a string.
		/// </summary>
		/// <param name="Text">String</param>
		/// <returns>String, with control characters encoded.</returns>
		public static string EncodeControlCharacters(string Text)
		{
			int i = Text.IndexOfAny(controlCharacters);
			if (i < 0)
				return Text;

			StringBuilder sb = new StringBuilder();
			int j = 0;

			while (i >= 0)
			{
				if (i > j)
					sb.Append(Text[j..i]);

				sb.Append(' ');
				sb.Append(controlCharacterNames[Array.IndexOf(controlCharacters, Text[i])]);
				sb.Append(' ');
				
				j = i + 1;
				i = Text.IndexOfAny(controlCharacters, j);
			}

			if (j < Text.Length)
				sb.Append(Text[j..]);

			return sb.ToString();
		}

		private static readonly char[] controlCharacters = new char[]
		{
			(char)00, (char)01, (char)02, (char)03, (char)04, (char)05, (char)06, (char)07, (char)08,
			(char)11, (char)12, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19,
			(char)20, (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29,
			(char)30, (char)31
		};

		private static readonly string[] controlCharacterNames = new string[]
		{
			"NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL", "BS",
			"VT", "FF", "SO", "SI", "DLE", "DC1", "DC2", "DC3",
			"DC4", "NAK", "SYN", "ETB", "CAN", "EM", "SUB", "ESC", "FS", "GS",
			"RS", "US"
		};

	}
}
