using System.Diagnostics;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Outputs sniffed data to <see cref="Debug"/>.
	/// </summary>
	public class DebugSniffer : TextSnifferBase
	{
		/// <summary>
		/// Outputs sniffed data to <see cref="Debug"/>.
		/// </summary>
		/// <param name="BinaryPresentationMethod">How binary data is to be presented.</param>
		public DebugSniffer(BinaryPresentationMethod BinaryPresentationMethod)
			: base(BinaryPresentationMethod)
		{
		}

		/// <inheritdoc/>
		protected override Task WriteLine(string s)
		{
			Debug.WriteLine(s);
			return Task.CompletedTask;
		}
	}
}
