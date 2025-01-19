using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.IoTGateway.Svc.ServiceManagement;
using Waher.IoTGateway.Svc.ServiceManagement.Enumerations;
using Waher.Persistence;
using Waher.Runtime.Inventory;


#pragma warning disable CA1416 // Validate platform compatibility

namespace Waher.IoTGateway.Svc
{
	/// <summary>
	/// Gateway Service
	/// </summary>
	public class GatewayService : ServiceBase
	{
		private bool autoPaused = false;
		private bool starting = false;

		/// <summary>
		/// Gateway Service
		/// </summary>
		/// <param name="ServiceName">Service Name</param>
		/// <param name="InstanceName">Name of service instance.</param>
		public GatewayService(string ServiceName, string InstanceName)
			: base()
		{
			this.ServiceName = ServiceName;
			if (!string.IsNullOrEmpty(InstanceName))
				this.ServiceName += " " + InstanceName;

			this.AutoLog = true;
			this.CanHandlePowerEvent = true;
			this.CanHandleSessionChangeEvent = true;
			this.CanPauseAndContinue = true;
			this.CanShutdown = true;
			this.CanStop = true;
		}

		/// <inheritdoc/>
		protected override void OnStart(string[] args)
		{
			try
			{
				bool Started;

				if (this.starting)
					Started = false;
				else
				{
					using PendingTimer Timer = new(this);

					Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

					Gateway.GetDatabaseProvider += Program.GetDatabase;
					Gateway.RegistrationSuccessful += Program.RegistrationSuccessful;
					Gateway.OnTerminate += this.TerminateService;

					this.starting = true;
					try
					{
						Started = Gateway.Start(false, true, Program.InstanceName).Result;
						Types.SetModuleParameter("SERVICE_NAME", this.ServiceName);
					}
					finally
					{
						this.starting = false;
					}
				}

				if (!Started)
				{
					Log.Alert("Gateway being started in another process.");
					ThreadPool.QueueUserWorkItem(_ => this.Stop());
					return;
				}
			}
			catch (Exception ex)
			{
				this.ExitCode = 1;
				Log.Alert(ex);
			}
		}

		private Task TerminateService(object Sender, EventArgs e)
		{
			this.ExitCode = 1;
			ThreadPool.QueueUserWorkItem(_ => this.Stop());
			return Task.CompletedTask;
		}

		private class PendingTimer : IDisposable
		{
			private readonly GatewayService service;
			private Timer timer;
			private bool disposed = false;

			public PendingTimer(GatewayService Service)
			{
				this.service = Service;
				this.timer = new Timer(this.MoreTime, null, 0, 1000);
			}

			public void Dispose()
			{
				this.disposed = true;
				this.timer?.Dispose();
				this.timer = null;
			}

			private void MoreTime(object State)
			{
				if (!this.disposed)
				{
					try
					{
						this.service.RequestAdditionalTime(2000);
					}
					catch (InvalidOperationException)
					{
						this.timer?.Dispose();
						this.timer = null;
					}
					catch (Exception)
					{
						// Ignore
					}
				}
			}
		}

		/// <inheritdoc/>
		protected override void OnPause()
		{
			this.OnStop();
		}

		/// <inheritdoc/>
		protected override void OnContinue()
		{
			try
			{
				bool Started;

				if (this.starting)
					Started = false;
				else
				{
					using PendingTimer Timer = new(this);

					this.starting = true;
					try
					{
						Started = Gateway.Start(false, true, Program.InstanceName).Result;
					}
					finally
					{
						this.starting = false;
					}
				}

				if (!Started)
				{
					Log.Alert("Gateway being started in another process.");
					ThreadPool.QueueUserWorkItem(_ => this.Stop());
					return;
				}
			}
			catch (Exception ex)
			{
				this.ExitCode = 1;
				Log.Alert(ex);
			}
		}

		/// <inheritdoc/>
		protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
		{
			switch (powerStatus)
			{
				case PowerBroadcastStatus.BatteryLow:
				case PowerBroadcastStatus.OemEvent:
				case PowerBroadcastStatus.PowerStatusChange:
				case PowerBroadcastStatus.QuerySuspend:
					Flush();
					return true;

				case PowerBroadcastStatus.ResumeAutomatic:
				case PowerBroadcastStatus.ResumeCritical:
				case PowerBroadcastStatus.ResumeSuspend:
					if (this.autoPaused)
					{
						this.autoPaused = false;

						if (this.starting)
							Log.Warning("Gateway is in the process of starting, called from another source.");
						else
						{
							Log.Notice("Resuming service.");
							this.OnContinue();
						}
					}
					return true;

				case PowerBroadcastStatus.Suspend:
					this.autoPaused = true;
					Log.Notice("Suspending service.");
					this.OnStop();
					return true;

				case PowerBroadcastStatus.QuerySuspendFailed:
				default:
					return true;
			}
		}

