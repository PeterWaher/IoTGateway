using System;
using System.Threading.Tasks;

namespace Waher.Networking.Sniffers
{
	/// <summary>
	/// Method extensions for sniffers and sniffable objects.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Data">Binary Data.</param>
		public static async Task ReceiveBinary(this ISniffable Sniffable, byte[] Data)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.ReceiveBinary(Data);
			}
		}

		/// <summary>
		/// Called when binary data has been received.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public static async Task ReceiveBinary(this ISniffable Sniffable, DateTime Timestamp, byte[] Data)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.ReceiveBinary(Timestamp, Data);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Data">Binary Data.</param>
		public static async Task TransmitBinary(this ISniffable Sniffable, byte[] Data)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.TransmitBinary(Data);
			}
		}

		/// <summary>
		/// Called when binary data has been transmitted.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Data">Binary Data.</param>
		public static async Task TransmitBinary(this ISniffable Sniffable, DateTime Timestamp, byte[] Data)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.TransmitBinary(Timestamp, Data);
			}
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Text">Text</param>
		public static async Task ReceiveText(this ISniffable Sniffable, string Text)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.ReceiveText(Text);
			}
		}

		/// <summary>
		/// Called when text has been received.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public static async Task ReceiveText(this ISniffable Sniffable, DateTime Timestamp, string Text)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.ReceiveText(Timestamp, Text);
			}
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Text">Text</param>
		public static async Task TransmitText(this ISniffable Sniffable, string Text)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.TransmitText(Text);
			}
		}

		/// <summary>
		/// Called when text has been transmitted.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Text">Text</param>
		public static async Task TransmitText(this ISniffable Sniffable, DateTime Timestamp, string Text)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.TransmitText(Timestamp, Text);
			}
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Comment">Comment.</param>
		public static async Task Information(this ISniffable Sniffable, string Comment)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Information(Comment);
			}
		}

		/// <summary>
		/// Called to inform the viewer of something.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Comment">Comment.</param>
		public static async Task Information(this ISniffable Sniffable, DateTime Timestamp, string Comment)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Information(Timestamp, Comment);
			}
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Warning">Warning.</param>
		public static async Task Warning(this ISniffable Sniffable, string Warning)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Warning(Warning);
			}
		}

		/// <summary>
		/// Called to inform the viewer of a warning state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Warning">Warning.</param>
		public static async Task Warning(this ISniffable Sniffable, DateTime Timestamp, string Warning)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Warning(Timestamp, Warning);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Error">Error.</param>
		public static async Task Error(this ISniffable Sniffable, string Error)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Error(Error);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an error state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Error">Error.</param>
		public static async Task Error(this ISniffable Sniffable, DateTime Timestamp, string Error)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Error(Timestamp, Error);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Exception">Exception.</param>
		public static async Task Exception(this ISniffable Sniffable, string Exception)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Exception(Exception);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public static async Task Exception(this ISniffable Sniffable, DateTime Timestamp, string Exception)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Exception(Timestamp, Exception);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Exception">Exception.</param>
		public static async Task Exception(this ISniffable Sniffable, Exception Exception)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Exception(Exception);
			}
		}

		/// <summary>
		/// Called to inform the viewer of an exception state.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Timestamp">Timestamp of event.</param>
		/// <param name="Exception">Exception.</param>
		public static async Task Exception(this ISniffable Sniffable, DateTime Timestamp, Exception Exception)
		{
			if (Sniffable?.HasSniffers ?? false)
			{
				foreach (ISniffer Sniffer in Sniffable.Sniffers)
					await Sniffer.Exception(Timestamp, Exception);
			}
		}

	}
}
