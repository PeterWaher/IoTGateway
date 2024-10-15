namespace Waher.Things.Ieee1451.Ieee1451_0.Messages
{
	/// <summary>
	/// Sampling Mode
	/// </summary>
	public enum SamplingMode
	{
		/// <summary>
		/// Trigger-initiated
		/// </summary>
		TriggerInit = 1,

		/// <summary>
		/// Free-running without pre-trigger 
		/// </summary> 
		FreeNoPre = 2,

		/// <summary>
		/// Free-running with pre-trigger
		/// </summary>
		FreePreTrig = 3,

		/// <summary>
		/// Continuous sampling
		/// </summary>
		Continuous = 4,

		/// <summary>
		/// Immediate operation
		/// </summary>
		Immediate = 5
	}
}
