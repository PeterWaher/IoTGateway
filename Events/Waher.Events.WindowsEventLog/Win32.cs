using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Waher.Events.WindowsEventLog
{
	/// <summary>
	/// Windows event type enumeration.
	/// </summary>
	public enum WindowsEventType : ushort
	{
		/// <summary>
		/// Information event
		/// </summary>
		EVENTLOG_SUCCESS = 0x0000,

		/// <summary>
		/// Error event
		/// </summary>
		EVENTLOG_ERROR_TYPE = 0x0001,

		/// <summary>
		/// Warning event
		/// </summary>
		EVENTLOG_WARNING_TYPE = 0x0002,

		/// <summary>
		/// Information event
		/// </summary>
		EVENTLOG_INFORMATION_TYPE = 0x0004,

		/// <summary>
		/// Success Audit event
		/// </summary>
		EVENTLOG_AUDIT_SUCCESS = 0x0008,

		/// <summary>
		/// Failure Audit event
		/// </summary>
		EVENTLOG_AUDIT_FAILURE = 0x0010
	}

	/// <summary>
	/// Read event flags.
	/// </summary>
	[Flags]
	public enum EventLogReadFlags : uint
	{
		/// <summary>
		/// Read the records sequentially. If this is the first read operation, the <see cref="EVENTLOG_FORWARDS_READ"/> <see cref="EVENTLOG_BACKWARDS_READ"/>
		/// flags determines which record is read first.
		/// </summary>
		EVENTLOG_SEQUENTIAL_READ = 0x0001,

		/// <summary>
		/// Begin reading from the record specified in the dwRecordOffset parameter. 
		/// This option may not work with large log files if the function cannot determine the log file's size. For details, see Knowledge Base 
		/// article, 177199.
		/// </summary>
		EVENTLOG_SEEK_READ = 0x0002,

		/// <summary>
		/// The log is read in chronological order (oldest to newest). The default.
		/// </summary>
		EVENTLOG_FORWARDS_READ = 0x0004,

		/// <summary>
		/// The log is read in reverse chronological order (newest to oldest). 
		/// </summary>
		EVENTLOG_BACKWARDS_READ = 0x0008
	}

	/// <summary>
	/// Handles interaction with the Windows Event Log API.
	/// </summary>
	public static class Win32
	{
		/// <summary>
		/// Retrieves a registered handle to the specified event log.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363678(v=vs.85).aspx"/>
		/// <remarks>
		/// If the source name cannot be found, the event logging service uses the Application log. Although events will be reported, 
		/// the events will not include descriptions because there are no message and category message files for looking up descriptions 
		/// related to the event identifiers.
		/// 
		/// To close the handle to the event log, use the DeregisterEventSource function.</remarks>
		/// <param name="lpUNCServerName">The Universal Naming Convention (UNC) name of the remote server on which this operation is to be performed. 
		/// If this parameter is NULL, the local computer is used.</param>
		/// <param name="lpSourceName">The name of the event source whose handle is to be retrieved. The source name must be a subkey of a log under 
		/// the Eventlog registry key. Note that the Security log is for system use only.
		/// 
		/// Note: This string must not contain characters prohibited in XML Attributes, with the exception of XML Escape sequences such as &amp;lt &amp;gl.</param>
		/// <returns>If the function succeeds, the return value is a handle to the event log.
		/// 
		/// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
		/// 
		/// The function returns ERROR_ACCESS_DENIED if lpSourceName specifies the Security event log.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr RegisterEventSourceW(string lpUNCServerName, string lpSourceName);

		/// <summary>
		/// Closes the specified event log.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363642(v=vs.85).aspx"/>
		/// <param name="hEventLog">A handle to the event log. The <see cref="RegisterEventSourceW"/> function returns this handle.</param>
		/// <returns>If the function succeeds, the return value is nonzero.
		/// 
		/// If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool DeregisterEventSource(IntPtr hEventLog);

		/// <summary>
		/// Writes an entry at the end of the specified event log.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363679(v=vs.85).aspx"/>
		/// <remarks>
		/// This function is used to log an event. The entry is written to the end of the configured log for the source identified by the hEventLog parameter. 
		/// The ReportEvent function adds the time, the entry's length, and the offsets before storing the entry in the log. To enable the function to add 
		/// the user name, you must supply the user's SID in the lpUserSid parameter.
		/// 
		/// There are different size limits on the size of the message data that can be logged depending on the version of Windows used by both the client 
		/// where the application is run and the server where the message is logged. The server is determined by the lpUNCServerName parameter passed to the 
		/// <see cref="RegisterEventSourceW"/> function. Different errors are returned when the size limit is exceeded that depend on the version of Windows.
		/// 
		/// If the string that you log contains %n, where n is an integer value (for example, %1), the event viewer treats it as an insertion string. Because 
		/// an IPv6 address can contain this character sequence, you must provide a format specifier(!S!) to log an event message that contains an IPv6 address.
		/// This specifier tells the formatting code to use the string literally and not perform any further expansions (for example, 
		/// "my IPv6 address is: %1!S!").
		/// </remarks>
		/// <param name="hEventLog">A handle to the event log. The RegisterEventSource function returns this handle. 
		/// 
		/// As of Windows XP with SP2, this parameter cannot be a handle to the Security log. To write an event to the Security log, 
		/// use the AuthzReportSecurityEvent function.</param>
		/// <param name="wType">The type of event to be logged. For more information about event types, see 
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363662(v=vs.85).aspx"/>.</param>
		/// <param name="wCategory">The event category. This is source-specific information; the category can have any value. For more information, see 
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363649(v=vs.85).aspx"/>.</param>
		/// <param name="dwEventID">The event identifier. The event identifier specifies the entry in the message file associated with the event source. 
		/// For more information, see <see href="https://msdn.microsoft.com/en-us/library/aa363651(v=vs.85).aspx"/>.</param>
		/// <param name="lpUserSid">A pointer to the current user's security identifier. This parameter can be NULL if the security identifier is 
		/// not required.</param>
		/// <param name="wNumStrings">The number of insert strings in the array pointed to by the <paramref name="lpStrings"/> parameter. A value of 
		/// zero indicates that no strings are present.</param>
		/// <param name="dwDataSize">The number of bytes of event-specific raw (binary) data to write to the log. If this parameter is zero, no 
		/// event-specific data is present.</param>
		/// <param name="lpStrings">A pointer to a buffer containing an array of null-terminated strings that are merged into the message before 
		/// Event Viewer displays the string to the user. This parameter must be a valid pointer (or NULL), even if wNumStrings is zero. Each string 
		/// is limited to 31,839 characters.</param>
		/// <param name="lpRawData">A pointer to the buffer containing the binary data. This parameter must be a valid pointer (or NULL), even if the 
		/// dwDataSize parameter is zero.</param>
		/// <returns>If the function succeeds, the return value is nonzero, indicating that the entry was written to the log.
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError, which returns one of the 
		/// following extended error codes.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool ReportEventW(IntPtr hEventLog, WindowsEventType wType, ushort wCategory, uint dwEventID,
			IntPtr lpUserSid, ushort wNumStrings, uint dwDataSize, string[] lpStrings, IntPtr lpRawData);

		/// <summary>
		/// Clears the specified event log, and optionally saves the current copy of the log to a backup file.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363637(v=vs.85).aspx"/>
		/// <remarks>After this function returns, any handles that reference the cleared event log cannot be used to read the log.</remarks>
		/// <param name="hEventLog">A handle to the event log to be cleared. The <see cref="OpenEventLogW"/> function returns this handle.</param>
		/// <param name="lpBackupFileName">The absolute or relative path of the backup file. If this file already exists, the function fails.
		/// 
		/// If the lpBackupFileName parameter is NULL, the event log is not backed up.</param>
		/// <returns>If the function succeeds, the return value is nonzero.
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError. The <see cref="ClearEventLogW"/>
		/// function can fail if the event log is empty or the backup file already exists.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool ClearEventLogW(IntPtr hEventLog, string lpBackupFileName);

		/// <summary>
		/// Closes the specified event log.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363639(v=vs.85).aspx"/>
		/// <param name="hEventLog">A handle to the event log to be closed. The <see cref="OpenEventLogW"/> or <see cref="OpenBackupEventLogW"/> function 
		/// returns this handle.</param>
		/// <returns>If the function succeeds, the return value is nonzero. 
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool CloseEventLog(IntPtr hEventLog);

		/// <summary>
		/// Opens a handle to the specified event log.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363672(v=vs.85).aspx"/>
		/// <remarks>To close the handle to the event log, use the <see cref="CloseEventLog"/> function.</remarks>
		/// <param name="lpUNCServerName">The Universal Naming Convention (UNC) name of the remote server on which the event log is to be opened. 
		/// If this parameter is NULL, the local computer is used.</param>
		/// <param name="lpSourceName">The name of the log.
		/// 
		/// If you specify a custom log and it cannot be found, the event logging service opens the Application log; however, there will be no 
		/// associated message or category string file.</param>
		/// <returns>If the function succeeds, the return value is the handle to an event log. 
		/// 
		/// If the function fails, the return value is NULL.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr OpenEventLogW(string lpUNCServerName, string lpSourceName);

		/// <summary>
		/// Opens a handle to a backup event log created by the BackupEventLog function.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363671(v=vs.85).aspx"/>
		/// <remarks>If the backup filename specifies a remote server, the lpUNCServerName parameter must be NULL.
		/// 
		/// When this function is used on Windows Vista and later computers, only backup event logs that were saved with the BackupEventLog function 
		/// on WindowsVista and later computers can be opened.</remarks>
		/// <param name="lpUNCServerName">The Universal Naming Convention (UNC) name of the remote server on which this operation is to be performed.
		/// If this parameter is NULL, the local computer is used.</param>
		/// <param name="lpFileName">The full path of the backup file.</param>
		/// <returns>If the function succeeds, the return value is a handle to the backup event log. 
		/// 
		/// If the function fails, the return value is NULL.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr OpenBackupEventLogW(string lpUNCServerName, string lpFileName);

		/// <summary>
		/// Reads the specified number of entries from the specified event log. The function can be used to read log entries in chronological
		/// or reverse chronological order.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363674(v=vs.85).aspx"/>
		/// <remarks>
		/// When this function returns successfully, the read position in the event log is adjusted by the number of records read.
		/// 
		/// Note The configured file name for this source may also be the configured file name for other sources(several sources can exist as subkeys 
		/// under a single log). Therefore, this function may return events that were logged by more than one source.
		/// </remarks>
		/// <param name="hEventLog">A handle to the event log to be read. The <see cref="OpenEventLogW"/> function returns this handle.</param>
		/// <param name="dwReadFlags">Use the following flag values to indicate how to read the log file. This parameter must include one of the following values 
		/// (the flags are mutually exclusive): <see cref="EventLogReadFlags.EVENTLOG_SEEK_READ"/>, <see cref="EventLogReadFlags.EVENTLOG_SEQUENTIAL_READ"/>
		/// 
		/// You must specify one of the following flags to indicate the direction for successive read operations (the flags are mutually exclusive):
		/// <see cref="EventLogReadFlags.EVENTLOG_FORWARDS_READ"/>, <see cref="EventLogReadFlags.EVENTLOG_FORWARDS_READ"/></param>
		/// <param name="dwRecordOffset">The record number of the log-entry at which the read operation should start. This parameter is ignored unless 
		/// <paramref name="dwReadFlags"/> includes the 
		/// <see cref="EventLogReadFlags.EVENTLOG_SEEK_READ"/> flag.</param>
		/// <param name="lpBuffer">An application-allocated buffer that will receive one or more EVENTLOGRECORD structures. This parameter 
		/// cannot be NULL, even if the <paramref name="nNumberOfBytesToRead"/> parameter is zero. 
		/// 
		/// The maximum size of this buffer is 0x7ffff bytes.</param>
		/// <param name="nNumberOfBytesToRead">The size of the lpBuffer buffer, in bytes. This function will read as many log entries as will fit in the buffer; 
		/// the function will not return partial entries.</param>
		/// <param name="pnBytesRead">A pointer to a variable that receives the number of bytes read by the function.</param>
		/// <param name="pnMinNumberOfBytesNeeded">A pointer to a variable that receives the required size of the lpBuffer buffer. This value is valid only 
		/// this function returns zero and GetLastError returns ERROR_INSUFFICIENT_BUFFER.</param>
		/// <returns>If the function succeeds, the return value is nonzero. 
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool ReadEventLogW(IntPtr hEventLog, EventLogReadFlags dwReadFlags, uint dwRecordOffset, IntPtr lpBuffer,
			uint nNumberOfBytesToRead, out uint pnBytesRead, out uint pnMinNumberOfBytesNeeded);

		/// <summary>
		/// Saves the specified event log to a backup file. The function does not clear the event log.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363635(v=vs.85).aspx"/>
		/// <remarks>
		/// The BackupEventLog function fails with the ERROR_PRIVILEGE_NOT_HELD error if the user does not have the SE_BACKUP_NAME privilege.
		/// </remarks>
		/// <param name="hEventLog">A handle to the open event log. The <see cref="OpenEventLogW"/> function returns this handle.</param>
		/// <param name="lpBackupFileName">The absolute or relative path of the backup file.</param>
		/// <returns>If the function succeeds, the return value is nonzero. 
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool BackupEventLogW(IntPtr hEventLog, string lpBackupFileName);

		/// <summary>
		/// Retrieves information about the specified event log.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363663(v=vs.85).aspx"/>
		/// <param name="hEventLog">A handle to the event log. The <see cref="OpenEventLogW"/> or <see cref="RegisterEventSourceW"/> function returns 
		/// this handle.</param>
		/// <param name="dwInfoLevel">The level of event log information to return. This parameter can be the following value.</param>
		/// <param name="lpBuffer">An application-allocated buffer that receives the event log information. The format of this data 
		/// depends on the value of the <paramref name="dwInfoLevel"/> parameter.</param>
		/// <param name="cbBufSize">The size of the <paramref name="lpBuffer"/> buffer, in bytes.</param>
		/// <param name="pcbBytesNeeded">The function sets this parameter to the required buffer size for the requested information, regardless of 
		/// whether the function succeeds. Use this value if the function fails with ERROR_INSUFFICIENT_BUFFER to allocate a buffer of the correct 
		/// size.</param>
		/// <returns>If the function succeeds, the return value is nonzero. 
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool GetEventLogInformation(IntPtr hEventLog, uint dwInfoLevel, out IntPtr lpBuffer, uint cbBufSize, out uint pcbBytesNeeded);

		/// <summary>
		/// Retrieves the number of records in the specified event log.
		/// </summary>
		/// <remarks>
		/// The oldest record in an event log is not necessarily record number 1. To determine the oldest record number in an event log, use the 
		/// <see cref="GetOldestEventLogRecord"/> function.
		/// </remarks>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363664(v=vs.85).aspx"/>
		/// <param name="hEventLog">A handle to the open event log. The <see cref="OpenEventLogW"/> or <see cref="OpenBackupEventLogW"/> function returns 
		/// this handle.</param>
		/// <param name="NumberOfRecords">A pointer to a variable that receives the number of records in the specified event log.</param>
		/// <returns>If the function succeeds, the return value is nonzero. 
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool GetNumberOfEventLogRecords(IntPtr hEventLog, out uint NumberOfRecords);

		/// <summary>
		/// Retrieves the absolute record number of the oldest record in the specified event log.
		/// </summary>
		/// <remarks>
		/// The oldest record in an event log is not necessarily record number 1. For more information, see Event Log Records.
		/// </remarks>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363665(v=vs.85).aspx"/>
		/// <param name="hEventLog">A handle to the open event log. The <see cref="OpenEventLogW"/> or <see cref="OpenBackupEventLogW"/> function returns 
		/// this handle.</param>
		/// <param name="OldestRecord">A pointer to a variable that receives the absolute record number of the oldest record in the specified event log.</param>
		/// <returns>If the function succeeds, the return value is nonzero.
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool GetOldestEventLogRecord(IntPtr hEventLog, out uint OldestRecord);

		/// <summary>
		/// Enables an application to receive notification when an event is written to the specified event log. When the event is written to the log, 
		/// the specified event object is set to the signaled state.
		/// </summary>
		/// <see href="https://msdn.microsoft.com/en-us/library/aa363670(v=vs.85).aspx"/>
		/// <remarks>
		/// The <see cref="NotifyChangeEventLog"/> function does not work with remote handles. If the <paramref name="hEventLog"/> parameter is the handle 
		/// to an event log on a remote computer, <see cref="NotifyChangeEventLog"/> returns zero, and GetLastError returns ERROR_INVALID_HANDLE.
		/// 
		/// If the thread is not waiting on the event when the system calls PulseEvent, the thread will not receive the notification.Therefore, you 
		/// should create a separate thread to wait for notifications.
		/// 
		/// The system will continue to notify you of changes until you close the handle to  the event log.To close the event log, use the 
		/// <see cref="CloseEventLog"/> or <see cref="DeregisterEventSource"/> function.
		/// </remarks>
		/// <param name="hEventLog">A handle to an event log. The <see cref="OpenEventLogW"/> function returns this handle.</param>
		/// <param name="hEvent">A handle to a manual-reset or auto-reset event object. Use the CreateEvent function to create the 
		/// event object.</param>
		/// <returns>If the function succeeds, the return value is nonzero.
		/// 
		/// If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool NotifyChangeEventLog(IntPtr hEventLog, IntPtr hEvent);

	}
}
