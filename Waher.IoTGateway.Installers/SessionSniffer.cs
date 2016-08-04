using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;
using Waher.Networking.Sniffers;

namespace Waher.IoTGateway.Installers
{
	public class SessionSniffer : ISniffer
	{
		private Session session;

		public SessionSniffer(Session Session)
		{
			this.session = Session;
		}

		public void Error(string Error)
		{
			this.session["Log"] = "ERROR: " + Error;
		}

		public void Exception(string Exception)
		{
			this.session["Log"] = "EXCEPTION: " + Exception;
		}

		public void Information(string Comment)
		{
			this.session["Log"] = Comment;
		}

		public void ReceiveBinary(byte[] Data)
		{
		}

		public void ReceiveText(string Text)
		{
			this.session["Log"] = "Rx: " + Text;
		}

		public void TransmitBinary(byte[] Data)
		{
		}

		public void TransmitText(string Text)
		{
			this.session["Log"] = "Tx: " + Text;
		}

		public void Warning(string Warning)
		{
			this.session["Log"] = "WARNING: " + Warning;
		}
	}
}
