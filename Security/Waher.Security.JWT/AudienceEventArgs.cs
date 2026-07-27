using System;

namespace Waher.Security.JWT
{
	/// <summary>
	/// Event arguments for events related to the audience of a JWT token.
	/// </summary>
	public class AudienceEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for events related to the audience of a JWT token.
		/// </summary>
		/// <param name="Audience">Audience of the JWT token.</param>
		public AudienceEventArgs(params string[] Audience)
			: base()
		{
			this.Audience = Audience;
		}

		/// <summary>
		/// Audience of the JWT token.
		/// </summary>
		public string[] Audience { get; }

		/// <summary>
		/// If the audience is acceptable.
		/// </summary>
		public bool Acceptable { get; set; } = false;
	}
}
