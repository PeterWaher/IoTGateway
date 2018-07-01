using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Synchronization
{
	/// <summary>
	/// Class tha removes erroneous samples from a sequence of samples.
	/// </summary>
	public class SpikeRemoval
	{
		private readonly long?[] window;
		private readonly int windowSize;
		private readonly int spikePos;
		private readonly int spikeWidth;
		private long sum;
		private int nrInWindow;

		/// <summary>
		/// Class tha removes erroneous samples from a sequence of samples.
		/// </summary>
		/// <param name="WindowSize">Size of window to use to identify spikes.</param>
		/// <param name="SpikePosition">Position in window where spike removal is made.</param>
		/// <param name="SpikeWidth">Number of samples that can constitute a spike.</param>
		public SpikeRemoval(int WindowSize, int SpikePosition, int SpikeWidth)
		{
			this.windowSize = WindowSize;
			this.spikePos = SpikePosition;
			this.spikeWidth = SpikeWidth;
			this.window = new long?[WindowSize];
			this.nrInWindow = 0;
			this.sum = 0;
		}

		/// <summary>
		/// Adds a sample to the spike removal window.
		/// </summary>
		/// <param name="Sample">Sample</param>
		/// <param name="Removed">If a suspected spike value was removed at the spike position.</param>
		/// <param name="Sum">Sum of all samples that have passed the spike removal test.</param>
		/// <param name="NrSamples">Number of samples in sum.</param>
		/// <returns>If a sum of ok samples has been calculated.</returns>
		public bool Add(long Sample, out bool Removed, out long Sum, out int NrSamples)
		{
			if (this.window[0].HasValue)
			{
				this.sum -= this.window[0].Value;
				this.nrInWindow--;
			}

			Array.Copy(this.window, 1, this.window, 0, this.windowSize - 1);
			this.window[this.windowSize - 1] = Sample;
			this.sum += Sample;
			this.nrInWindow++;

			double Avg = ((double)this.sum) / this.nrInWindow;
			long? v;

			Removed = false;

			if (this.nrInWindow >= this.spikePos)
			{
				int NrLt = 0;
				int NrGt = 0;

				foreach (int? Value in this.window)
				{
					if (Value.HasValue)
					{
						if (Value.Value < Avg)
							NrLt++;
						else if (Value.Value > Avg)
							NrGt++;
					}
				}

				if (NrLt <= this.spikeWidth || NrGt <= this.spikeWidth)
				{
					v = this.window[this.windowSize - this.spikePos - 1];

					if (v.HasValue)
					{
						if ((NrLt <= this.spikeWidth && v.Value < Avg) || 
							(NrGt <= this.spikeWidth && v.Value > Avg))
						{
							Removed = true;

							this.sum -= v.Value;
							this.nrInWindow--;
							this.window[this.windowSize - this.spikePos - 1] = null;

							Avg = ((double)this.sum) / this.nrInWindow;
						}
					}
				}
			}

			int i;

			for (Sum = NrSamples = i = 0; i < this.spikePos; i++)
			{
				if ((v = this.window[i]).HasValue)
				{
					NrSamples++;
					Sum += v.Value;
				}
			}

			if (NrSamples > 0)
				return true;
			else
				return false;
		}

	}
}
