using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Content;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppSensorDataTests : CommunicationTests
	{
		private SensorClient sensorClient;
		private SensorServer sensorServer;
		private double temp;

		public override void ConnectClients()
		{
			base.ConnectClients();

			this.sensorClient = new SensorClient(this.client1);
			this.sensorServer = new SensorServer(this.client2, true);

			this.temp = 12.3;

			this.sensorServer.OnExecuteReadoutRequest += (sender, e) =>
			{
				DateTime Now = DateTime.Now;

				e.ReportFields(true,
					new QuantityField(ThingReference.Empty, Now, "Temperature", this.temp, 1, "C", FieldType.Momentary, FieldQoS.AutomaticReadout),
					new BooleanField(ThingReference.Empty, Now, "Bool", true, FieldType.Momentary, FieldQoS.AutomaticReadout),
					new DateField(ThingReference.Empty, Now, "Date", DateTime.Today, FieldType.Momentary, FieldQoS.AutomaticReadout),
					new DateTimeField(ThingReference.Empty, Now, "DateTime", Now, FieldType.Momentary, FieldQoS.AutomaticReadout),
					new DurationField(ThingReference.Empty, Now, "Duration", new Duration(true, 1, 2, 3, 4, 5, 6), FieldType.Momentary, FieldQoS.AutomaticReadout),
					new EnumField(ThingReference.Empty, Now, "Enum", TypeCode.Boolean, FieldType.Momentary, FieldQoS.AutomaticReadout),
					new Int32Field(ThingReference.Empty, Now, "Int32", int.MinValue, FieldType.Momentary, FieldQoS.AutomaticReadout),
					new Int64Field(ThingReference.Empty, Now, "Int64", long.MinValue, FieldType.Momentary, FieldQoS.AutomaticReadout),
					new StringField(ThingReference.Empty, Now, "String", "Hello world.", FieldType.Momentary, FieldQoS.AutomaticReadout),
					new TimeField(ThingReference.Empty, Now, "Time", Now.TimeOfDay, FieldType.Momentary, FieldQoS.AutomaticReadout));
			};
		}

		public override void DisposeClients()
		{
			if (this.sensorServer != null)
			{
				this.sensorServer.Dispose();
				this.sensorServer = null;
			}

			if (this.sensorClient != null)
			{
				this.sensorClient.Dispose();
				this.sensorClient = null;
			}

			base.DisposeClients();
		}

		[TestMethod]
		public void SensorData_Test_01_ReadAll()
		{
			this.ConnectClients();
			try
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

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 20000), "Readout not performed correctly");

				foreach (Field Field in Fields)
					Console.Out.WriteLine(Field.ToString());
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void SensorData_Test_02_Subscribe_MaxInterval()
		{
			this.ConnectClients();
			try
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
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void SensorData_Test_03_Subscribe_ChangeBy()
		{
			this.ConnectClients();
			try
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
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void SensorData_Test_04_Subscribe_ChangeUp()
		{
			this.ConnectClients();
			try
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
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void SensorData_Test_05_Subscribe_ChangeDown()
		{
			this.ConnectClients();
			try
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
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void SensorData_Test_06_Subscribe_MinInterval()
		{
			this.ConnectClients();
			try
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
				Assert.IsTrue(Elapsed > new TimeSpan(0, 0, 5));
			}
			finally
			{
				this.DisposeClients();
			}
		}
	}
}
