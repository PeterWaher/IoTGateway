using System.Diagnostics.Tracing;
using System.Xml;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Queue;

namespace Waher.Events.Syslog.Test
{
	[TestClass]
	public sealed class SyslogTests
	{
		private static ConsoleOutSniffer? sniffer;
		private static ConsoleEventSink? consoleSink;

		[AssemblyInitialize]
		public static Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(SyslogTests).Assembly,
				typeof(Log).Assembly,
				typeof(SyslogEventSink).Assembly);

			return Task.CompletedTask;
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Log.TerminateAsync();
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			consoleSink = new();
			Log.Register(consoleSink);

			sniffer = new(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine);
		}

		[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
		public static async Task ClassCleanup()
		{
			if (consoleSink is not null)
			{
				Log.Unregister(consoleSink);
				await consoleSink.DisposeAsync();
				consoleSink = null;
			}

			if (sniffer is not null)
			{
				await sniffer.FlushAsync();
				await sniffer.DisposeAsync();
				sniffer = null;
			}
		}

		[TestMethod]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf,
			EventType.Debug, EventLevel.Minor, "Debug message", "TestObject", "TestActor",
			"EventId123", "Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf,
			EventType.Informational, EventLevel.Minor, "Information message", "TestObject",
			"TestActor", "EventId123", "Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Notice,
			EventLevel.Minor, "Notice message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Warning,
			EventLevel.Minor, "Warning message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Error,
			EventLevel.Minor, "Error message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Critical,
			EventLevel.Minor, "Critical message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Alert,
			EventLevel.Minor, "Alert message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Emergency,
			EventLevel.Minor, "Emergency message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
		public async Task Test_01_EventZeroTags(string Host, int Port, bool Tls,
			SyslogEventSeparation Separation, EventType Type, EventLevel Level,
			string Message, string Object, string Actor, string EventId, string Facility,
			string Module, string StackTrace)
		{
			using SyslogClient Client = new(Host, Port, Tls, "LocalHost", "Unit Test",
				Separation, sniffer);

			await Client.Send(new Event(Type, Message, Object, Actor, EventId, Level,
				Facility, Module, StackTrace), true);

			await Task.Delay(1000);
		}

		[TestMethod]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf,
			EventType.Debug, EventLevel.Minor, "Debug message", "TestObject", "TestActor",
			"EventId123", "Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf,
			EventType.Informational, EventLevel.Minor, "Information message", "TestObject",
			"TestActor", "EventId123", "Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Notice,
			EventLevel.Minor, "Notice message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Warning,
			EventLevel.Minor, "Warning message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Error,
			EventLevel.Minor, "Error message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Critical,
			EventLevel.Minor, "Critical message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Alert,
			EventLevel.Minor, "Alert message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Emergency,
			EventLevel.Minor, "Emergency message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
		public async Task Test_02_EventOneTags(string Host, int Port, bool Tls,
			SyslogEventSeparation Separation, EventType Type, EventLevel Level,
			string Message, string Object, string Actor, string EventId, string Facility,
			string Module, string StackTrace, string Tag1Name, object Tag1Value)
		{
			using SyslogClient Client = new(Host, Port, Tls, "LocalHost", "Unit Test",
				Separation, sniffer);

			await Client.Send(new Event(Type, Message, Object, Actor, EventId, Level,
				Facility, Module, StackTrace,
				new KeyValuePair<string, object>(Tag1Name, Tag1Value)),
				true);

			await Task.Delay(1000);
		}
	}
}
