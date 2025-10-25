using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup.Databases
{
	/// <summary>
	/// Interface for Database settings
	/// </summary>
	[TypeName(TypeNameSerialization.FullName)]
	public class DatabaseSettings
	{
		private string objectId = null;

		/// <summary>
		/// Interface for Database settings
		/// </summary>
		public DatabaseSettings()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}
	}
}
