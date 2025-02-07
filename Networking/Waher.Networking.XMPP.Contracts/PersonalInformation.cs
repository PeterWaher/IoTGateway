using System;
using Waher.Content;
using Waher.Persistence;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contains personal information found in a legal identity.
	/// </summary>
	public class PersonalInformation
	{
		/// <summary>
		/// First name
		/// </summary>
		public CaseInsensitiveString FirstName = null;

		/// <summary>
		/// Middle names
		/// </summary>
		public CaseInsensitiveString MiddleNames = null;

		/// <summary>
		/// Last names
		/// </summary>
		public CaseInsensitiveString LastNames = null;

		/// <summary>
		/// Address
		/// </summary>
		public CaseInsensitiveString Address = null;

		/// <summary>
		/// Address, second row
		/// </summary>
		public CaseInsensitiveString Address2 = null;

		/// <summary>
		/// Postal Code
		/// </summary>
		public CaseInsensitiveString PostalCode = null;

		/// <summary>
		/// Area
		/// </summary>
		public CaseInsensitiveString Area = null;

		/// <summary>
		/// City
		/// </summary>
		public CaseInsensitiveString City = null;

		/// <summary>
		/// Region
		/// </summary>
		public CaseInsensitiveString Region = null;

		/// <summary>
		/// Country
		/// </summary>
		public CaseInsensitiveString Country = null;

		/// <summary>
		/// Nationality
		/// </summary>
		public CaseInsensitiveString Nationality = null;

		/// <summary>
		/// Personal Number
		/// </summary>
		public CaseInsensitiveString PersonalNumber = null;

		/// <summary>
		/// Organization Name
		/// </summary>
		public CaseInsensitiveString OrgName = null;

		/// <summary>
		/// Organization Department
		/// </summary>
		public CaseInsensitiveString OrgDepartment = null;

		/// <summary>
		/// Role in Organization
		/// </summary>
		public CaseInsensitiveString OrgRole = null;

		/// <summary>
		/// Organization Address
		/// </summary>
		public CaseInsensitiveString OrgAddress = null;

		/// <summary>
		/// Organization Address, second row
		/// </summary>
		public CaseInsensitiveString OrgAddress2 = null;

		/// <summary>
		/// Organization Postal Code
		/// </summary>
		public CaseInsensitiveString OrgPostalCode = null;

		/// <summary>
		/// Organization Area
		/// </summary>
		public CaseInsensitiveString OrgArea = null;

		/// <summary>
		/// Organization City
		/// </summary>
		public CaseInsensitiveString OrgCity = null;

		/// <summary>
		/// Organization Region
		/// </summary>
		public CaseInsensitiveString OrgRegion = null;

		/// <summary>
		/// Organization Country
		/// </summary>
		public CaseInsensitiveString OrgCountry = null;

		/// <summary>
		/// Organization Number
		/// </summary>
		public CaseInsensitiveString OrgNumber = null;

		/// <summary>
		/// e-mail address
		/// </summary>
		public string EMail = null;

		/// <summary>
		/// Phone Number
		/// </summary>
		public string Phone = null;

		/// <summary>
		/// JID
		/// </summary>
		public string Jid = null;

		/// <summary>
		/// Gender
		/// </summary>
		public Gender? Gender = null;

		/// <summary>
		/// Birth year
		/// </summary>
		public int? BirthYear = null;

		/// <summary>
		/// Birth month
		/// </summary>
		public int? BirthMonth = null;

		/// <summary>
		/// Birth day
		/// </summary>
		public int? BirthDay = null;

		/// <summary>
		/// If identity has organization information
		/// </summary>
		public bool HasOrg = false;

		/// <summary>
		/// If identity has birth date
		/// </summary>
		public bool HasBirthDate = false;

		/// <summary>
		/// Birth date
		/// </summary>
		public DateTime? BirthDate
		{
			get
			{
				if (this.HasBirthDate)
					return new DateTime(this.BirthYear.Value, this.BirthMonth.Value, this.BirthDay.Value);
				else
					return null;
			}
		}

		/// <summary>
		/// Age
		/// </summary>
		public int Age => Duration.GetDurationBetween(this.BirthDate.Value, DateTime.Now).Years;
	}
}
