using System;
using System.Collections.Generic;
using System.IO;
using Waher.Content.Images.Exif;

namespace Waher.Content.Images
{
	/// <summary>
	/// Extracts EXIF meta-data from images.
	/// 
	/// Specification:
	/// https://www.exif.org/Exif2-2.PDF
	/// </summary>
	public static class EXIF
	{
		/// <summary>
		/// Tries to parse EXIF data.
		/// </summary>
		/// <param name="ExifData">Binary EXIF data.</param>
		/// <param name="Tags">Parsed EXIF tags.</param>
		/// <returns>If EXIF data was successfully parsed.</returns>
		public static bool TryParse(byte[] ExifData, out ExifTag[] Tags)
		{
			ExifReader Reader = new ExifReader(ExifData);
			List<ExifTag> List = new List<ExifTag>();

			Tags = null;

			// EXIF identifier
			if (Reader.NextASCIIString() != "Exif")
				return false;

			// Padding
			if (Reader.NextByte() < 0)
				return false;

			// Byte order
			int RefPos = Reader.Position;

			if (Reader.NextByte() != 0x4d)
				return false;

			if (Reader.NextByte() != 0x4d)
				return false;

			if (Reader.NextSHORT() != 0x2a)
				return false;

			// IFD 0 offset
			uint? Offset = Reader.NextLONG();
			if (!Offset.HasValue)
				return false;

			int Pos = RefPos + (int)Offset.Value;
			if (Pos < Reader.Position || Pos >= Reader.Length)
				return false;

			Reader.Position = Pos;

			do
			{
				int NrRecords = Reader.NextSHORT();
				if (NrRecords < 0)
					return false;

				while (NrRecords > 0)
				{
					int TagID = Reader.NextSHORT();
					if (TagID < 0)
						return false;

					int Type = Reader.NextSHORT();
					if (Type < 0)
						return false;

					uint? Count = Reader.NextLONG();
					if (!Count.HasValue)
						return false;

					int ElementSize;
					int NrElements = (int)Count.Value;

					switch ((ExifTagType)Type)
					{
						case ExifTagType.ASCII:
							ElementSize = 1;
							NrElements = 1;
							break;

						case ExifTagType.BYTE:
						case ExifTagType.UNDEFINED:
						default:
							ElementSize = 1;
							break;

						case ExifTagType.SHORT:
							ElementSize = 2;
							break;

						case ExifTagType.LONG:
						case ExifTagType.SLONG:
							ElementSize = 4;
							break;

						case ExifTagType.RATIONAL:
						case ExifTagType.SRATIONAL:
							ElementSize = 8;
							break;
					}

					int RecSize = (int)(Count.Value * ElementSize);
					int PosBak;
					int i, j;

					if (RecSize <= 4)
						PosBak = Reader.Position + 4;
					else
					{
						uint? ValueOffset = Reader.NextLONG();
						if (!ValueOffset.HasValue || RefPos + ValueOffset.Value > Reader.Length)
							return false;

						PosBak = Reader.Position;
						Reader.Position = RefPos + (int)ValueOffset.Value;
					}

					switch ((ExifTagType)Type)
					{
						case ExifTagType.BYTE:
							if (NrElements == 1)
							{
								i = Reader.NextByte();
								if (i < 0)
									return false;

								List.Add(new ExifTypedTag<byte>((ExifTagName)TagID, (byte)i));
							}
							else
							{
								byte[] Items = new byte[NrElements];

								for (j = 0; j < NrElements; j++)
								{
									i = Reader.NextByte();
									if (i < 0)
										return false;

									Items[j] = (byte)i;
								}

								List.Add(new ExifTypedTag<byte[]>((ExifTagName)TagID, Items));
							}
							break;

						case ExifTagType.ASCII:
							string AsciiValue = Reader.NextASCIIString();
							List.Add(new ExifAsciiTag((ExifTagName)TagID, AsciiValue));
							break;

						case ExifTagType.SHORT:
							if (NrElements == 1)
							{
								i = Reader.NextSHORT();
								if (i < 0)
									return false;

								List.Add(new ExifTypedTag<ushort>((ExifTagName)TagID, (ushort)i));
							}
							else
							{
								ushort[] Items = new ushort[NrElements];

								for (j = 0; j < NrElements; j++)
								{
									i = Reader.NextSHORT();
									if (i < 0)
										return false;

									Items[j] = (ushort)i;
								}

								List.Add(new ExifTypedTag<ushort[]>((ExifTagName)TagID, Items));
							}
							break;

						case ExifTagType.LONG:
							uint? u;

							if (NrElements == 1)
							{
								u = Reader.NextLONG();
								if (!u.HasValue)
									return false;

								List.Add(new ExifTypedTag<uint>((ExifTagName)TagID, u.Value));
							}
							else
							{
								uint[] Items = new uint[NrElements];

								for (j = 0; j < NrElements; j++)
								{
									u = Reader.NextLONG();
									if (!u.HasValue)
										return false;

									Items[j] = u.Value;
								}

								List.Add(new ExifTypedTag<uint[]>((ExifTagName)TagID, Items));
							}
							break;

						case ExifTagType.RATIONAL:
							if (NrElements == 1)
							{
								u = Reader.NextLONG();
								if (!u.HasValue)
									return false;

								uint NumeratorValue = u.Value;

								u = Reader.NextLONG();
								if (!u.HasValue)
									return false;

								uint DenominatorValue = u.Value;
								List.Add(new ExifTypedTag<Rational>((ExifTagName)TagID, new Rational(NumeratorValue, DenominatorValue)));
							}
							else
							{
								Rational[] Items = new Rational[NrElements];

								for (j = 0; j < NrElements; j++)
								{
									u = Reader.NextLONG();
									if (!u.HasValue)
										return false;

									uint NumeratorValue = u.Value;

									u = Reader.NextLONG();
									if (!u.HasValue)
										return false;

									uint DenominatorValue = u.Value;

									Items[j] = new Rational(NumeratorValue, DenominatorValue);
								}

								List.Add(new ExifTypedTag<Rational[]>((ExifTagName)TagID, Items));
							}
							break;

						case ExifTagType.UNDEFINED:
							if (NrElements == 1)
							{
								i = Reader.NextByte();
								if (i < 0)
									return false;

								List.Add(new ExifTypedTag<byte>((ExifTagName)TagID, (byte)i));
							}
							else
							{
								byte[] Items = new byte[NrElements];

								for (j = 0; j < NrElements; j++)
								{
									i = Reader.NextByte();
									if (i < 0)
										return false;

									Items[j] = (byte)i;
								}

								List.Add(new ExifTypedTag<byte[]>((ExifTagName)TagID, Items));
							}
							break;

						case ExifTagType.SLONG:
							if (NrElements == 1)
							{
								u = Reader.NextLONG();
								if (!u.HasValue)
									return false;

								List.Add(new ExifTypedTag<int>((ExifTagName)TagID, (int)u.Value));
							}
							else
							{
								int[] Items = new int[NrElements];

								for (j = 0; j < NrElements; j++)
								{
									u = Reader.NextLONG();
									if (!u.HasValue)
										return false;

									Items[j] = (int)u.Value;
								}

								List.Add(new ExifTypedTag<int[]>((ExifTagName)TagID, Items));
							}
							break;

						case ExifTagType.SRATIONAL:
							if (NrElements == 1)
							{
								u = Reader.NextLONG();
								if (!u.HasValue)
									return false;

								int NumeratorValue = (int)u.Value;

								u = Reader.NextLONG();
								if (!u.HasValue)
									return false;

								int DenominatorValue = (int)u.Value;
								List.Add(new ExifTypedTag<SignedRational>((ExifTagName)TagID, new SignedRational(NumeratorValue, DenominatorValue)));
							}
							else
							{
								SignedRational[] Items = new SignedRational[NrElements];

								for (j = 0; j < NrElements; j++)
								{
									u = Reader.NextLONG();
									if (!u.HasValue)
										return false;

									int NumeratorValue = (int)u.Value;

									u = Reader.NextLONG();
									if (!u.HasValue)
										return false;

									int DenominatorValue = (int)u.Value;

									Items[j] = new SignedRational(NumeratorValue, DenominatorValue);
								}

								List.Add(new ExifTypedTag<SignedRational[]>((ExifTagName)TagID, Items));
							}
							break;
					}

					Reader.Position = PosBak;
					NrRecords--;
				}

				// Next IFD offset
				Offset = Reader.NextLONG();
				if (!Offset.HasValue)
					return false;

				if (Offset.Value == 0)
					break;

				Pos = RefPos + (int)Offset.Value;
				if (Pos < Reader.Position || Pos >= Reader.Length)
					return false;

				Reader.Position = Pos;
			}
			while (true);

			Tags = List.ToArray();
			return true;
		}

