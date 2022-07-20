using System;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute informs the database layer, that the corresponding collection should not be backed up.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class NoBackupAttribute : Attribute
	{
		/// <summary>
		/// This attribute informs the database layer, that the corresponding collection should not be backed up.
		/// </summary>
		/// <param name="Reason">Reason for not making backups</param>
		public NoBackupAttribute(string Reason)
		{
			this.Reason = Reason;
		}

		/// <summary>
		/// Reason for not making backups, if provided.
		/// </summary>
		public string Reason { get; }
	}
}
