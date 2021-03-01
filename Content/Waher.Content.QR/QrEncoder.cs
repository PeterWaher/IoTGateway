using System;
using Waher.Content.QR.Encoding;

namespace Waher.Content.QR
{
	/// <summary>
	/// QR Code encoder.
	/// </summary>
	public class QrEncoder : IDisposable
	{
		private readonly static ReedSolomonEC[] ecCalc = new ReedSolomonEC[31];

		/// <summary>
		/// QR Code encoder.
		/// </summary>
		public QrEncoder()
		{
		}

		/// <summary>
		/// QR Code encoder.
		/// </summary>
		public void Dispose()
		{
		}

		private int BitLengthCharacterCount(byte Version, EncodingMode Mode)
		{
			switch (Mode)
			{
				case EncodingMode.Numeric:
					if (Version < 10)
						return 10;
					else if (Version < 27)
						return 12;
					else if (Version < 41)
						return 14;
					break;

				case EncodingMode.Alphanumeric:
					if (Version < 10)
						return 9;
					else if (Version < 27)
						return 11;
					else if (Version < 41)
						return 13;
					break;

				case EncodingMode.Byte:
					if (Version < 10)
						return 8;
					else if (Version < 27)
						return 16;  // TODO: ??? or 12?
					else if (Version < 41)
						return 16;
					break;

				case EncodingMode.Kanji:
					if (Version < 10)
						return 8;
					else if (Version < 27)
						return 10;
					else if (Version < 41)
						return 12;
					break;
			}

			throw new NotSupportedException("Character Count bit length not supported.");   // TODO
		}

		/// <summary>
		/// Applies Error Correction to a byte message.
		/// </summary>
		/// <param name="Version">Version</param>
		/// <param name="Level">Error Correction level</param>
		/// <param name="Message">Byte message</param>
		/// <returns>Encoded message.</returns>
		public byte[] ApplyErrorCorrection(int Version, CorrectionLevel Level, byte[] Message)
		{
			if (Version <= 0 || Version > 40)
				throw new ArgumentOutOfRangeException("Invalid version.");

			VersionInfo Info;

			switch (Level)
			{
				case CorrectionLevel.L:
					Info = Versions.LowVersions[Version - 1];
					break;

				case CorrectionLevel.M:
					Info = Versions.MediumVersions[Version - 1];
					break;

				case CorrectionLevel.Q:
					Info = Versions.QuartileVersions[Version - 1];
					break;

				case CorrectionLevel.H:
					Info = Versions.HighVersions[Version - 1];
					break;

				default:
					throw new ArgumentException("Invalid Error Correction level.", nameof(Level));
			}

			return this.ApplyErrorCorrection(Info, Message);
		}

		/// <summary>
		/// Applies Error Correction to a byte message.
		/// </summary>
		/// <param name="Version">Version information</param>
		/// <param name="Message">Byte message</param>
		/// <returns>Encoded message.</returns>
		public byte[] ApplyErrorCorrection(VersionInfo Version, byte[] Message)
		{
			int SourceIndex = Message.Length;
			int MessageLen = Version.TotalDataBytes;

			if (SourceIndex > MessageLen)
				throw new ArgumentException("Message too long for selected version.", nameof(Message));
			else if (SourceIndex < MessageLen)
			{
				Array.Resize<byte>(ref Message, MessageLen);
				while (SourceIndex < MessageLen)
				{
					Message[SourceIndex++] = 236;
					if (SourceIndex < MessageLen)
						Message[SourceIndex++] = 17;
				}
			}

			int i, j, c, d = Version.TotalBlocks;
			byte[][] DataBlocks = new byte[d][];

			SourceIndex = 0;
			c = Version.DataBytesPerBlock1;
			for (i = 0; i < Version.BlocksPerGroup1; i++)
			{
				DataBlocks[i] = new byte[c];
				Array.Copy(Message, SourceIndex, DataBlocks[i], 0, c);
				SourceIndex += c;
			}

			if (Version.BlocksPerGroup2 > 0)
			{
				c = Version.DataBytesPerBlock2;
				for (j = 0; j < Version.BlocksPerGroup2; j++)
				{
					DataBlocks[i] = new byte[c];
					Array.Copy(Message, SourceIndex, DataBlocks[i++], 0, c);
					SourceIndex += c;
				}
			}

			ReedSolomonEC EC = ecCalc[Version.EcBytesPerBlock];
			if (EC is null)
			{
				EC = new ReedSolomonEC(Version.EcBytesPerBlock);
				ecCalc[Version.EcBytesPerBlock] = EC;
			}

			byte[][] ControlBlocks = new byte[d][];

			for (i = 0; i < d; i++)
				ControlBlocks[i] = EC.GenerateCorrectionCode(DataBlocks[i]);

			byte[] Result = new byte[Version.TotalBytes];
			int DestinationIndex = 0;

			for (i = 0, c = Version.DataBytesPerBlock1; i < c; i++)
			{
				for (j = 0; j < d; j++)
					Result[DestinationIndex++] = DataBlocks[j][i];
			}

			if (Version.DataBytesPerBlock2 > c)
			{
				while (i < Version.DataBytesPerBlock2)
				{
					for (j = Version.BlocksPerGroup1; j < d; j++)
						Result[DestinationIndex++] = DataBlocks[j][i];

					i++;
				}
			}

			for (i = 0, c = Version.EcBytesPerBlock; i < c; i++)
			{
				for (j = 0; j < d; j++)
					Result[DestinationIndex++] = ControlBlocks[j][i];
			}

			return Result;
		}


	}
}
