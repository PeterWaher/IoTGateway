namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Keeps track of received message counters, for duplicate detection and replay protection
	/// (Matter Core Specification, 4.6.5).
	/// </summary>
	public class ReceptionState
	{
		/// <summary>
		/// Size of the message counter window.
		/// </summary>
		public const int WindowSize = 32;

		private readonly bool allowRollover;
		private uint maxCounter;
		private uint bitmap;
		private bool initialized;

		/// <summary>
		/// Keeps track of received message counters.
		/// </summary>
		/// <param name="AllowRollover">If counters are allowed to roll over (unencrypted messages), meaning
		/// counters behind the window are accepted as a new synchronization point.</param>
		public ReceptionState(bool AllowRollover)
		{
			this.allowRollover = AllowRollover;
		}

		/// <summary>
		/// Checks a received message counter, and registers it if new.
		/// </summary>
		/// <param name="Counter">Message counter.</param>
		/// <returns>true if the message is new, false if it is a duplicate (or replay).</returns>
		public bool CheckAndRegister(uint Counter)
		{
			lock (this)
			{
				if (!this.initialized)
				{
					this.initialized = true;
					this.maxCounter = Counter;
					this.bitmap = 0;
					return true;
				}

				if (Counter == this.maxCounter)
					return false;

				uint Diff = Counter - this.maxCounter;   // Modulo 2^32 arithmetic

				if (Diff < 0x80000000)
				{
					// Counter ahead of maximum.

					if (Diff > WindowSize)
						this.bitmap = 0;
					else if (Diff == WindowSize)
						this.bitmap = 1u << (WindowSize - 1);
					else
						this.bitmap = (this.bitmap << (int)Diff) | (1u << ((int)Diff - 1));

					this.maxCounter = Counter;
					return true;
				}
				else
				{
					uint Behind = this.maxCounter - Counter;

					if (Behind > WindowSize)
					{
						if (this.allowRollover)
						{
							this.maxCounter = Counter;
							this.bitmap = 0;
							return true;
						}
						else
							return false;
					}

					uint Mask = 1u << ((int)Behind - 1);

					if ((this.bitmap & Mask) != 0)
						return false;

					this.bitmap |= Mask;
					return true;
				}
			}
		}
	}
}