		/// <inheritdoc/>
		protected override void OnSessionChange(SessionChangeDescription ChangeDescription)
		{
			int SessionId = ChangeDescription.SessionId;
			List<KeyValuePair<string, object>> Tags =
			[
				new("SessionId", SessionId),
				new("Domain", Gateway.Domain?.Value ?? "N/A")
			];

			AddWtsUserName(Tags, SessionId);
			AddWtsName(Tags, "Initial Program", SessionId, WtsInfoClass.WTSInitialProgram, NullTerminatedString);
			AddWtsName(Tags, "Application Name", SessionId, WtsInfoClass.WTSApplicationName, NullTerminatedString);
			AddWtsName(Tags, "Working Directory", SessionId, WtsInfoClass.WTSWorkingDirectory, NullTerminatedString);
			AddWtsName(Tags, "Station Name", SessionId, WtsInfoClass.WTSWinStationName, NullTerminatedString);
			AddWtsName(Tags, "Connect State", SessionId, WtsInfoClass.WTSConnectState, EnumerationString<WtsConnectClass>);
			AddWtsName(Tags, "Client Build Number", SessionId, WtsInfoClass.WTSClientBuildNumber, IntegerValue);
			AddWtsName(Tags, "Client Name", SessionId, WtsInfoClass.WTSClientName, NullTerminatedString);
			AddWtsName(Tags, "Client Directory", SessionId, WtsInfoClass.WTSClientDirectory, NullTerminatedString);
			AddWtsName(Tags, "Client Product ID", SessionId, WtsInfoClass.WTSClientProductId, IntegerValue);
			AddWtsName(Tags, "Client Hardware ID", SessionId, WtsInfoClass.WTSClientHardwareId, IntegerValue);
			AddWtsName(Tags, "Client Address", SessionId, WtsInfoClass.WTSClientAddress, ClientAddress);
			AddWtsName(Tags, "Client Display", SessionId, WtsInfoClass.WTSClientDisplay, ClientDisplay);
			AddWtsName(Tags, "Client Protocol Type", SessionId, WtsInfoClass.WTSClientProtocolType, EnumerationString<WtsClientProtocolType>);
			AddWtsName(Tags, "Client Info", SessionId, WtsInfoClass.WTSClientInfo, NullTerminatedString);

			string Message;

			switch (ChangeDescription.Reason)
			{
				case SessionChangeReason.ConsoleConnect:
					Message = "User connected to machine via console interface.";
					break;

				case SessionChangeReason.ConsoleDisconnect:
					Message = "User disconnected console interface.";
					break;

				case SessionChangeReason.RemoteConnect:
					Message = "User connected remotely to machine.";
					break;

				case SessionChangeReason.RemoteDisconnect:
					Message = "User disconnected remote interface.";
					break;

				case SessionChangeReason.SessionLock:
					Message = "User session locked.";
					break;

				case SessionChangeReason.SessionLogoff:
					Message = "User logged off.";
					break;

				case SessionChangeReason.SessionLogon:
					Message = "User logged on.";
					break;

				case SessionChangeReason.SessionRemoteControl:
					Message = "User remote control status of session has changed.";
					break;

				case SessionChangeReason.SessionUnlock:
					Message = "User session unlocked.";
					break;

				default:
					Tags.Add(new KeyValuePair<string, object>("Reason", ChangeDescription.Reason.ToString()));
					Message = "Session changed.";
					break;
			}

			if (CaseInsensitiveString.IsNullOrEmpty(Gateway.Domain))
				Log.Notice(Message, [.. Tags]);
			else
			{
				if ((Setup.NotificationConfiguration.Instance.Addresses?.Length ?? 0) == 0)
					Log.Alert(Message, [.. Tags]);
				else
				{
					Log.Notice(Message, [.. Tags]);

					StringBuilder Markdown = new();

					Markdown.AppendLine(MarkdownDocument.Encode(Message));
					Markdown.AppendLine();
					Markdown.AppendLine("| Details ||");
					Markdown.AppendLine("|:----|:---|");

					foreach (KeyValuePair<string, object> Tag in Tags)
					{
						Markdown.Append("| ");
						Markdown.Append(MarkdownDocument.Encode(Tag.Key));
						Markdown.Append(" | ");
						Markdown.Append(MarkdownDocument.Encode(Tag.Value?.ToString() ?? string.Empty));
						Markdown.AppendLine(" |");
					}

					Gateway.SendNotification(Markdown.ToString());
				}
			}
		}

