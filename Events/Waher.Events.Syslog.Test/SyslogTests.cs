using System.Security.Authentication;

namespace Waher.Events.Syslog.Test
{
	[TestClass]
	public sealed class SyslogTests
	{
		[TestMethod]
		[DataRow("localhost", 514, false, EventType.Debug, EventLevel.Minor, 
			"Debug message", "TestObject", "TestActor", "EventId123", "Facility1", 
			"Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, EventType.Informational, EventLevel.Minor,
			"Information message", "TestObject", "TestActor", "EventId123", "Facility1",
			"Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, EventType.Notice, EventLevel.Minor,
			"Notice message", "TestObject", "TestActor", "EventId123", "Facility1",
			"Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, EventType.Warning, EventLevel.Minor,
			"Warning message", "TestObject", "TestActor", "EventId123", "Facility1",
			"Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, EventType.Error, EventLevel.Minor,
			"Error message", "TestObject", "TestActor", "EventId123", "Facility1",
			"Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, EventType.Critical, EventLevel.Minor,
			"Critical message", "TestObject", "TestActor", "EventId123", "Facility1",
			"Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, EventType.Alert, EventLevel.Minor,
			"Alert message", "TestObject", "TestActor", "EventId123", "Facility1",
			"Module1", "StackTrace1")]
		[DataRow("localhost", 514, false, EventType.Emergency, EventLevel.Minor,
			"Emergency message", "TestObject", "TestActor", "EventId123", "Facility1",
			"Module1", "StackTrace1")]
		public async Task Test_01_EventZeroTags(string Host, int Port, bool Tls,
			EventType Type, EventLevel Level, string Message, string Object, string Actor,
			string EventId, string Facility, string Module, string StackTrace)
		{
			using SyslogClient Client = new(Host, Port, Tls, "LocalHost", "Unit Test");

			await Client.Send(new Event(Type, Message, Object, Actor, EventId, Level, 
				Facility, Module, StackTrace));
		}
	}
}
