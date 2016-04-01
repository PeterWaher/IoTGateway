using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Test
{
	[TestFixture]
	public class SensorDataTests : CommunicationTests
	{
		private SensorClient sensorClient;
		private SensorServer sensorServer;

		public override void Setup()
		{
			base.Setup();

			this.sensorClient = new SensorClient(this.client1);
			this.sensorServer = new SensorServer(this.client2, true);

			this.sensorServer.OnExecuteReadoutRequest += (sender, e) =>
			{
				DateTime Now = DateTime.Now;

				e.ReportFields(true,
					new QuantityField(ThingReference.Empty, Now, "Temperature", 12.3, 1, "C", FieldType.Momentary, FieldQoS.AutomaticReadout));
			};
		}

		public override void TearDown()
		{
			this.sensorServer.Dispose();
			this.sensorServer = null;

			this.sensorClient.Dispose();
			this.sensorClient = null;

			base.TearDown();
		}

		[Test]
		public void Test_01_ReadAll()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			IEnumerable<Field> Fields = null;

			SensorDataClientRequest Request = this.sensorClient.RequestReadout(this.client2.FullJID, FieldType.All);
			Request.OnStateChanged += (sender, NewState) => Console.Out.WriteLine(NewState.ToString());
			Request.OnErrorsReceived += (sender, Errors) => Error.Set();
			Request.OnFieldsReceived += (sender, NewFields) =>
			{
				Fields = NewFields;
				Done.Set();
			};

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Readout not performed correctly");

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		[Test]
		public void Test_02_Subscribe()
		{
		}
	}
}