		private static void AddWtsUserName(List<KeyValuePair<string, object>> Tags, int SessionId)
		{
			string Value = GetUserName(SessionId);
			if (!string.IsNullOrEmpty(Value))
				Tags.Add(new KeyValuePair<string, object>("User Name", Value));
		}

		private static void AddWtsName(List<KeyValuePair<string, object>> Tags, string Key, int SessionId,
			WtsInfoClass InfoClass, ParseWtsInfo Parser)
		{
			object Value = GetWtsValue(SessionId, InfoClass, Parser);
			if (Value is not null)
				Tags.Add(new KeyValuePair<string, object>(Key, Value));
		}

		private static string GetUserName(int SessionId)
		{
			try
			{
				string UserName = GetWtsValue(SessionId, WtsInfoClass.WTSUserName, NullTerminatedString) as string;
				if (string.IsNullOrEmpty(UserName))
					return null;

				string Domain = GetWtsValue(SessionId, WtsInfoClass.WTSDomainName, NullTerminatedString) as string;
				if (!string.IsNullOrEmpty(Domain))
					UserName = Domain + "\\" + UserName;

				return UserName;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return null;
			}
		}

		private delegate object ParseWtsInfo(IntPtr Buffer, int Len);

		private static object NullTerminatedString(IntPtr Buffer, int _)
		{
			string s = Marshal.PtrToStringAnsi(Buffer);

			if (!string.IsNullOrEmpty(s))
				s = CommonTypes.Escape(s, specialCharactersToEscape, specialCharacterEscapes);

			return s;
		}

		private static long GetInteger(IntPtr Buffer, int Len)
		{
			switch (Len)
			{
				case 1: return Marshal.ReadByte(Buffer, 0);
				case 2: return Marshal.ReadInt16(Buffer, 0);
				case 4: return Marshal.ReadInt32(Buffer, 0);
				case 8: return Marshal.ReadInt64(Buffer, 0);
				default:
					if (Len > 8)
						Len = 8;

					byte[] Bin = new byte[8];
					int i;

					for (i = 0; i < Len; i++)
						Bin[i] = Marshal.ReadByte(Buffer, i);

					return BitConverter.ToInt64(Bin, 0);
			}
		}

		private static object IntegerValue(IntPtr Buffer, int Len)
		{
			return GetInteger(Buffer, Len);
		}

		private static object EnumerationString<T>(IntPtr Buffer, int Len)
			where T : Enum
		{
			long i = GetInteger(Buffer, Len);
			return Enum.ToObject(typeof(T), i);
		}

		private static object ClientAddress(IntPtr Buffer, int Len)
		{
			// https://www.pinvoke.net/default.aspx/wtsapi32/WTS_CLIENT_ADDRESS.html

			if (EnumerationString<AddressFamily>(Buffer, Math.Min(4, Len)) is not AddressFamily AddressFamily)
				return null;

			switch (AddressFamily)
			{
				case AddressFamily.InterNetwork:
					if (Len < 10)
						return AddressFamily;

					byte[] Bin;

					try
					{
						Bin = new byte[4];
						Marshal.Copy(Buffer, Bin, 6, 4);

						return new IPAddress(Bin).ToString();
					}
					catch (Exception)
					{
						return AddressFamily;
					}

				case AddressFamily.InterNetworkV6:
					if (Len < 20)
						return AddressFamily;

					try
					{
						Bin = new byte[16];
						Marshal.Copy(Buffer, Bin, 4, 16);
					
						return new IPAddress(Bin).ToString();
					}
					catch (Exception)
					{
						return AddressFamily;
					}

				default:
					return AddressFamily;
			}
		}

		private static object ClientDisplay(IntPtr Buffer, int Len)
		{
			// https://learn.microsoft.com/en-us/windows/win32/api/wtsapi32/ns-wtsapi32-wts_client_display

			if (Len < 12)
				return null;

			byte[] Bin = new byte[12];
			Marshal.Copy(Buffer, Bin, 0, 12);

			int Width = BitConverter.ToInt32(Bin, 0);
			int Height = BitConverter.ToInt32(Bin, 4);
			int ColorDepth = BitConverter.ToInt32(Bin, 8);
			string s = Width.ToString() + "x" + Height.ToString();

