using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Dsn
{
	/// <summary>
	/// Information fields for the message.
	/// </summary>
	public class PerMessageFields : DsnFields
	{
		private string originalEnvelopeId = null;
		private string reportingMta = null;
		private string reportingMtaType = null;
		private string dsnGateway = null;
		private string dsnGatewayType = null;
		private string receivedFromMta = null;
		private string receivedFromMtaType = null;
		private DateTimeOffset? arrivalDate = null;

		/// <summary>
		/// Information fields for the message.
		/// </summary>
		/// <param name="Rows">Rows</param>
		public PerMessageFields(string[] Rows)
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
				case "ORIGINAL-ENVELOPE-ID":
					this.originalEnvelopeId = Value;
					return true;

				case "REPORTING-MTA":
					if (this.ParseType(ref Value, out this.reportingMtaType))
					{
						this.reportingMta = Value;
						return true;
					}
					else
						return false;

				case "DSN-GATEWAY":
					if (this.ParseType(ref Value, out this.dsnGatewayType))
					{
						this.dsnGateway = Value;
						return true;
					}
					else
						return false;

				case "RECEIVED-FROM-MTA":
					if (this.ParseType(ref Value, out this.receivedFromMtaType))
					{
						this.receivedFromMta = Value;
						return true;
					}
					else
						return false;

				case "ARRIVAL-DATE":
					if (CommonTypes.TryParseRfc822(Value, out DateTimeOffset DTO))
					{
						this.arrivalDate = DTO;
						return true;
					}
					else
						return false;

				default:
					return false;
			}
		}

		/// <summary>
		/// Original envelope identifier
		/// </summary>
		public string OriginalEnvelopeId => this.originalEnvelopeId;

		/// <summary>
		/// Reporting Message Transfer Agent
		/// </summary>
		public string ReportingMta => this.reportingMta;

		/// <summary>
		/// Type of Reporting Message Transfer Agent
		/// </summary>
		public string ReportingMtaType => this.reportingMtaType;

		/// <summary>
		/// Delivery Status Notification Gateway
		/// </summary>
		public string DsnGateway => this.dsnGateway;

		/// <summary>
		/// Type of Delivery Status Notification Gateway
		/// </summary>
		public string DsnGatewayType => this.dsnGatewayType;

		/// <summary>
		/// Received from Message Transfer Agent
		/// </summary>
		public string ReceivedFromMta => this.receivedFromMta;

		/// <summary>
		/// Type of Received from Message Transfer Agent
		/// </summary>
		public string ReceivedFromMtaType => this.receivedFromMtaType;

		/// <summary>
		/// Arrival Date
		/// </summary>
		public DateTimeOffset? ArrivalDate => this.arrivalDate;
	}
}
