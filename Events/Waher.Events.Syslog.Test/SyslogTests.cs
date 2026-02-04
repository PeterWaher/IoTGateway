using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Runtime.Inventory;
using Waher.Runtime.Threading;

namespace Waher.Events.Syslog.Test
{
	/// <summary>
	/// Tests run using a Visual Syslog Server as recipient.
	/// https://github.com/MaxBelkov/visualsyslog/
	/// </summary>
	[TestClass]
	public sealed class SyslogTests
	{
		private static TextWriterSniffer? sniffer;
		private static ConsoleEventSink? consoleSink;
		private static SyslogClient? client;

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
		public static void ClassInitialize(TestContext Context)
		{
			consoleSink = new();
			Log.Register(consoleSink);
			
			sniffer = new TextWriterSniffer(new TestContextWriter(Context), 
				BinaryPresentationMethod.Hexadecimal, "TestContext");
		}

		[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
		public static async Task ClassCleanup()
		{
			if (client is not null)
			{
				await client.DisposeAsync();
				client = null;
			}

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
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Debug,
			EventLevel.Minor, "Debug message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Informational,
			EventLevel.Minor, "Information message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1")]
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
			using Runtime.Threading.Semaphore SyslogSemaphore = await Semaphores.BeginWrite("SyslogClient");

			await SetupClient(Host, Port, Tls, Separation);

			await client!.Send(new Event(Type, Message, Object, Actor, EventId, Level,
				Facility, Module, StackTrace), true);
		}

		private static async Task SetupClient(string Host, int Port, bool Tls,
			SyslogEventSeparation Separation)
		{
			if (client is not null &&
				(client.Host != Host ||
				client.Port != Port ||
				client.Tls != Tls ||
				client.Separation != Separation))
			{
				await client.DisposeAsync();
				client = null;
			}

			client ??= new SyslogClient(Host, Port, Tls, "LocalHost", "Unit Test",
				Separation, sniffer);
		}

		[TestMethod]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Debug,
			EventLevel.Minor, "Debug message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Informational,
			EventLevel.Minor, "Information message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1)]
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
		public async Task Test_02_EventOneTag(string Host, int Port, bool Tls,
			SyslogEventSeparation Separation, EventType Type, EventLevel Level,
			string Message, string Object, string Actor, string EventId, string Facility,
			string Module, string StackTrace, string Tag1Name, object Tag1Value)
		{
			using Runtime.Threading.Semaphore SyslogSemaphore = await Semaphores.BeginWrite("SyslogClient");

			await SetupClient(Host, Port, Tls, Separation);

			await client!.Send(new Event(Type, Message, Object, Actor, EventId, Level,
				Facility, Module, StackTrace,
				new KeyValuePair<string, object>(Tag1Name, Tag1Value)),
				true);
		}

		[TestMethod]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Debug,
			EventLevel.Minor, "Debug message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Informational,
			EventLevel.Minor, "Information message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Notice,
			EventLevel.Minor, "Notice message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Warning,
			EventLevel.Minor, "Warning message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Error,
			EventLevel.Minor, "Error message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Critical,
			EventLevel.Minor, "Critical message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Alert,
			EventLevel.Minor, "Alert message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		[DataRow("localhost", 514, false, SyslogEventSeparation.CrLf, EventType.Emergency,
			EventLevel.Minor, "Emergency message", "TestObject", "TestActor", "EventId123",
			"Facility1", "Module1", "StackTrace1", "A", 1, "B", 2)]
		public async Task Test_03_EventTwoTags(string Host, int Port, bool Tls,
			SyslogEventSeparation Separation, EventType Type, EventLevel Level,
			string Message, string Object, string Actor, string EventId, string Facility,
			string Module, string StackTrace, string Tag1Name, object Tag1Value,
			string Tag2Name, object Tag2Value)
		{
			using Runtime.Threading.Semaphore SyslogSemaphore = await Semaphores.BeginWrite("SyslogClient");

			await SetupClient(Host, Port, Tls, Separation);

			await client!.Send(new Event(Type, Message, Object, Actor, EventId, Level,
				Facility, Module, StackTrace,
				new KeyValuePair<string, object>(Tag1Name, Tag1Value),
				new KeyValuePair<string, object>(Tag2Name, Tag2Value)),
				true);
		}
	}
}
