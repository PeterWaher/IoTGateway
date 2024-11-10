using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Multipart;
using Waher.Content.Text;
using Waher.Networking.SASL;
using Waher.Networking.SMTP.Exceptions;
using Waher.Networking.Sniffers;

namespace Waher.Networking.SMTP
{
    /// <summary>
    /// Simple SMTP Client
    /// </summary>
    public class SimpleSmtpClient : CommunicationLayer, ISaslClientSide, IDisposable
	{
		/// <summary>
		/// 25
		/// </summary>
		public const int DefaultSmtpPort = 25;

		/// <summary>
		/// 587
		/// </summary>
		public const int AlternativeSmtpPort = 587;

		private readonly List<KeyValuePair<int, string>> response = new List<KeyValuePair<int, string>>();
		private TaskCompletionSource<KeyValuePair<int, string>[]> responseSource = new TaskCompletionSource<KeyValuePair<int, string>[]>();
		private RowTcpClient client;
		private readonly object synchObj = new object();
		private readonly string userName;
		private readonly string password;
		private readonly string host;
		private readonly int port;
		private string domain;
		private bool startTls = false;
		//private bool smptUtf8 = false;
		//private bool eightBitMime = false;
		//private bool enhancedStatusCodes = false;
		//private bool help = false;
		private bool trustCertificate = false;
		//private int? size = null;
		private string[] authMechanisms = null;
		private string[] permittedAuthenticationMechanisms = null;

		/// <summary>
		/// Simple SMTP Client
		/// </summary>
		/// <param name="Domain">Domain</param>
		/// <param name="Host">Host name</param>
		/// <param name="Port">Port number</param>
		/// <param name="Sniffers">Sniffers</param>
		public SimpleSmtpClient(string Domain, string Host, int Port, params ISniffer[] Sniffers)
			: this(Domain, Host, Port, null, null, Sniffers)
		{
		}

		/// <summary>
		/// Simple SMTP Client
		/// </summary>
		/// <param name="Domain">Domain</param>
		/// <param name="Host">Host name</param>
		/// <param name="Port">Port number</param>
		/// <param name="Sniffers">Sniffers</param>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		public SimpleSmtpClient(string Domain, string Host, int Port, string UserName, string Password, params ISniffer[] Sniffers)
			: base(false, Sniffers)
		{
			this.domain = Domain;
			this.host = Host;
			this.port = Port;
			this.userName = UserName;
			this.password = Password;
		}

		/// <summary>
		/// Connects to the server.
		/// </summary>
		public async Task Connect()
		{
			this.client?.Dispose();
			this.client = null;

			lock (this.synchObj)
			{
				this.response.Clear();
			}

			this.client = new RowTcpClient(Encoding.UTF8, 10000, false);
			this.client.Client.ReceiveTimeout = 10000;
			this.client.Client.SendTimeout = 10000;

			this.client.OnReceived += this.Client_OnReceived;
			this.client.OnSent += this.Client_OnSent;
			this.client.OnError += this.Client_OnError;
			this.client.OnInformation += this.Client_OnInformation;
			this.client.OnWarning += this.Client_OnWarning;

			await this.Information("Connecting to " + this.host + ":" + this.port.ToString());
			await this.client.ConnectAsync(this.host, this.port);
			await this.Information("Connected to " + this.host + ":" + this.port.ToString());

			await this.AssertOkResult();
		}

		private async Task<string> Client_OnWarning(string Text)
		{
			await this.Warning(Text);
			return Text;
		}

		private async Task<string> Client_OnInformation(string Text)
		{
			await this.Information(Text);
			return Text;
		}

		private Task Client_OnError(object Sender, Exception Exception)
		{
			return this.Error(Exception.Message);
		}

		private async Task<bool> Client_OnSent(object Sender, string Text)
		{
			await this.TransmitText(Text);
			return true;
		}

		private async Task<bool> Client_OnReceived(object Sender, string Row)
		{
			if (string.IsNullOrEmpty(Row))
			{
				await this.Error("No response returned.");
				return true;
			}

			await this.ReceiveText(Row);

			int i = Row.IndexOfAny(spaceHyphen);
			if (i < 0)
				i = Row.Length;

			if (!int.TryParse(Row.Substring(0, i), out int Code))
			{
				await this.Error("Invalid response returned.");
				return true;
			}

			bool More = i < Row.Length && Row[i] == '-';

			lock (this.synchObj)
			{
				if (i < Row.Length)
					Row = Row.Substring(i + 1).Trim();
				else
					Row = string.Empty;

				this.response.Add(new KeyValuePair<int, string>(Code, Row));

				if (!More)
				{
					this.responseSource.TrySetResult(this.response.ToArray());
					this.response.Clear();
				}
			}

			return true;
		}

