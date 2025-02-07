using System;
using System.IO;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to a text writer.
	/// </summary>
	public class TextWriterSniffer : TextSnifferBase, IDisposable
	{
		/// <summary>
		/// Text output writer.
		/// </summary>
		protected TextWriter output;

		private bool disposed = false;

		/// <summary>
		/// Outputs sniffed data to a text writer.
		/// </summary>
		/// <param name="Output">Output</param>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public TextWriterSniffer(TextWriter Output, BinaryPresentationMethod BinaryPresentationMethod)
			: base(BinaryPresentationMethod)
		{
			this.output = Output;
		}

		/// <inheritdoc/>
		protected override async Task WriteLine(string s)
		{
			if (this.disposed)
				return;

			await this.BeforeWrite();
			try
			{
				this.output?.WriteLine(s);
			}
			finally
			{
				await this.AfterWrite();
			}
		}

		/// <summary>
		/// If output can be disposed.
		/// </summary>
		public virtual bool CanDisposeOutput => true;

		/// <summary>
		/// Disposes of the current output.
		/// </summary>
		public virtual void DisposeOutput()
		{
			if (this.CanDisposeOutput)
			{
				this.output?.Flush();
				this.output?.Dispose();
			}

			this.output = null;
		}

		/// <inheritdoc/>
		public override Task DisposeAsync()
		{
			this.disposed = true;

			this.DisposeOutput();
			this.disposed = true;

			return base.DisposeAsync();
		}
	}
}
