using System;

namespace Waher.Runtime.IO
{
	/// <summary>
	/// Class that generates a sequence of file names, based on the system time.
	/// </summary>
	public class FileNameTimeSequence
	{
		private readonly bool utc;
		private DateTime last = DateTime.MinValue;

		/// <summary>
		/// Class that generates a sequence of file names, based on the system time.
		/// </summary>
		/// <param name="Utc">If UTC time (true) or local time (false) should be used.</param>
		public FileNameTimeSequence(bool Utc)
		{
			this.utc = Utc;
		}
	}
}
