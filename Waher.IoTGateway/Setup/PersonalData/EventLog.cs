namespace Waher.IoTGateway.Setup.PersonalData
{
	/// <summary>
	/// Information about event log.
	/// </summary>
	public class EventLog : IProcessingActivity
	{
		/// <summary>
		/// Priority of the processing activity. When the transparent information about all processing activities is assembled,
		/// they are presented in ascending priority order.
		/// </summary>
		public int Priority => 230;

		/// <summary>
		/// Filename of transparent information markdown for the processing activity.
		/// </summary>
		public string TransparentInformationMarkdownFileName => "EventLogs.md";
	}
}
