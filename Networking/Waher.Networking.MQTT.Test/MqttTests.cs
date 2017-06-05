using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;

namespace Waher.Networking.MQTT.Test
{
	[TestClass]
	public abstract class MqttTests
	{
		private MqttClient client;

		[TestInitialize]
		public void TestInitialize()
		{
			this.client = new MqttClient("iot.eclipse.org", this.Port, this.Encypted, "UnitTest", string.Empty, 
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));
		}

		public abstract bool Encypted
		{
			get;
		}

		public abstract int Port
		{
			get;
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}
		}

		[TestMethod]
		public void Test_01_Connect()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.client.OnStateChanged += (sender, State) =>
			{
				switch (State)
				{
					case MqttState.Connected:
						Done.Set();
						break;

					case MqttState.Error:
					case MqttState.Offline:
						Error.Set();
						break;
				}
			};

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Unable to connect to MQTT broker.");
		}
	}
}
