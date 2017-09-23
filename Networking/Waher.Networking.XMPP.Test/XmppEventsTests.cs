using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Events;
using Waher.Events.XMPP;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppEventsTests : CommunicationTests
	{
		XmppEventReceptor receptor = null;
		XmppEventSink sink = null;

		[TestInitialize]
		public override void Setup()
		{
			base.Setup();

			this.receptor = new XmppEventReceptor(this.client2);
			this.sink = new XmppEventSink("XMPP Event Sink", this.client1, this.client2.FullJID, true);

			Log.Register(this.sink);
		}

		[TestCleanup]
		public override void TearDown()
		{
			if (this.sink != null)
			{
				Log.Unregister(this.sink);
				this.sink.Dispose();
				this.sink = null;
			}

			if (this.receptor != null)
			{
				this.receptor.Dispose();
				this.receptor = null;
			}

			base.TearDown();
		}

		[TestMethod]
		public void Events_Test_01_LogEvent()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			Event Event = null;

			this.receptor.OnEvent += (sender, e) =>
			{
				Event = e.Event;
				Done.Set();
			};

			Log.Debug("Debug message", "Test Object", "Test Actor", "Test Event ID", EventLevel.Medium, "Test Facility", "Test Module", "Test Stack Trace",
				new KeyValuePair<string, object>("Boolean", true),
				new KeyValuePair<string, object>("Byte", (byte)2),
				new KeyValuePair<string, object>("Int16", (short)3),
				new KeyValuePair<string, object>("Int32", (int)4),
				new KeyValuePair<string, object>("Int64", (long)5),
				new KeyValuePair<string, object>("SByte", (sbyte)6),
				new KeyValuePair<string, object>("UInt16", (ushort)7),
				new KeyValuePair<string, object>("UInt32", (uint)8),
				new KeyValuePair<string, object>("UInt64", (ulong)9),
				new KeyValuePair<string, object>("Decimal", (decimal)9.1),
				new KeyValuePair<string, object>("Double", (double)9.2),
				new KeyValuePair<string, object>("Single", (float)9.3),
				new KeyValuePair<string, object>("DateTime", new DateTime(2015, 12, 23, 22, 44, 05)),
				new KeyValuePair<string, object>("String", "Hola Bandola"),
				new KeyValuePair<string, object>("Char", 'a'),
				new KeyValuePair<string, object>("TimeSpan", new TimeSpan(22, 44, 05)),
				new KeyValuePair<string, object>("Uri", new Uri("http://waher.se/")),
				new KeyValuePair<string, object>("Obj", this));

			Assert.IsTrue(Done.WaitOne(10000), "Event not propagated properly.");
			Assert.AreEqual(EventType.Debug, Event.Type);
			Assert.AreEqual("Debug message", Event.Message);
			Assert.AreEqual("Test Object", Event.Object);
			Assert.AreEqual("Test Actor", Event.Actor);
			Assert.AreEqual("Test Event ID", Event.EventId);
			Assert.AreEqual(EventLevel.Medium, Event.Level);
			Assert.AreEqual("Test Facility", Event.Facility);
			Assert.AreEqual("Test Module", Event.Module);
			Assert.AreEqual("Test Stack Trace", Event.StackTrace);
			Assert.AreEqual(18, Event.Tags.Length);

			Assert.AreEqual("Boolean", Event.Tags[0].Key);
			Assert.AreEqual(true, Event.Tags[0].Value);

			Assert.AreEqual("Byte", Event.Tags[1].Key);
			Assert.AreEqual((byte)2, Event.Tags[1].Value);

			Assert.AreEqual("Int16", Event.Tags[2].Key);
			Assert.AreEqual((short)3, Event.Tags[2].Value);

			Assert.AreEqual("Int32", Event.Tags[3].Key);
			Assert.AreEqual((int)4, Event.Tags[3].Value);

			Assert.AreEqual("Int64", Event.Tags[4].Key);
			Assert.AreEqual((long)5, Event.Tags[4].Value);

			Assert.AreEqual("SByte", Event.Tags[5].Key);
			Assert.AreEqual((sbyte)6, Event.Tags[5].Value);

			Assert.AreEqual("UInt16", Event.Tags[6].Key);
			Assert.AreEqual((ushort)7, Event.Tags[6].Value);

			Assert.AreEqual("UInt32", Event.Tags[7].Key);
			Assert.AreEqual((uint)8, Event.Tags[7].Value);

			Assert.AreEqual("UInt64", Event.Tags[8].Key);
			Assert.AreEqual((ulong)9, Event.Tags[8].Value);

			Assert.AreEqual("Decimal", Event.Tags[9].Key);
			Assert.AreEqual((decimal)9.1, Event.Tags[9].Value);

			Assert.AreEqual("Double", Event.Tags[10].Key);
			Assert.AreEqual((double)9.2, Event.Tags[10].Value);

			Assert.AreEqual("Single", Event.Tags[11].Key);
			Assert.AreEqual((float)9.3, Event.Tags[11].Value);

			Assert.AreEqual("DateTime", Event.Tags[12].Key);
			Assert.AreEqual(new DateTime(2015, 12, 23, 22, 44, 05), Event.Tags[12].Value);

			Assert.AreEqual("String", Event.Tags[13].Key);
			Assert.AreEqual("Hola Bandola", Event.Tags[13].Value);

			Assert.AreEqual("Char", Event.Tags[14].Key);
			Assert.AreEqual("a", Event.Tags[14].Value);	// XML does not differentiate between single characters and entire strings.

			Assert.AreEqual("TimeSpan", Event.Tags[15].Key);
			Assert.AreEqual(new TimeSpan(22, 44, 05), Event.Tags[15].Value);

			Assert.AreEqual("Uri", Event.Tags[16].Key);
			Assert.AreEqual(new Uri("http://waher.se/"), Event.Tags[16].Value);

			Assert.AreEqual("Obj", Event.Tags[17].Key);
			Assert.AreEqual(this.GetType().FullName, Event.Tags[17].Value);
		}
	}
}
