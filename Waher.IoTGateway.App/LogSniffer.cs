using System;
using System.Text;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.Sniffers;

namespace Waher.IoTGateway.App
{
	public class LogSniffer : ISniffer
	{
		public void Error(string Error)
		{
			Log.Error(Error);
		}

		public void Exception(string Exception)
		{
			Log.Critical(Exception);
		}

		public void Information(string Comment)
		{
			Log.Informational(Comment);
		}

		public void ReceiveBinary(byte[] Data)
		{
			Log.Informational("Rx: " + ToString(Data));
		}

		public void ReceiveText(string Text)
		{
			Log.Informational("Rx: " + Text);
		}

		public void TransmitBinary(byte[] Data)
		{
			Log.Informational("Tx: " + ToString(Data));
		}

		public void TransmitText(string Text)
		{
			Log.Informational("Tx: " + Text);
		}

		public void Warning(string Warning)
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
