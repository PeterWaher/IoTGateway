using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.IoTGateway.App
{
	public class LogSniffer : SnifferBase
	{
		public override Task Error(DateTime _, string Error)
		{
			Log.Error(Error);
			return Task.CompletedTask;
		}

		public override Task Exception(DateTime _, string Exception)
		{
			Log.Critical(Exception);
			return Task.CompletedTask;
		}

		public override Task Information(DateTime _, string Comment)
		{
			Log.Informational(Comment);
			return Task.CompletedTask;
		}

		public override Task ReceiveBinary(DateTime _, byte[] Data)
		{
			Log.Informational("Rx: " + ToString(Data));
			return Task.CompletedTask;
		}

		public override Task ReceiveText(DateTime _, string Text)
		{
			Log.Informational("Rx: " + Text);
			return Task.CompletedTask;
		}

		public override Task TransmitBinary(DateTime _, byte[] Data)
		{
			Log.Informational("Tx: " + ToString(Data));
			return Task.CompletedTask;
		}

		public override Task TransmitText(DateTime _, string Text)
		{
			Log.Informational("Tx: " + Text);
			return Task.CompletedTask;
		}

		public override Task Warning(DateTime _, string Warning)
		{
			Log.Warning(Warning);
			return Task.CompletedTask;
		}

		private static string ToString(byte[] Data)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (byte b in Data)
			{
				if (First)
					First = false;
				else
					sb.Append(' ');

				sb.Append(b.ToString("X2"));
			}

			return sb.ToString();
		}
	}
}
