using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Dsn
{
	/// <summary>
	/// The Action field indicates the action performed by the Reporting-MTA
	/// as a result of its attempt to deliver the message to this recipient
	/// address.  
	/// </summary>
	public enum Action
	{
		/// <summary>
		/// indicates that the message could not be delivered to the
		/// recipient.  The Reporting MTA has abandoned any attempts
		/// to deliver the message to this recipient.  No further
		/// notifications should be expected.
		/// </summary>
		failed,

		/// <summary>
		/// indicates that the Reporting MTA has so far been unable
		/// to deliver or relay the message, but it will continue to
		/// attempt to do so.  Additional notification messages may
		/// be issued as the message is further delayed or
		/// successfully delivered, or if delivery attempts are later
		/// abandoned.
		/// </summary>
		delayed,

		/// <summary>
		/// indicates that the message was successfully delivered to
		/// the recipient address specified by the sender, which
		/// includes "delivery" to a mailing list exploder.  It does
		/// not indicate that the message has been read.  This is a
		/// terminal state and no further DSN for this recipient
		/// should be expected.
		/// </summary>
		delivered,

		/// <summary>
		/// indicates that the message has been relayed or gatewayed
		/// into an environment that does not accept responsibility
		/// for generating DSNs upon successful delivery.  This
		/// action-value SHOULD NOT be used unless the sender has
		/// requested notification of successful delivery for this
		/// recipient.
		/// </summary>
		relayed,

		/// <summary>
		/// indicates that the message has been successfully
		/// delivered to the recipient address as specified by the
		/// sender, and forwarded by the Reporting-MTA beyond that
		/// destination to multiple additional recipient addresses.
		/// An action-value of "expanded" differs from "delivered" in
		/// that "expanded" is not a terminal state.  Further
		/// "failed" and/or "delayed" notifications may be provided.
		/// </summary>
		expanded
	}

	/// <summary>
	/// Information fields for one recipient.
	/// </summary>
	public class PerRecipientFields : DsnFields
	{
		private string originalRecipient = null;
		private string originalRecipientType = null;
		private string finalRecipient = null;
		private string finalRecipientType = null;
		private Action? action = null;
		private int[] status = null;
		private string remoteMta = null;
		private string remoteMtaType = null;
		private string diagnosticCode = null;
		private string diagnosticCodeType = null;
		private string finalLogId = null;
		private DateTimeOffset? lastAttemptDate = null;
		private DateTimeOffset? willRetryUntil = null;

		/// <summary>
		/// Information fields for one recipient.
		/// </summary>
		/// <param name="Rows">Rows</param>
		public PerRecipientFields(string[] Rows)
			: base(Rows)
		{
		}

		/// <summary>
		/// Parses a field
		/// </summary>
		/// <param name="Key">Key</param>
		/// <param name="Value">Value</param>
		/// <returns>If the key was recognized.</returns>
		protected override bool Parse(string Key, string Value)
		{
			switch (Key.ToUpper())
			{
				case "ORIGINAL-RECIPIENT":
					if (this.ParseType(ref Value, out this.originalRecipientType))
					{
						this.originalRecipient = Value;
						return true;
					}
					else
						return false;

				case "FINAL-RECIPIENT":
					if (this.ParseType(ref Value, out this.finalRecipientType))
					{
						this.finalRecipient = Value;
						return true;
					}
					else
						return false;

				case "ACTION":
					if (Enum.TryParse<Action>(Value.ToLower(), out Action Action))
					{
						this.action = Action;
						return true;
					}
					else
						return false;

				case "STATUS":
					string[] Parts = Value.Split('.');
					if (Parts.Length == 3 &&
						int.TryParse(Parts[0], out int s1) &&
						int.TryParse(Parts[1], out int s2) &&
						int.TryParse(Parts[2], out int s3))
					{
						this.status = new int[] { s1, s2, s3 };
						return true;
					}
					else
						return false;

				case "REMOTE-MTA":
					if (this.ParseType(ref Value, out this.remoteMtaType))
					{
						this.remoteMta = Value;
						return true;
					}
					else
						return false;

				case "DIAGNOSTIC-CODE":
					if (this.ParseType(ref Value, out this.diagnosticCodeType))
					{
						this.diagnosticCode = Value;
						return true;
					}
					else
						return false;

				case "LAST-ATTEMPT-DATE":
					if (CommonTypes.TryParseRfc822(Value, out DateTimeOffset DTO))
					{
						this.lastAttemptDate = DTO;
						return true;
					}
					else
						return false;

				case "FINAL-LOG-ID":
					this.finalLogId = Value;
					return true;

				case "WILL-RETRY-UNTIL":
					if (CommonTypes.TryParseRfc822(Value, out DTO))
					{
						this.willRetryUntil = DTO;
						return true;
					}
					else
						return false;

				default:
					return false;
			}
		}

		/// <summary>
		/// Original recipient
		/// </summary>
		public string OriginalRecipient => this.originalRecipient;

		/// <summary>
		/// Type of original recipient
		/// </summary>
		public string OriginalRecipientType => this.originalRecipientType;

		/// <summary>
		/// Final recipient
		/// </summary>
		public string FinalRecipient => this.finalRecipient;

		/// <summary>
		/// Type of final recipient
		/// </summary>
		public string FinalRecipientType => this.finalRecipientType;

		/// <summary>
		/// Action
		/// </summary>
		public Action? Action => this.action;

		/// <summary>
		/// Status
		/// </summary>
		public int[] Status => this.status;

		/// <summary>
		/// Remote Message Transfer Agent
		/// </summary>
		public string RemoteMta => this.remoteMta;

		/// <summary>
		/// Type of Remote Message Transfer Agent
		/// </summary>
		public string RemoteMtaType => this.remoteMtaType;

		/// <summary>
		/// Diagnostic code
		/// </summary>
		public string DiagnosticCode => this.diagnosticCode;

		/// <summary>
		/// Type of Diagnostic code
		/// </summary>
		public string DiagnosticCodeType => this.diagnosticCodeType;

		/// <summary>
		/// Final Log ID
		/// </summary>
		public string FinalLogId => this.finalLogId;

		/// <summary>
		/// Timepoint of last attempt
		/// </summary>
		public DateTimeOffset? LastAttemptDate => this.lastAttemptDate;

		/// <summary>
		/// Until when retries will be made
		/// </summary>
		public DateTimeOffset? WillRetryUntil => this.willRetryUntil;
	}
}
