using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Waher.Runtime.Inventory;
using Waher.Runtime.Queue;

namespace Waher.Events.Socket.Test
{
	[TestClass]
	public class SocketTests
	{
		private static SocketEventSink? eventSink;
		private static SocketEventRecipient? eventListener;
		private static AsyncQueue<Event>? eventsReceived;
		private static AsyncQueue<XmlDocument>? customFragmentsReceived;

		[AssemblyInitialize]
		public static Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(SocketTests).Assembly,
				typeof(Log).Assembly,
				typeof(SocketEventSink).Assembly);

			return Task.CompletedTask;
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			Log.Terminate();
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			eventsReceived = new AsyncQueue<Event>();
			customFragmentsReceived = new AsyncQueue<XmlDocument>();

			eventListener = await SocketEventRecipient.Create(8081, false);
			eventListener.EventReceived += EventListener_EventReceived;
			eventListener.CustomFragmentReceived += EventListener_CustomFragmentReceived;

			eventSink = new SocketEventSink("Event Sink", "localhost", 8081, false);
			Log.Register(eventSink);
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (eventSink is not null)
			{
				Log.Unregister(eventSink);
				eventSink.Dispose();
				eventSink = null;
			}

			eventListener?.Dispose();
			eventListener = null;

			eventsReceived?.Dispose();
			eventsReceived = null;

			customFragmentsReceived?.Dispose();
			customFragmentsReceived = null;
		}

		private static Task EventListener_EventReceived(object Sender, EventEventArgs e)
		{
			eventsReceived?.Add(e.Event);
			return Task.CompletedTask;
		}

		private static Task EventListener_CustomFragmentReceived(object Sender, CustomFragmentEventArgs e)
		{
			customFragmentsReceived?.Add(e.Fragment);
			return Task.CompletedTask;
		}

		[DataTestMethod]
		[DataRow(EventType.Debug)]
		[DataRow(EventType.Informational)]
		[DataRow(EventType.Notice)]
		[DataRow(EventType.Warning)]
		[DataRow(EventType.Error)]
		[DataRow(EventType.Critical)]
		[DataRow(EventType.Alert)]
		[DataRow(EventType.Emergency)]
		public async Task Test_01_EventTest(EventType Type)
		{
			Event Event = new(new DateTime(1234, 1, 2, 3, 4, 5), Type,
				"Multiple\r\nRow\r\nMessage.", "Obj", "Test", "TestEvent",
				EventLevel.Medium, "UnitTest", "Test_01", "1\r\n2\r\n3\r\n4",
				new KeyValuePair<string, object>("Test1", "Value1"),
				new KeyValuePair<string, object>("Test2", true),
				new KeyValuePair<string, object>("Test3", 123),
				new KeyValuePair<string, object>("Test4", 123.4));

			Log.Event(Event);

			Assert.IsNotNull(eventsReceived);
			Event Event2 = await eventsReceived.Wait();

			Assert.AreEqual(Event.Type, Event2.Type);
			Assert.AreEqual(Event.Level, Event2.Level);
			Assert.AreEqual(Event.Message, Event2.Message);
			Assert.AreEqual(Event.Timestamp, Event2.Timestamp);
			Assert.AreEqual(Event.Object, Event2.Object);
			Assert.AreEqual(Event.Actor, Event2.Actor);
			Assert.AreEqual(Event.EventId, Event2.EventId);
			Assert.AreEqual(Event.Facility, Event2.Facility);
			Assert.AreEqual(Event.Module, Event2.Module);
			Assert.AreEqual(Event.Tags.Length, Event2.Tags.Length);

			int i;

			for (i = 0; i < 4; i++)
			{
				Assert.AreEqual(Event.Tags[i].Key, Event2.Tags[i].Key);
				Assert.AreEqual(Event.Tags[i].Value, Event2.Tags[i].Value);
			}

			if (Type >= EventType.Critical)
				Assert.AreEqual(Event.StackTrace, Event2.StackTrace);
		}

		[TestMethod]
		public async Task Test_02_Custom()
		{
			Assert.IsNotNull(eventSink);
			await eventSink.Queue("<hello><world>Kilroy was here.</world></hello>");

			Assert.IsNotNull(customFragmentsReceived);
			XmlDocument Doc = await customFragmentsReceived.Wait();

			Assert.IsNotNull(Doc);
			Assert.IsNotNull("<hello><world>Kilroy was here.</world></hello>", Doc.OuterXml);
		}
	}
}