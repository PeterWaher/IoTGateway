using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;
using Waher.Networking.Sniffers;

namespace Waher.IoTGateway.Installers
{
	public class SessionSniffer : ISniffer, IDisposable
	{
		private Session session;

		public SessionSniffer(Session Session)
		{
			this.session = Session;
		}

		public void Dispose()
		{
			this.session = null;
		}

		private void Log(string s)
		{
			Session Session = this.session;
			if (Session != null)
				Session["Log"] = s;
		}

		public void Error(string Error)
		{
			this.Log("ERROR: " + Error);
		}

		public void Exception(string Exception)
		{
			this.Log("EXCEPTION: " + Exception);
		}

		public void Information(string Comment)
		{
			this.Log(Comment);
		}

		public void ReceiveBinary(byte[] Data)
		{
		}

		public void ReceiveText(string Text)
		{
			this.Log("Rx: " + Text);
		}

		public void TransmitBinary(byte[] Data)
		{
		}

		public void TransmitText(string Text)
		{
			this.Log("Tx: " + Text);
		}

		public void Warning(string Warning)
		{
			this.Log("WARNING: " + Warning);
		}
	}
}