		/// <summary>
		/// Disposes of the client.
		/// </summary>
		public void Dispose()
		{
			this.client?.Dispose();
			this.client = null;
		}

		/// <summary>
		/// Domain
		/// </summary>
		public string Domain
		{
			get => this.domain;
		}

		/// <summary>
		/// If server certificate should be trusted by default (default=false).
		/// </summary>
		public bool TrustCertificate
		{
			get => this.trustCertificate;
			set => this.trustCertificate = value;
		}

		/// <summary>
		/// Server certificate.
		/// </summary>
		public X509Certificate ServerCertificate => this.client.RemoteCertificate;

		/// <summary>
		/// If server certificate is valid.
		/// </summary>
		public bool ServerCertificateValid => this.client.RemoteCertificateValid;

		/// <summary>
		/// Permitted authentication mechanisms.
		/// </summary>
		public string[] PermittedAuthenticationMechanisms
		{
			get => this.permittedAuthenticationMechanisms;
			set => this.permittedAuthenticationMechanisms = value;
		}

		/// <summary>
		/// Reads a response from the server.
		/// </summary>
		/// <returns>Response codes, and messages</returns>
		public Task<KeyValuePair<int, string>[]> ReadResponse()
		{
			return this.ReadResponse(10000);
		}

		/// <summary>
		/// Reads a response from the server.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Response codes, and messages</returns>
		public async Task<KeyValuePair<int, string>[]> ReadResponse(int Timeout)
		{
			TaskCompletionSource<KeyValuePair<int, string>[]> Source = this.responseSource;
			if (await Task.WhenAny(Source.Task, Task.Delay(Timeout)) != Source.Task)
				throw new TimeoutException("Response not returned in time.");

			return Source.Task.Result;
		}

		private static readonly char[] spaceHyphen = new char[] { ' ', '-' };

		private Task WriteLine(string Row)
		{
			lock (this.synchObj)
			{
				this.response.Clear();
				this.responseSource = new TaskCompletionSource<KeyValuePair<int, string>[]>();
			}

			return this.client.SendAsync(Row);
		}

		private Task Write(byte[] Bytes)
		{
			return this.client.SendAsync(Bytes);
		}

		private Task<string> AssertOkResult()
		{
			return this.AssertResult(300);
		}

		private Task<string> AssertContinue()
		{
			return this.AssertResult(400);
		}

		private async Task<string> AssertResult(int MaxExclusive)
		{
			KeyValuePair<int, string>[] Response = await this.ReadResponse();
			int Code = Response[0].Key;
			string Message = Response[0].Value.Trim();

			if (string.IsNullOrEmpty(Message))
				Message = "Request rejected.";

			if (Code < 200 || Code >= MaxExclusive)
			{
				if (Code >= 400 && Code < 500)
					throw new SmtpTemporaryErrorException(Message, Code);
				else
					throw new SmtpException(Message, Code);
			}

			return Response[0].Value;
		}

