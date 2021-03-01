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
		private readonly static byte[][] alignmentMarkers = new byte[][]
		{
			new byte[] { 6 },
			new byte[] { 6, 18 },
			new byte[] { 6, 22 },
			new byte[] { 6, 26 },
			new byte[] { 6, 30 },
			new byte[] { 6, 34 },
			new byte[] { 6, 22, 38 },
			new byte[] { 6, 24, 42 },
			new byte[] { 6, 26, 46 },
			new byte[] { 6, 28, 50 },
			new byte[] { 6, 30, 54 },
			new byte[] { 6, 32, 58 },
			new byte[] { 6, 34, 62 },
			new byte[] { 6, 26, 46, 66 },
			new byte[] { 6, 26, 48, 70 },
			new byte[] { 6, 26, 50, 74 },
			new byte[] { 6, 30, 54, 78 },
			new byte[] { 6, 30, 56, 82 },
			new byte[] { 6, 30, 58, 86 },
			new byte[] { 6, 34, 62, 90 },
			new byte[] { 6, 28, 50, 72, 94 },
			new byte[] { 6, 26, 50, 74, 98 },
			new byte[] { 6, 30, 54, 78, 102 },
			new byte[] { 6, 28, 54, 80, 106 },
			new byte[] { 6, 32, 58, 84, 110 },
			new byte[] { 6, 30, 58, 86, 114 },
			new byte[] { 6, 34, 62, 90, 118 },
			new byte[] { 6, 26, 50, 74, 98 , 122 },
			new byte[] { 6, 30, 54, 78, 102, 126 },
			new byte[] { 6, 26, 52, 78, 104, 130 },
			new byte[] { 6, 30, 56, 82, 108, 134 },
			new byte[] { 6, 34, 60, 86, 112, 138 },
			new byte[] { 6, 30, 58, 86, 114, 142 },
			new byte[] { 6, 34, 62, 90, 118, 146 },
			new byte[] { 6, 30, 54, 78, 102, 126, 150 },
			new byte[] { 6, 24, 50, 76, 102, 128, 154 },
			new byte[] { 6, 28, 54, 80, 106, 132, 158 },
			new byte[] { 6, 32, 58, 84, 110, 136, 162 },
			new byte[] { 6, 26, 54, 82, 110, 138, 166 },
			new byte[] { 6, 30, 58, 86, 114, 142, 170 }
		};

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
			return this.ApplyErrorCorrection(this.FindVersionInfo(Version, Level), Message);
		}

		private VersionInfo FindVersionInfo(int Version, CorrectionLevel Level)
		{
			if (Version <= 0 || Version > 40)
				throw new ArgumentOutOfRangeException("Invalid version.");

			switch (Level)
			{
				case CorrectionLevel.L: return Versions.LowVersions[Version - 1];
				case CorrectionLevel.M: return Versions.MediumVersions[Version - 1];
				case CorrectionLevel.Q: return Versions.QuartileVersions[Version - 1];
				case CorrectionLevel.H: return Versions.HighVersions[Version - 1];
				default: throw new ArgumentException("Invalid Error Correction level.", nameof(Level));
			}
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

		/// <summary>
		/// Generates a QR Code matrix from a data message.
		/// </summary>
		/// <param name="Version">QR Code version to use.</param>
		/// <param name="Level">Error Correction Level</param>
		/// <param name="Message">Data message.</param>
		/// <returns>QR Code matrix.</returns>
		public QrMatrix GenerateMatrix(int Version, CorrectionLevel Level, byte[] Message)
		{
			return this.GenerateMatrix(this.FindVersionInfo(Version, Level), Message, true);
		}

		/// <summary>
		/// Generates a QR Code matrix from a data message.
		/// </summary>
		/// <param name="Version">QR Code version to use.</param>
		/// <param name="Level">Error Correction Level</param>
		/// <param name="Message">Data message.</param>
		/// <param name="ApplyErrorCorrection">If Error Correction should be applied to the message (true)
		/// or if the message include error correction codes already (false).</param>
		/// <returns>QR Code matrix.</returns>
		public QrMatrix GenerateMatrix(int Version, CorrectionLevel Level, byte[] Message, bool ApplyErrorCorrection)
		{
			return this.GenerateMatrix(this.FindVersionInfo(Version, Level), Message, ApplyErrorCorrection);
		}

		/// <summary>
		/// Generates a QR Code matrix from a data message.
		/// </summary>
		/// <param name="Version">QR Code version and error correction information.</param>
		/// <param name="Message">Data message.</param>
		/// <returns>QR Code matrix.</returns>
		public QrMatrix GenerateMatrix(VersionInfo Version, byte[] Message)
		{
			return this.GenerateMatrix(Version, Message, true);
		}

		/// <summary>
		/// Generates a QR Code matrix from a data message.
		/// </summary>
		/// <param name="Version">QR Code version and error correction information.</param>
		/// <param name="Message">Data message.</param>
		/// <param name="ApplyErrorCorrection">If Error Correction should be applied to the message (true)
		/// or if the message include error correction codes already (false).</param>
		/// <returns>QR Code matrix.</returns>
		public QrMatrix GenerateMatrix(VersionInfo Version, byte[] Message, bool ApplyErrorCorrection)
		{
			int Size = ((Version.Version - 1) << 2) + 21;
			QrMatrix M = new QrMatrix(Size);
			int i, j, c;

			// Finder patterns

			M.DrawFinderMarker(0, 0);
			M.DrawFinderMarker(Size - 7, 0);
			M.DrawFinderMarker(0, Size - 7);

			// Separators

			M.HLine(0, 7, 7, false, false);
			M.VLine(7, 0, 6, false, false);

			M.HLine(0, 7, Size - 8, false, false);
			M.VLine(7, Size - 7, Size - 1, false, false);

			M.HLine(Size - 8, Size - 1, 7, false, false);
			M.VLine(Size - 8, 0, 6, false, false);

			// Alignment patterns
			
			if (Version.Version > 0 && Version.Version <= alignmentMarkers.Length)
			{
				byte[] Markers = alignmentMarkers[Version.Version - 1];
				c = Markers.Length;
				
				for (i = 0; i < c; i++)
				{
					for (j = 0; j < c; j++)
					{
						if ((i == 0 && (j == 0 || j == c - 1)) || (j == 0 && i == c - 1))
							continue;
			
						M.DrawAlignmentMarker(Markers[i] - 2, Markers[j] - 2);
					}
				}
			}
			
			// Timing patterns
			
			M.HLine(8, Size - 9, 6, true, true);
			M.VLine(6, 8, Size - 9, true, true);
			
			// Dark module
			
			M.HLine(8, 8, Size - 8, true, false);
			
			// Reserve format information area
			
			M.HLine(0, 5, 8, false, false);
			M.HLine(7, 8, 8, false, false);
			M.VLine(8, 0, 5, false, false);
			M.VLine(8, 7, 7, false, false);
			
			M.HLine(Size - 8, Size - 1, 8, false, false);
			
			M.VLine(8, Size - 7, Size - 1, false, false);
			
			// Reserve version information area
			
			for (i = 0; i < 3; i++)
			{
				M.HLine(0, 5, Size - 9 - i, false, false);
				M.VLine(Size - 9 - i, 0, 5, false, false);
			}
			
			// Data bits
			
			byte[] FullMessage = ApplyErrorCorrection ? this.ApplyErrorCorrection(Version, Message) : Message;

			M.EncodeDataBits(FullMessage);	// TODO: Check result.
			//if (!M.EncodeDataBits(FullMessage))
			//	throw new NotSupportedException("Message too long for selected QR version & error correction level.");

			return M;
		}

	}
}
