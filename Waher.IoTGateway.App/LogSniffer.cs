using System;
using System.Text;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.IoTGateway.App
{
	public class LogSniffer : SnifferBase
	{
		public override void Error(DateTime _, string Error)
		{
			Log.Error(Error);
		}

		public override void Exception(DateTime _, string Exception)
		{
			Log.Critical(Exception);
		}

		public override void Information(DateTime _, string Comment)
		{
			Log.Informational(Comment);
		}

		public override void ReceiveBinary(DateTime _, byte[] Data)
		{
			Log.Informational("Rx: " + ToString(Data));
		}

		public override void ReceiveText(DateTime _, string Text)
		{
			Log.Informational("Rx: " + Text);
		}

		public override void TransmitBinary(DateTime _, byte[] Data)
		{
			Log.Informational("Tx: " + ToString(Data));
		}

		public override void TransmitText(DateTime _, string Text)
		{
			Log.Informational("Tx: " + Text);
		}

		public override void Warning(DateTime _, string Warning)
		{
			Log.Warning(Warning);
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