		/// <summary>
		/// Sends the EHLO command.
		/// </summary>
		/// <param name="Domain">Domain</param>
		/// <exception cref="IOException">If unable to execute command.</exception>
		/// <exception cref="AuthenticationException">If transport authentication failed.</exception>
		/// <returns>Domain name, as seen by server.</returns>
		public async Task<string> EHLO(string Domain)
		{
			this.startTls = false;
			//this.size = null;
			this.authMechanisms = null;
			//this.smptUtf8 = false;
			//this.eightBitMime = false;
			//this.enhancedStatusCodes = false;
			//this.help = false;

			if (string.IsNullOrEmpty(Domain))
				await this.WriteLine("EHLO");
			else
				await this.WriteLine("EHLO " + Domain);

			KeyValuePair<int, string>[] Response = await this.ReadResponse();
			if (Response[0].Key < 200 || Response[0].Key >= 300)
				throw new IOException("Request rejected.");

			int i = Response[0].Value.LastIndexOf('[');
			int j = Response[0].Value.LastIndexOf(']');
			string ResponseDomain;

			if (i >= 0 && j > i)
			{
				ResponseDomain = Response[0].Value.Substring(i + 1, j - i - 1);

				if (string.IsNullOrEmpty(Domain))
					Domain = ResponseDomain;

				if (string.IsNullOrEmpty(this.domain))
					this.domain = ResponseDomain;
			}
			else
				ResponseDomain = string.Empty;

			foreach (KeyValuePair<int, string> P in Response)
			{
				string s = P.Value.ToUpper();

				switch (s)
				{
					case "STARTTLS":
						this.startTls = true;
						break;

					case "SMTPUTF8":
						//this.smptUtf8 = true;
						break;

					case "8BITMIME":
						//this.eightBitMime = true;
						break;

					case "ENHANCEDSTATUSCODES":
						//this.enhancedStatusCodes = true;
						break;

					case "HELP":
						//this.help = true;
						break;

					default:
						/*if (s.StartsWith("SIZE "))
						{
							if (int.TryParse(s.Substring(5).Trim(), out int i))
								this.size = i;
						}
						else*/
						if (s.StartsWith("AUTH "))
							this.authMechanisms = s.Substring(5).Trim().Split(space, StringSplitOptions.RemoveEmptyEntries);
						break;
				}
			}

			if (this.startTls && !(this.client.Stream is SslStream))
			{
				await this.WriteLine("STARTTLS");
				await this.AssertOkResult();    // Will pause when complete.

				await this.client.PauseReading();

				await this.Information("Starting TLS handshake.");
				await this.client.UpgradeToTlsAsClient(null, SslProtocols.Tls12, this.trustCertificate);
				await this.Information("TLS handshake complete.");
				this.client.Continue();

				ResponseDomain = await this.EHLO(Domain);
			}
			else if (!(this.authMechanisms is null) && !string.IsNullOrEmpty(this.userName) && !string.IsNullOrEmpty(this.password))
			{
				foreach (string Mechanism in this.authMechanisms)
				{
					if (Mechanism == "EXTERNAL")
						continue;

					SslStream SslStream = this.client.Stream as SslStream;
					foreach (IAuthenticationMechanism M in SaslModule.Mechanisms)
					{
						if (M.Name != Mechanism)
							continue;

						if (!M.Allowed(SslStream))
							break;

						if (!(this.permittedAuthenticationMechanisms is null) &&
							Array.IndexOf(this.permittedAuthenticationMechanisms, Mechanism) < 0)
						{
							break;
						}

						bool? b;

						try
						{
							b = await M.Authenticate(this.userName, this.password, this);
							if (!b.HasValue)
								continue;
						}
						catch (Exception)
						{
							b = false;
						}

						if (!b.Value)
							throw new AuthenticationException("Unable to authenticate user.");

						return ResponseDomain;
					}
				}

				throw new AuthenticationException("No suitable and supported authentication mechanism found.");
			}

			return ResponseDomain;
		}

		private static readonly char[] space = new char[] { ' ' };

		/// <summary>
		/// Executes the VRFY command.
		/// </summary>
		/// <param name="Account">Account name</param>
		public async Task VRFY(string Account)
		{
			await this.WriteLine("VRFY " + Account);
			await this.AssertOkResult();
		}

		/// <summary>
		/// Executes the MAIL FROM command.
		/// </summary>
		/// <param name="Sender">Sender of mail.</param>
		public async Task MAIL_FROM(string Sender)
		{
			await this.WriteLine("MAIL FROM: <" + Sender + ">");
			await this.AssertOkResult();
		}

		/// <summary>
		/// Executes the RCPT TO command.
		/// </summary>
		/// <param name="Receiver">Receiver of mail.</param>
		public async Task RCPT_TO(string Receiver)
		{
			await this.WriteLine("RCPT TO: <" + Receiver + ">");
			await this.AssertOkResult();
		}

		/// <summary>
		/// Executes the QUIT command.
		/// </summary>
		public async Task QUIT()
		{
			await this.WriteLine("QUIT");
			await this.AssertOkResult();
		}

		/// <summary>
		/// Executes the DATA command.
		/// </summary>
		public async Task DATA(KeyValuePair<string, string>[] Headers, byte[] Body)
		{
			await this.WriteLine("DATA");
			await this.AssertContinue();

			foreach (KeyValuePair<string, string> Header in Headers)
				await this.WriteLine(Header.Key + ": " + Header.Value);

			await this.WriteLine(string.Empty);

			int c = Body.Length;
			int i = 0;
			int j;

			while (i < c)
			{
				j = this.IndexOf(Body, crLfDot, i);
				if (j < 0)
					j = c;

				if (i == 0 && j == c)
					await this.Write(Body);
				else
				{
					byte[] Bin = new byte[j - i];
					Array.Copy(Body, i, Bin, 0, j - i);
					await this.Write(Bin);
				}

				i = j;
				if (i < c)
				{
					await this.Write(crLfDot);
					i += 2;
				}
			}

			await this.WriteLine(string.Empty);
			await this.WriteLine(string.Empty);
			await this.WriteLine(".");

			await this.AssertOkResult();
		}

		private static readonly byte[] crLfDot = new byte[] { (byte)'\r', (byte)'\n', (byte)'.' };

