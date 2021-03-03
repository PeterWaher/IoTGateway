using System;
using System.Collections.Generic;
using Waher.Content.QR.Encoding;
using Waher.Content.QR.Serialization;

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

		private int BitLengthCharacterCount(int Version, EncodingMode Mode)
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
			return this.ApplyErrorCorrection(Versions.FindVersionInfo(Version, Level), Message);
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
			return this.GenerateMatrix(Versions.FindVersionInfo(Version, Level), Message, true);
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
			return this.GenerateMatrix(Versions.FindVersionInfo(Version, Level), Message, ApplyErrorCorrection);
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
			return this.GenerateMatrix(Version, Message, ApplyErrorCorrection, false);
		}

		/// <summary>
		/// Generates a QR Code matrix from a data message.
		/// </summary>
		/// <param name="Version">QR Code version and error correction information.</param>
		/// <param name="Message">Data message.</param>
		/// <param name="ApplyErrorCorrection">If Error Correction should be applied to the message (true)
		/// or if the message include error correction codes already (false).</param>
		/// <param name="AssertMessageFit">Asserts message fits within the code.</param>
		/// <returns>QR Code matrix.</returns>
		public QrMatrix GenerateMatrix(VersionInfo Version, byte[] Message, bool ApplyErrorCorrection, bool AssertMessageFit)
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

			if (Version.Version >= 7)
			{
				for (i = 0; i < 3; i++)
				{
					M.HLine(0, 5, Size - 9 - i, false, false);
					M.VLine(Size - 9 - i, 0, 5, false, false);
				}
			}

			// Data bits

			M.SaveMask();

			byte[] FullMessage = ApplyErrorCorrection ? this.ApplyErrorCorrection(Version, Message) : Message;
			bool Fits = M.EncodeDataBits(FullMessage);

			if (AssertMessageFit && !Fits)
				throw new NotSupportedException("Message too long for selected QR version & error correction level.");

			// Data masking

			int Mask = -1;

			for (i = 0, c = int.MaxValue; i < 8; i++)
			{
				j = M.Penalty(masks[i]);
				if (j < c)
				{
					Mask = i;
					c = j;
				}
			}

			M.ApplyMask(masks[Mask]);

			// Format information

			uint u = (uint)Version.Level;
			u <<= 3;
			u |= (uint)Mask;
			u = typeInformationBits[u];

			u <<= 32 - 15;
			M.WriteBits(u, 0, 8, 1, 0, 6);
			M.WriteBits(u, 8, Size - 1, 0, -1, 7);
			u <<= 6;
			M.WriteBits(u, 6, 8, 1, 0, 1);
			u <<= 1;
			M.WriteBits(u, Size - 8, 8, 1, 0, 8);
			M.WriteBits(u, 8, 8, 0, -1, 2);
			u <<= 2;
			M.WriteBits(u, 8, 5, 0, -1, 5);

			// Version information

			if (Version.Version >= 7)
			{
				u = versionInformationBits[Version.Version - 7];
				u <<= 32 - 18;
				for (i = 0; i < 6; i++)
				{
					M.WriteBits(u, 5 - i, Size - 9, 0, -1, 3);
					M.WriteBits(u, Size - 9, 5 - i, -1, 0, 3);
					u <<= 3;
				}
			}

			return M;
		}

		private static readonly MaskFunction[] masks = new MaskFunction[]
		{
			QrMatrix.Mask0, QrMatrix.Mask1, QrMatrix.Mask2, QrMatrix.Mask3,
			QrMatrix.Mask4, QrMatrix.Mask5, QrMatrix.Mask6, QrMatrix.Mask7
		};

		private static readonly uint[] typeInformationBits = new uint[]
		{
			0b111011111000100,
			0b111001011110011,
			0b111110110101010,
			0b111100010011101,
			0b110011000101111,
			0b110001100011000,
			0b110110001000001,
			0b110100101110110,
			0b101010000010010,
			0b101000100100101,
			0b101111001111100,
			0b101101101001011,
			0b100010111111001,
			0b100000011001110,
			0b100111110010111,
			0b100101010100000,
			0b011010101011111,
			0b011000001101000,
			0b011111100110001,
			0b011101000000110,
			0b010010010110100,
			0b010000110000011,
			0b010111011011010,
			0b010101111101101,
			0b001011010001001,
			0b001001110111110,
			0b001110011100111,
			0b001100111010000,
			0b000011101100010,
			0b000001001010101,
			0b000110100001100,
			0b000100000111011
		};

		private static readonly uint[] versionInformationBits = new uint[]
		{
			0b000111110010010100,
			0b001000010110111100,
			0b001001101010011001,
			0b001010010011010011,
			0b001011101111110110,
			0b001100011101100010,
			0b001101100001000111,
			0b001110011000001101,
			0b001111100100101000,
			0b010000101101111000,
			0b010001010001011101,
			0b010010101000010111,
			0b010011010100110010,
			0b010100100110100110,
			0b010101011010000011,
			0b010110100011001001,
			0b010111011111101100,
			0b011000111011000100,
			0b011001000111100001,
			0b011010111110101011,
			0b011011000010001110,
			0b011100110000011010,
			0b011101001100111111,
			0b011110110101110101,
			0b011111001001010000,
			0b100000100111010101,
			0b100001011011110000,
			0b100010100010111010,
			0b100011011110011111,
			0b100100101100001011,
			0b100101010000101110,
			0b100110101001100100,
			0b100111010101000001,
			0b101000110001101001
		};

		/// <summary>
		/// Encodes text for purposes of presenting it in the smallest QR Code matrix possible.
		/// </summary>
		/// <param name="Level">Error Correction Level</param>
		/// <param name="Message">Data message.</param>
		/// <returns>Encoded message, with recommended QR Code version.</returns>
		public KeyValuePair<byte[], VersionInfo> Encode(CorrectionLevel Level, string Message)
		{
			BitWriter Output = new BitWriter();
			ITextEncoder Encoder;
			EncodingMode Mode;
			byte[] Bin = null;
			int ByteLen;
			int NrCharacters;

			if (NumericEncoder.CanEncode(Message))
			{
				Encoder = new NumericEncoder(Output);
				NrCharacters = Message.Length;
				ByteLen = NumericEncoder.GetByteLength(NrCharacters);
				Output.WriteBits(0b0001, 4);
				Mode = EncodingMode.Numeric;
			}
			else if (AlphanumericEncoder.CanEncode(Message))
			{
				Encoder = new AlphanumericEncoder(Output);
				NrCharacters = Message.Length;
				ByteLen = AlphanumericEncoder.GetByteLength(NrCharacters);
				Output.WriteBits(0b0010, 4);
				Mode = EncodingMode.Alphanumeric;
			}
			else
			{
				ByteEncoder ByteEncoder = new ByteEncoder(Output);
				Encoder = ByteEncoder;

				Bin = ByteEncoder.GetBytes(Message);
				NrCharacters = ByteLen = Bin.Length;

				Output.WriteBits(0b0100, 4);
				Mode = EncodingMode.Byte;
			}

			VersionInfo[] Options;
			VersionInfo Version = null;

			switch (Level)
			{
				case CorrectionLevel.L:
					Options = Versions.LowVersions;
					break;

				case CorrectionLevel.M:
					Options = Versions.MediumVersions;
					break;

				case CorrectionLevel.Q:
					Options = Versions.QuartileVersions;
					break;

				case CorrectionLevel.H:
				default:
					Options = Versions.HighVersions;
					break;
			}

			foreach (VersionInfo Option in Options)
			{
				if (Option.TotalDataBytes >= ByteLen + 2)
				{
					Version = Option;
					break;
				}
			}

			if (Version is null)
				throw new NotSupportedException("Message too large to fit in any of the recognized QR versions with the error correction level chosen.");

			Output.WriteBits((uint)NrCharacters, this.BitLengthCharacterCount(Version.Version, Mode));

			if (Bin is null)
				Encoder.Encode(Message);
			else
			{
				foreach (byte b in Bin)
					Output.WriteBits(b, 8);
			}

			ByteLen = Output.TotalBits;
			int i = (Version.TotalDataBytes << 3) - ByteLen;
			if (i > 0)
			{
				if (i > 4)
					i = 4;

				Output.WriteBits(0, i); // Terminator
			}

			i = Output.TotalBits & 7;
			if (i > 0)
				Output.WriteBits(0, 8 - i); // Byte padding

			i = Output.TotalBits >> 3;
			ByteLen = Version.TotalDataBytes;

			while (i++ < ByteLen)
			{
				Output.WriteBits(236, 8);
				if (++i < ByteLen)
					Output.WriteBits(17, 8);
			}

			return new KeyValuePair<byte[], VersionInfo>(Output.ToArray(), Version);
		}

		/// <summary>
		/// Generates the smallest QR Code matrix for a given data message.
		/// </summary>
		/// <param name="Level">Error Correction Level</param>
		/// <param name="Message">Data message.</param>
		/// <returns>QR Code matrix.</returns>
		public QrMatrix GenerateMatrix(CorrectionLevel Level, string Message)
		{
			KeyValuePair<byte[], VersionInfo> P = this.Encode(Level, Message);
			return this.GenerateMatrix(P.Value, P.Key, true, true);
		}

	}
}
