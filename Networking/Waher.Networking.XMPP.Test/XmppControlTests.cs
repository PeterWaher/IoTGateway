using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Control;
using Waher.Content;
using Waher.Runtime.Inventory;
using Waher.Things;
using Waher.Things.ControlParameters;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppControlTests : CommunicationTests
	{
		private ControlClient controlClient;
		private ControlServer controlServer;
		private bool b = false;
		private ColorReference cl = new ColorReference(0, 0, 0);
		private DateTime d = DateTime.Today;
		private DateTime dt = DateTime.Now;
		private double db = 0;
		private Duration dr = Duration.Zero;
		private TypeCode e = TypeCode.Boolean;
		private int i = 0;
		private long l = 0;
		private string s = string.Empty;
		private TimeSpan t = TimeSpan.Zero;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(
				typeof(XmppClient).Assembly,
				typeof(BOSH.HttpBinding).Assembly,
				typeof(WebSocket.WebSocketBinding).Assembly,
                typeof(P2P.EndpointSecurity).Assembly);
		}

		public override void ConnectClients()
		{
			base.ConnectClients();

			Assert.AreEqual(XmppState.Connected, this.client1.State);
			Assert.AreEqual(XmppState.Connected, this.client2.State);

			this.controlClient = new ControlClient(this.client1);
			this.controlServer = new ControlServer(this.client2,
				new BooleanControlParameter("Bool", "Page1", "Bool:", "Boolean value", (sender) => this.b, (sender, value) => this.b = value),
				new ColorControlParameter("Color", "Page1", "Color:", "Color value", (sender) => this.cl, (sender, value) => this.cl = value),
				new DateControlParameter("Date", "Page1", "Date:", "Date value", DateTime.MinValue, DateTime.MaxValue, (sender) => this.d, (sender, value) => this.d = value),
				new DateTimeControlParameter("DateTime", "Page1", "DateTime:", "DateTime value", DateTime.MinValue, DateTime.MaxValue, (sender) => this.dt, (sender, value) => this.dt = value),
				new DoubleControlParameter("Double", "Page1", "Double:", "Double value", (sender) => this.db, (sender, value) => this.db = value),
				new DurationControlParameter("Duration", "Page1", "Duration:", "Duration value", (sender) => this.dr, (sender, value) => this.dr = value),
				new EnumControlParameter("Enum", "Page1", "Enum:", "Enum value", typeof(TypeCode), (sender) => this.e, (sender, value) => this.e = (TypeCode)value),
				new Int32ControlParameter("Int32", "Page1", "Int32:", "Int32 value", (sender) => this.i, (sender, value) => this.i = value),
				new Int64ControlParameter("Int64", "Page1", "Int64:", "Int64 value", (sender) => this.l, (sender, value) => this.l = value),
				new StringControlParameter("String", "Page1", "String:", "String value", (sender) => this.s, (sender, value) => this.s = value),
				new TimeControlParameter("Time", "Page1", "Time:", "Time value", (sender) => this.t, (sender, value) => this.t = value));
		}

		public override void DisposeClients()
		{
			if (this.controlServer != null)
			{
				this.controlServer.Dispose();
				this.controlServer = null;
			}

			if (this.controlClient != null)
			{
				this.controlClient.Dispose();
				this.controlClient = null;
			}

			base.DisposeClients();
		}

		[TestMethod]
		public void Control_Test_01_Bool()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.b = false;

				this.controlClient.Set(this.client2.FullJID, "Bool", true, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(true, this.b);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_02_Color()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.cl = new ColorReference(0, 0, 0);

				this.controlClient.Set(this.client2.FullJID, "Color", new ColorReference(1, 2, 3), (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(1, this.cl.Red);
				Assert.AreEqual(2, this.cl.Green);
				Assert.AreEqual(3, this.cl.Blue);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_03_Date()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.d = DateTime.MinValue;

				this.controlClient.Set(this.client2.FullJID, "Date", DateTime.Today, true, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(DateTime.Today, this.d);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_04_DateTime()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				DateTime Now = DateTime.Now;
				this.dt = DateTime.MinValue;

				Now = new DateTime(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, Now.Second, Now.Millisecond);

				this.controlClient.Set(this.client2.FullJID, "DateTime", Now, false, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(Now.ToUniversalTime().Ticks, this.dt.ToUniversalTime().Ticks);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_05_Double()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.db = 0;

				this.controlClient.Set(this.client2.FullJID, "Double", 3.1415927, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(3.1415927, this.db);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_06_Duration()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.dr = Duration.Zero;

				this.controlClient.Set(this.client2.FullJID, "Duration", new Duration(true, 1, 2, 3, 4, 5, 6), (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(new Duration(true, 1, 2, 3, 4, 5, 6), this.dr);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_07_Enum()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.e = TypeCode.Boolean;

				this.controlClient.Set(this.client2.FullJID, "Enum", TypeCode.Int16, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(TypeCode.Int16, this.e);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_08_Int32()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.i = 0;

				this.controlClient.Set(this.client2.FullJID, "Int32", int.MinValue, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(int.MinValue, this.i);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_09_Int64()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.l = 0;

				this.controlClient.Set(this.client2.FullJID, "Int64", long.MinValue, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(long.MinValue, this.l);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_10_String()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				this.s = string.Empty;

				this.controlClient.Set(this.client2.FullJID, "String", "ABC", (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual("ABC", this.s);
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void Control_Test_11_Time()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				TimeSpan Time = DateTime.Now.TimeOfDay;
				this.t = TimeSpan.Zero;

				this.controlClient.Set(this.client2.FullJID, "Time", Time, (sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000), "Configuration not performed correctly");
				Assert.AreEqual(Time, this.t);
			}
			finally
			{
				this.DisposeClients();
			}
		}

	}
}