		private int IndexOf(byte[] Data, byte[] Segment, int StartIndex)
		{
			int i, j;
			int d = Segment.Length;
			int c = Data.Length - d + 1;

			for (i = StartIndex; i < c; i++)
			{
				for (j = 0; j < d; j++)
				{
					if (Data[i + j] != Segment[j])
						break;
				}

				if (j == d)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Initiates authentication
		/// </summary>
		/// <param name="Mechanism">Mechanism</param>
		/// <param name="Parameters">Any parameters.</param>
		/// <returns>If initiation was successful, challenge is returned.</returns>
		public async Task<string> Initiate(IAuthenticationMechanism Mechanism, string Parameters)
		{
			string s = "AUTH " + Mechanism.Name;
			if (!string.IsNullOrEmpty(Parameters))
				s += " " + Parameters;

			await this.WriteLine(s);
			return await this.AssertContinue();
		}

		/// <summary>
		/// Sends a challenge response back to the server.
		/// </summary>
		/// <param name="Mechanism">Mechanism</param>
		/// <param name="Parameters">Any parameters.</param>
		/// <returns>If challenge response was successful, response is returned.</returns>
		public async Task<string> ChallengeResponse(IAuthenticationMechanism Mechanism, string Parameters)
		{
			await this.WriteLine(Parameters);
			return await this.AssertContinue();
		}

		/// <summary>
		/// Sends a final response back to the server.
		/// </summary>
		/// <param name="Mechanism">Mechanism</param>
		/// <param name="Parameters">Any parameters.</param>
		/// <returns>If final response was successful, response is returned.</returns>
		public async Task<string> FinalResponse(IAuthenticationMechanism Mechanism, string Parameters)
		{
			await this.WriteLine(Parameters);
			await this.AssertOkResult();

			return null;    // No response in SMTP
		}

		/// <summary>
		/// Sends a formatted e-mail message.
		/// </summary>
		/// <param name="Sender">Sender of message.</param>
		/// <param name="Recipient">Recipient of message.</param>
		/// <param name="Subject">Subject</param>
		/// <param name="MarkdownContent">Markdown content.</param>
		/// <param name="Attachments">Any attachments.</param>
		public async Task SendFormattedEMail(string Sender, string Recipient, string Subject,
			string MarkdownContent, params object[] Attachments)
		{
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(MarkdownContent);
			string HTML = "<html><body>" + HtmlDocument.GetBody(await Doc.GenerateHTML()) + "</body></html>";
			string PlainText = await Doc.GeneratePlainText();
			ContentAlternatives Content = new ContentAlternatives(new EmbeddedContent[]
			{
				new EmbeddedContent()
				{
					ContentType = "text/html; charset=utf-8",
					Raw = Encoding.UTF8.GetBytes(HTML)
				},
				new EmbeddedContent()
				{
					ContentType = PlainTextCodec.DefaultContentType + "; charset=utf-8",
					Raw = Encoding.UTF8.GetBytes(PlainText)
				},
				new EmbeddedContent()
				{
					ContentType = "text/markdown; charset=utf-8",
					Raw = Encoding.UTF8.GetBytes(MarkdownContent)
				}
			});

			KeyValuePair<byte[], string> P = await InternetContent.EncodeAsync(Content, Encoding.UTF8);

			if (Attachments.Length > 0)
			{
				List<EmbeddedContent> Parts = new List<EmbeddedContent>()
				{
					new EmbeddedContent()
					{
						ContentType = P.Value,
						Raw = P.Key
					}
				};

				foreach (object Attachment in Attachments)
				{
					KeyValuePair<byte[], string> P2 = await InternetContent.EncodeAsync(Attachment, Encoding.UTF8);
					Parts.Add(new EmbeddedContent()
					{
						ContentType = P2.Value,
						Raw = P2.Key
					});
				}

				MixedContent Mixed = new MixedContent(Parts.ToArray());

				P = await InternetContent.EncodeAsync(Mixed, Encoding.UTF8);
			}

			byte[] BodyBin = P.Key;
			string ContentType = P.Value;

			List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string>("MIME-VERSION", "1.0"),
				new KeyValuePair<string, string>("FROM", Sender),
				new KeyValuePair<string, string>("TO", Recipient),
				new KeyValuePair<string, string>("SUBJECT", Subject),
				new KeyValuePair<string, string>("DATE", CommonTypes.EncodeRfc822(DateTime.Now)),
				new KeyValuePair<string, string>("IMPORTANCE", "normal"),
				new KeyValuePair<string, string>("X-PRIORITY", "3"),
				new KeyValuePair<string, string>("MESSAGE-ID", Guid.NewGuid().ToString()),
				new KeyValuePair<string, string>("CONTENT-TYPE", ContentType)
			};

			await this.MAIL_FROM(Sender);
			await this.RCPT_TO(Recipient);
			await this.DATA(Headers.ToArray(), BodyBin);
		}

	}
}