		/// <summary>
		/// Tries to extract EXIF meta-data from a JPEG image.
		/// </summary>
		/// <param name="FileName">Filename of JPEG image.</param>
		/// <param name="Tags">Parsed EXIF tags.</param>
		/// <returns>If EXIF information was found and extracted.</returns>
		public static bool TryExtractFromJPeg(string FileName, out ExifTag[] Tags)
		{
			using (FileStream fs = File.OpenRead(FileName))
			{
				return TryExtractFromJPeg(fs, out Tags);
			}
		}

		/// <summary>
		/// Tries to extract EXIF meta-data from a JPEG image.
		/// </summary>
		/// <param name="Image">JPEG image.</param>
		/// <param name="Tags">Parsed EXIF tags.</param>
		/// <returns>If EXIF information was found and extracted.</returns>
		public static bool TryExtractFromJPeg(byte[] Image, out ExifTag[] Tags)
		{
			using (MemoryStream ms = new MemoryStream(Image))
			{
				return TryExtractFromJPeg(ms, out Tags);
			}
		}

		/// <summary>
		/// Tries to extract EXIF meta-data from a JPEG stream.
		/// </summary>
		/// <param name="Image">Stream, pointing to the start of the JPEG image.</param>
		/// <param name="Tags">Parsed EXIF tags.</param>
		/// <returns>If EXIF information was found and extracted.</returns>
		public static bool TryExtractFromJPeg(Stream Image, out ExifTag[] Tags)
		{
			Tags = null;

			if (Image.ReadByte() != 0xff)
				return false;

			if (Image.ReadByte() != 0xd8)
				return false;

			byte[] Buf = null;

			do
			{
				if (!TryReadWord(Image, out ushort TagID))
					return false;

				if (!TryReadWord(Image, out ushort Len) || Len < 2)
					return false;

				Len -= 2;

				switch (TagID)
				{
					case 0xffe1:    //APP1
						Buf = new byte[Len];

						if (Image.Read(Buf, 0, Len) != Len)
							return false;

						return TryParse(Buf, out Tags);

					default:
						if (Image.CanSeek)
							Image.Seek(Len, SeekOrigin.Current);
						else
						{
							if (Buf is null || Buf.Length < Len)
								Buf = new byte[Len];

							if (Image.Read(Buf, 0, Len) != Len)
								return false;
						}
						break;
				}
			}
			while (true);
		}

		private static bool TryReadWord(Stream Input, out ushort w)
		{
			int i = Input.ReadByte();
			if (i < 0)
			{
				w = 0;
				return false;
			}

			w = (byte)i;

			i = Input.ReadByte();
			if (i < 0)
				return false;

			w <<= 8;
			w |= (byte)i;

			return true;
		}

	}
}
