using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Content;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Test
{
	[TestFixture]
	public class SensorDataTests : CommunicationTests
	{
		private SensorClient sensorClient;
		private SensorServer sensorServer;
		private double temp;

		public override void Setup()
		{
			base.Setup();

			this.sensorClient = new SensorClient(this.client1);
			this.sensorServer = new SensorServer(this.client2, true);

			this.temp = 12.3;

			this.sensorServer.OnExecuteReadoutRequest += (sender, e) =>
			{
				DateTime Now = DateTime.Now;

				e.ReportFields(true,
					new QuantityField(ThingReference.Empty, Now, "Temperature", this.temp, 1, "C", FieldType.Momentary, FieldQoS.AutomaticReadout));
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
		public void Test_02_Subscribe_MaxInterval()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			IEnumerable<Field> Fields = null;

			SensorDataClientRequest Request = this.sensorClient.Subscribe(this.client2.FullJID, FieldType.All, 
				Duration.Parse("PT1S"), Duration.Parse("PT5S"), false);
			Request.OnStateChanged += (sender, NewState) => Console.Out.WriteLine(NewState.ToString());
			Request.OnErrorsReceived += (sender, Errors) => Error.Set();
			Request.OnFieldsReceived += (sender, NewFields) =>
			{
				Fields = NewFields;
				Done.Set();
			};

			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C", 
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Subscription not performed correctly");

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		[Test]
		public void Test_03_Subscribe_ChangeBy()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			IEnumerable<Field> Fields = null;

			SensorDataClientRequest Request = this.sensorClient.Subscribe(this.client2.FullJID, FieldType.All,
				new FieldSubscriptionRule[]
				{
					new FieldSubscriptionRule("Temperature", this.temp, 1)
				},
				Duration.Parse("PT1S"), Duration.Parse("PT5S"), false);
			Request.OnStateChanged += (sender, NewState) => Console.Out.WriteLine(NewState.ToString());
			Request.OnErrorsReceived += (sender, Errors) => Error.Set();
			Request.OnFieldsReceived += (sender, NewFields) =>
			{
				Fields = NewFields;
				Done.Set();
			};

			this.temp += 0.5;
			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Thread.Sleep(2000);

			this.temp += 0.5;
			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Subscription not performed correctly");

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());

			Done.Reset();

			this.temp -= 1;
			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Subscription not performed correctly");

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		[Test]
		public void Test_04_Subscribe_ChangeUp()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			IEnumerable<Field> Fields = null;

			SensorDataClientRequest Request = this.sensorClient.Subscribe(this.client2.FullJID, FieldType.All,
				new FieldSubscriptionRule[]
				{
					new FieldSubscriptionRule("Temperature", this.temp, 1, null)
				},
				Duration.Parse("PT1S"), Duration.Parse("PT5S"), false);
			Request.OnStateChanged += (sender, NewState) => Console.Out.WriteLine(NewState.ToString());
			Request.OnErrorsReceived += (sender, Errors) => Error.Set();
			Request.OnFieldsReceived += (sender, NewFields) =>
			{
				Fields = NewFields;
				Done.Set();
			};

			this.temp -= 1;
			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Thread.Sleep(2000);

			this.temp += 2;
			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Subscription not performed correctly");

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		[Test]
		public void Test_05_Subscribe_ChangeDown()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			IEnumerable<Field> Fields = null;

			SensorDataClientRequest Request = this.sensorClient.Subscribe(this.client2.FullJID, FieldType.All,
				new FieldSubscriptionRule[]
				{
					new FieldSubscriptionRule("Temperature", this.temp, 1, null)
				},
				Duration.Parse("PT1S"), Duration.Parse("PT5S"), false);
			Request.OnStateChanged += (sender, NewState) => Console.Out.WriteLine(NewState.ToString());
			Request.OnErrorsReceived += (sender, Errors) => Error.Set();
			Request.OnFieldsReceived += (sender, NewFields) =>
			{
				Fields = NewFields;
				Done.Set();
			};

			this.temp += 1;
			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Thread.Sleep(2000);

			this.temp -= 2;
			this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
				FieldType.Momentary, FieldQoS.AutomaticReadout));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Subscription not performed correctly");

			foreach (Field Field in Fields)
				Console.Out.WriteLine(Field.ToString());
		}

		[Test]
		public void Test_06_Subscribe_MinInterval()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			IEnumerable<Field> Fields = null;

			SensorDataClientRequest Request = this.sensorClient.Subscribe(this.client2.FullJID, FieldType.All,
				new FieldSubscriptionRule[]
				{
					new FieldSubscriptionRule("Temperature", this.temp, 1, null)
				},
				Duration.Parse("PT1S"), Duration.Parse("PT5S"), false);
			Request.OnStateChanged += (sender, NewState) => Console.Out.WriteLine(NewState.ToString());
			Request.OnErrorsReceived += (sender, Errors) => Error.Set();
			Request.OnFieldsReceived += (sender, NewFields) =>
			{
				Fields = NewFields;
				Done.Set();
			};

			int Count = 6;
			DateTime Start = DateTime.Now;

			while (Count > 0)
			{
				this.temp += 1;
				this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
					FieldType.Momentary, FieldQoS.AutomaticReadout));

				this.temp -= 1;
				this.sensorServer.NewMomentaryValues(new QuantityField(ThingReference.Empty, DateTime.Now, "Temperature", this.temp, 1, "C",
					FieldType.Momentary, FieldQoS.AutomaticReadout));

				switch (WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 100))
				{
					case 0:
						Done.Reset();
						Count--;
						break;

					case 1:
						Assert.Fail("Subscription not performed correctly");
						break;
				}
			}

			TimeSpan Elapsed = DateTime.Now - Start;
			Assert.Greater(Elapsed, new TimeSpan(0, 0, 5));
		}
	}
}