			switch (ColorDepth)
			{
				case 1:
					s += " (4 bits per pixel.)";
					break;

				case 2:
					s += " (8 bits per pixel.)";
					break;

				case 4:
					s += " (16 bits per pixel.)";
					break;

				case 8:
					s += " (3 byte RGB).";
					break;

				case 16:
					s += " (15 bits per pixel.)";
					break;

				case 24:
					s += " (24 bits per pixel.)";
					break;

				case 32:
					s += " (32 bits per pixel.)";
					break;
			}

			return s;
		}

		private static object GetWtsValue(int SessionId, WtsInfoClass InfoClass, ParseWtsInfo Parser)
		{
			object Result;

			try
			{
				if (Win32.WTSQuerySessionInformation(IntPtr.Zero, SessionId, InfoClass, out IntPtr Buffer, out int Len) &&
					Buffer != IntPtr.Zero && Len > 1)
				{
					try
					{
						Result = Parser(Buffer, Len);
						Win32.WTSFreeMemory(Buffer);
						Buffer = IntPtr.Zero;
					}
					finally
					{
						if (Buffer != IntPtr.Zero)
							Win32.WTSFreeMemory(Buffer);
					}
				}
				else
					return null;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return null;
			}

			return Result;
		}

		private static readonly char[] specialCharactersToEscape =
		[
			'\x00',
			'\x01',
			'\x02',
			'\x03',
			'\x04',
			'\x05',
			'\x06',
			'\a',	// 7  - 0x07
			'\b',	// 8  - 0x08
			'\n',	// 10 - 0x0a
			'\v',	// 11 - 0x0b
			'\f',	// 12 - 0x0c
			'\r',	// 13 - 0x0d
			'\x0e',
			'\x0f',
			'\x10',
			'\x11',
			'\x12',
			'\x13',
			'\x14',
			'\x15',
			'\x16',
			'\x17',
			'\x18',
			'\x19',
			'\x1a',
			'\x1b',
			'\x1c',
			'\x1d',
			'\x1e',
			'\x1f'
		];
		private static readonly string[] specialCharacterEscapes =
		[
			"<NUL>",	// '\x00',
			"<SOH>",	// '\x01',
			"<STX>",	// '\x02',
			"<ETX>",	// '\x03',
			"<EOT>",	// '\x04',
			"<ENQ>",	// '\x05',
			"<ACK>",	// '\x06',
			"<BEL>",	// '\a',	// 7  - 0x07
			"<BS>",		// '\b',	// 8  - 0x08
			"<LF>",		// '\n',	// 10 - 0x0a
			"<VT>",		// '\v',	// 11 - 0x0b
			"<FF>",		// '\f',	// 12 - 0x0c
			"<CR>",		// '\r',	// 13 - 0x0d
			"<SO>",		// '\x0e',
			"<SI>",		// '\x0f',
			"<DLE>",	// '\x10',
			"<DC1>",	// '\x11',
			"<DC2>",	// '\x12',
			"<DC3>",	// '\x13',
			"<DC4>",	// '\x14',
			"<NAK>",	// '\x15',
			"<SYN>",	// '\x16',
			"<ETB>",	// '\x17',
			"<CAN>",	// '\x18',
			"<EM>",		// '\x19',
			"<SUB>",	// '\x1a',
			"<ESC>",	// '\x1b',
			"<FS>",		// '\x1c',
			"<GS>",		// '\x1d',
			"<RS>",		// '\x1e',
			"<US>"		// '\x1f'
		];


		/// <inheritdoc/>
		protected override void OnShutdown()
		{
			Log.Notice("System is shutting down.");
			this.Stop();
		}

		/// <inheritdoc/>
		protected override void OnStop()
		{
			Log.Notice("Service is being stopped.");
			try
			{
				using PendingTimer Timer = new(this);

				Flush();
				Gateway.Stop().Wait();
				Log.TerminateAsync().Wait();
			}
			catch (Exception ex)
			{
				Log.Alert(ex);
			}
		}

		private static void Flush()
		{
			if (Database.HasProvider)
				Database.Provider.Flush().Wait();

			if (Ledger.HasProvider)
				Ledger.Provider.Flush().Wait();
		}

		/// <inheritdoc/>
		protected override async void OnCustomCommand(int command)
		{
			try
			{
				await Gateway.ExecuteServiceCommand(command);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}

#pragma warning restore CA1416 // Validate platform compatibility
