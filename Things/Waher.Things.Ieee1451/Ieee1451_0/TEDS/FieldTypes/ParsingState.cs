using Waher.Things.Ieee1451.Ieee1451_0.Messages;

namespace Waher.Things.Ieee1451.Ieee1451_0.TEDS.FieldTypes
{
	/// <summary>
	/// Contains parsing information.
	/// </summary>
	public class ParsingState
	{
		/// <summary>
		/// TEDS Class
		/// </summary>
		public byte Class;

		/// <summary>
		/// Current set of physical units.
		/// </summary>
		public Ieee1451_0PhysicalUnits Units;

		/// <summary>
		/// Sample Definition
		/// </summary>
		public Ieee1451_0SampleDefinition SampleDefinition;

		/// <summary>
		/// If names are in text format.
		/// </summary>
		public bool NameFormatText;
	}
}
