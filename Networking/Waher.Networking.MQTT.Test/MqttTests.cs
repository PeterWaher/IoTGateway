using System;
using System.Collections.Generic;
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
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));
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

		[TestMethod]
		public void Test_02_Subscribe()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			int Count = 0;

			this.Test_01_Connect();

			this.client.OnContentReceived += (sender, e) =>
			{
				Console.Out.WriteLine(e.Topic + ": " + System.Text.Encoding.UTF8.GetString(e.Data));

				if (++Count == 10)
					Done.Set();
			};

			this.client.SUBSCRIBE("#");
			try
			{
				Assert.IsTrue(Done.WaitOne(60000), "Content not received correctly.");
			}
			finally
			{
				this.client.UNSUBSCRIBE("#");
			}
		}

		[TestMethod]
		public void Test_03_Publish_AtMostOne()
		{
			this.Publish(MqttQualityOfService.AtMostOnce);
		}

		[TestMethod]
		public void Test_04_Publish_AtLeastOnce()
		{
			this.Publish(MqttQualityOfService.AtLeastOnce);
		}

		[TestMethod]
		public void Test_05_Publish_ExactlyOne()
		{
			this.Publish(MqttQualityOfService.ExactlyOnce);
		}

		private void Publish(MqttQualityOfService QoS)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			this.Test_01_Connect();

			this.client.OnContentReceived += (sender, e) =>
			{
				string s = System.Text.Encoding.UTF8.GetString(e.Data);

				if (s == "Hello world.")
					Done.Set();
				else
					Error.Set();
			};

			this.client.SUBSCRIBE("/Waher/IoT/Test", QoS);
			try
			{
				this.client.PUBLISH("/Waher/IoT/Test", QoS, false,
					System.Text.Encoding.UTF8.GetBytes("Hello world."));

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Content not published correctly.");
			}
			finally
			{
				this.client.UNSUBSCRIBE("/Waher/IoT/Test");
			}
		}

		[TestMethod]
		public void Test_06_HeartBeats()
		{
			ManualResetEvent Ping = new ManualResetEvent(false);
			ManualResetEvent PingResp = new ManualResetEvent(false);

			this.Test_01_Connect();

			this.client.OnPing += (sender, e) => Ping.Set();
			this.client.OnPingResponse += (sender, e) => PingResp.Set();

			Assert.IsTrue(Ping.WaitOne(60000), "Ping not sent properly.");
			Assert.IsTrue(PingResp.WaitOne(60000), "Ping response not received properly.");
		}

	}
}
