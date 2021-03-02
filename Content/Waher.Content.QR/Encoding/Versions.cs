using System;

namespace Waher.Content.QR.Encoding
{
	/// <summary>
	/// Internal database of QR versions and properties.
	/// </summary>
	public static class Versions
	{
		/// <summary>
		/// Version information for Low Error Correction mode.
		/// </summary>
		public static readonly VersionInfo[] LowVersions = new VersionInfo[]
		{
			new VersionInfo(CorrectionLevel.L, 1, 7, 1, 19, 0, 0),
			new VersionInfo(CorrectionLevel.L, 2, 10, 1, 34, 0, 0),
			new VersionInfo(CorrectionLevel.L, 3, 15, 1, 55, 0, 0),
			new VersionInfo(CorrectionLevel.L, 4, 20, 1, 80, 0, 0),
			new VersionInfo(CorrectionLevel.L, 5, 26, 1, 108, 0, 0),
			new VersionInfo(CorrectionLevel.L, 6, 18, 2, 68, 0, 0),
			new VersionInfo(CorrectionLevel.L, 7, 20, 2, 78, 0, 0),
			new VersionInfo(CorrectionLevel.L, 8, 24, 2, 97, 0, 0),
			new VersionInfo(CorrectionLevel.L, 9, 30, 2, 116, 0, 0),
			new VersionInfo(CorrectionLevel.L, 10, 18, 2, 68, 2, 69),
			new VersionInfo(CorrectionLevel.L, 11, 20, 4, 81, 0, 0),
			new VersionInfo(CorrectionLevel.L, 12, 24, 2, 92, 2, 93),
			new VersionInfo(CorrectionLevel.L, 13, 26, 4, 107, 0, 0),
			new VersionInfo(CorrectionLevel.L, 14, 30, 3, 115, 1, 116),
			new VersionInfo(CorrectionLevel.L, 15, 22, 5, 87, 1, 88),
			new VersionInfo(CorrectionLevel.L, 16, 24, 5, 98, 1, 99),
			new VersionInfo(CorrectionLevel.L, 17, 28, 1, 107, 5, 108),
			new VersionInfo(CorrectionLevel.L, 18, 30, 5, 120, 1, 121),
			new VersionInfo(CorrectionLevel.L, 19, 28, 3, 113, 4, 114),
			new VersionInfo(CorrectionLevel.L, 20, 28, 3, 107, 5, 108),
			new VersionInfo(CorrectionLevel.L, 21, 28, 4, 116, 4, 117),
			new VersionInfo(CorrectionLevel.L, 22, 28, 2, 111, 7, 112),
			new VersionInfo(CorrectionLevel.L, 23, 30, 4, 121, 5, 122),
			new VersionInfo(CorrectionLevel.L, 24, 30, 6, 117, 4, 118),
			new VersionInfo(CorrectionLevel.L, 25, 26, 8, 106, 4, 107),
			new VersionInfo(CorrectionLevel.L, 26, 28, 10, 114, 2, 115),
			new VersionInfo(CorrectionLevel.L, 27, 30, 8, 122, 4, 123),
			new VersionInfo(CorrectionLevel.L, 28, 30, 3, 117, 10, 118),
			new VersionInfo(CorrectionLevel.L, 29, 30, 7, 116, 7, 117),
			new VersionInfo(CorrectionLevel.L, 30, 30, 5, 115, 10, 116),
			new VersionInfo(CorrectionLevel.L, 31, 30, 13, 115, 3, 116),
			new VersionInfo(CorrectionLevel.L, 32, 30, 17, 115, 0, 0),
			new VersionInfo(CorrectionLevel.L, 33, 30, 17, 115, 1, 116),
			new VersionInfo(CorrectionLevel.L, 34, 30, 13, 115, 6, 116),
			new VersionInfo(CorrectionLevel.L, 35, 30, 12, 121, 7, 122),
			new VersionInfo(CorrectionLevel.L, 36, 30, 6, 121, 14, 122),
			new VersionInfo(CorrectionLevel.L, 37, 30, 17, 122, 4, 123),
			new VersionInfo(CorrectionLevel.L, 38, 30, 4, 122, 18, 123),
			new VersionInfo(CorrectionLevel.L, 39, 30, 20, 117, 4, 118),
			new VersionInfo(CorrectionLevel.L, 40, 30, 19, 118, 6, 119)
		};

		/// <summary>
		/// Version information for Medium Error Correction mode.
		/// </summary>
		public static readonly VersionInfo[] MediumVersions = new VersionInfo[]
		{
			new VersionInfo(CorrectionLevel.M, 1, 10, 1, 16, 0, 0),
			new VersionInfo(CorrectionLevel.M, 2, 16, 1, 28, 0, 0),
			new VersionInfo(CorrectionLevel.M, 3, 26, 1, 44, 0, 0),
			new VersionInfo(CorrectionLevel.M, 4, 18, 2, 32, 0, 0),
			new VersionInfo(CorrectionLevel.M, 5, 24, 2, 43, 0, 0),
			new VersionInfo(CorrectionLevel.M, 6, 16, 4, 27, 0, 0),
			new VersionInfo(CorrectionLevel.M, 7, 18, 4, 31, 0, 0),
			new VersionInfo(CorrectionLevel.M, 8, 22, 2, 38, 2, 39),
			new VersionInfo(CorrectionLevel.M, 9, 22, 3, 36, 2, 37),
			new VersionInfo(CorrectionLevel.M, 10, 26, 4, 43, 1, 44),
			new VersionInfo(CorrectionLevel.M, 11, 30, 1, 50, 4, 51),
			new VersionInfo(CorrectionLevel.M, 12, 22, 6, 36, 2, 37),
			new VersionInfo(CorrectionLevel.M, 13, 22, 8, 37, 1, 38),
			new VersionInfo(CorrectionLevel.M, 14, 24, 4, 40, 5, 41),
			new VersionInfo(CorrectionLevel.M, 15, 24, 5, 41, 5, 42),
			new VersionInfo(CorrectionLevel.M, 16, 28, 7, 45, 3, 46),
			new VersionInfo(CorrectionLevel.M, 17, 28, 10, 46, 1, 47),
			new VersionInfo(CorrectionLevel.M, 18, 26, 9, 43, 4, 44),
			new VersionInfo(CorrectionLevel.M, 19, 26, 3, 44, 11, 45),
			new VersionInfo(CorrectionLevel.M, 20, 26, 3, 41, 13, 42),
			new VersionInfo(CorrectionLevel.M, 21, 26, 17, 42, 0, 0),
			new VersionInfo(CorrectionLevel.M, 22, 28, 17, 46, 0, 0),
			new VersionInfo(CorrectionLevel.M, 23, 28, 4, 47, 14, 48),
			new VersionInfo(CorrectionLevel.M, 24, 28, 6, 45, 14, 46),
			new VersionInfo(CorrectionLevel.M, 25, 28, 8, 47, 13, 48),
			new VersionInfo(CorrectionLevel.M, 26, 28, 19, 46, 4, 47),
			new VersionInfo(CorrectionLevel.M, 27, 28, 22, 45, 3, 46),
			new VersionInfo(CorrectionLevel.M, 28, 28, 3, 45, 23, 46),
			new VersionInfo(CorrectionLevel.M, 29, 28, 21, 45, 7, 46),
			new VersionInfo(CorrectionLevel.M, 30, 28, 19, 47, 10, 48),
			new VersionInfo(CorrectionLevel.M, 31, 28, 2, 46, 29, 47),
			new VersionInfo(CorrectionLevel.M, 32, 28, 10, 46, 23, 47),
			new VersionInfo(CorrectionLevel.M, 33, 28, 14, 46, 21, 47),
			new VersionInfo(CorrectionLevel.M, 34, 28, 14, 46, 23, 47),
			new VersionInfo(CorrectionLevel.M, 35, 28, 12, 47, 26, 48),
			new VersionInfo(CorrectionLevel.M, 36, 28, 6, 47, 34, 48),
			new VersionInfo(CorrectionLevel.M, 37, 28, 29, 46, 14, 47),
			new VersionInfo(CorrectionLevel.M, 38, 28, 13, 46, 32, 47),
			new VersionInfo(CorrectionLevel.M, 39, 28, 40, 47, 7, 48),
			new VersionInfo(CorrectionLevel.M, 40, 28, 18, 47, 31, 48)
		};

		/// <summary>
		/// Version information for Quartile Error Correction mode.
		/// </summary>
		public static readonly VersionInfo[] QuartileVersions = new VersionInfo[]
		{
			new VersionInfo(CorrectionLevel.Q, 1, 13, 1, 13, 0, 0),
			new VersionInfo(CorrectionLevel.Q, 2, 22, 1, 22, 0, 0),
			new VersionInfo(CorrectionLevel.Q, 3, 18, 2, 17, 0, 0),
			new VersionInfo(CorrectionLevel.Q, 4, 26, 2, 24, 0, 0),
			new VersionInfo(CorrectionLevel.Q, 5, 18, 2, 15, 2, 16),
			new VersionInfo(CorrectionLevel.Q, 6, 24, 4, 19, 0, 0),
			new VersionInfo(CorrectionLevel.Q, 7, 18, 2, 14, 4, 15),
			new VersionInfo(CorrectionLevel.Q, 8, 22, 4, 18, 2, 19),
			new VersionInfo(CorrectionLevel.Q, 9, 20, 4, 16, 4, 17),
			new VersionInfo(CorrectionLevel.Q, 10, 24, 6, 19, 2, 20),
			new VersionInfo(CorrectionLevel.Q, 11, 28, 4, 22, 4, 23),
			new VersionInfo(CorrectionLevel.Q, 12, 26, 4, 20, 6, 21),
			new VersionInfo(CorrectionLevel.Q, 13, 24, 8, 20, 4, 21),
			new VersionInfo(CorrectionLevel.Q, 14, 20, 11, 16, 5, 17),
			new VersionInfo(CorrectionLevel.Q, 15, 30, 5, 24, 7, 25),
			new VersionInfo(CorrectionLevel.Q, 16, 24, 15, 19, 2, 20),
			new VersionInfo(CorrectionLevel.Q, 17, 28, 1, 22, 15, 23),
			new VersionInfo(CorrectionLevel.Q, 18, 28, 17, 22, 1, 23),
			new VersionInfo(CorrectionLevel.Q, 19, 26, 17, 21, 4, 22),
			new VersionInfo(CorrectionLevel.Q, 20, 30, 15, 24, 5, 25),
			new VersionInfo(CorrectionLevel.Q, 21, 28, 17, 22, 6, 23),
			new VersionInfo(CorrectionLevel.Q, 22, 30, 7, 24, 16, 25),
			new VersionInfo(CorrectionLevel.Q, 23, 30, 11, 24, 14, 25),
			new VersionInfo(CorrectionLevel.Q, 24, 30, 11, 24, 16, 25),
			new VersionInfo(CorrectionLevel.Q, 25, 30, 7, 24, 22, 25),
			new VersionInfo(CorrectionLevel.Q, 26, 28, 28, 22, 6, 23),
			new VersionInfo(CorrectionLevel.Q, 27, 30, 8, 23, 26, 24),
			new VersionInfo(CorrectionLevel.Q, 28, 30, 4, 24, 31, 25),
			new VersionInfo(CorrectionLevel.Q, 29, 30, 1, 23, 37, 24),
			new VersionInfo(CorrectionLevel.Q, 30, 30, 15, 24, 25, 25),
			new VersionInfo(CorrectionLevel.Q, 31, 30, 42, 24, 1, 25),
			new VersionInfo(CorrectionLevel.Q, 32, 30, 10, 24, 35, 25),
			new VersionInfo(CorrectionLevel.Q, 33, 30, 29, 24, 19, 25),
			new VersionInfo(CorrectionLevel.Q, 34, 30, 44, 24, 7, 25),
			new VersionInfo(CorrectionLevel.Q, 35, 30, 39, 24, 14, 25),
			new VersionInfo(CorrectionLevel.Q, 36, 30, 46, 24, 10, 25),
			new VersionInfo(CorrectionLevel.Q, 37, 30, 49, 24, 10, 25),
			new VersionInfo(CorrectionLevel.Q, 38, 30, 48, 24, 14, 25),
			new VersionInfo(CorrectionLevel.Q, 39, 30, 43, 24, 22, 25),
			new VersionInfo(CorrectionLevel.Q, 40, 30, 34, 24, 34, 25)
		};

		/// <summary>
		/// Version information for High Error Correction mode.
		/// </summary>
		public static readonly VersionInfo[] HighVersions = new VersionInfo[]
		{
			new VersionInfo(CorrectionLevel.H, 1, 17, 1, 9, 0, 0),
			new VersionInfo(CorrectionLevel.H, 2, 28, 1, 16, 0, 0),
			new VersionInfo(CorrectionLevel.H, 3, 22, 2, 13, 0, 0),
			new VersionInfo(CorrectionLevel.H, 4, 16, 4, 9, 0, 0),
			new VersionInfo(CorrectionLevel.H, 5, 22, 2, 11, 2, 12),
			new VersionInfo(CorrectionLevel.H, 6, 28, 4, 15, 0, 0),
			new VersionInfo(CorrectionLevel.H, 7, 26, 4, 13, 1, 14),
			new VersionInfo(CorrectionLevel.H, 8, 26, 4, 14, 2, 15),
			new VersionInfo(CorrectionLevel.H, 9, 24, 4, 12, 4, 13),
			new VersionInfo(CorrectionLevel.H, 10, 28, 6, 15, 2, 16),
			new VersionInfo(CorrectionLevel.H, 11, 24, 3, 12, 8, 13),
			new VersionInfo(CorrectionLevel.H, 12, 28, 7, 14, 4, 15),
			new VersionInfo(CorrectionLevel.H, 13, 22, 12, 11, 4, 12),
			new VersionInfo(CorrectionLevel.H, 14, 24, 11, 12, 5, 13),
			new VersionInfo(CorrectionLevel.H, 15, 24, 11, 12, 7, 13),
			new VersionInfo(CorrectionLevel.H, 16, 30, 3, 15, 13, 16),
			new VersionInfo(CorrectionLevel.H, 17, 28, 2, 14, 17, 15),
			new VersionInfo(CorrectionLevel.H, 18, 28, 2, 14, 19, 15),
			new VersionInfo(CorrectionLevel.H, 19, 26, 9, 13, 16, 14),
			new VersionInfo(CorrectionLevel.H, 20, 28, 15, 15, 10, 16),
			new VersionInfo(CorrectionLevel.H, 21, 30, 19, 16, 6, 17),
			new VersionInfo(CorrectionLevel.H, 22, 24, 34, 13, 0, 0),
			new VersionInfo(CorrectionLevel.H, 23, 30, 16, 15, 14, 16),
			new VersionInfo(CorrectionLevel.H, 24, 30, 30, 16, 2, 17),
			new VersionInfo(CorrectionLevel.H, 25, 30, 22, 15, 13, 16),
			new VersionInfo(CorrectionLevel.H, 26, 30, 33, 16, 4, 17),
			new VersionInfo(CorrectionLevel.H, 27, 30, 12, 15, 28, 16),
			new VersionInfo(CorrectionLevel.H, 28, 30, 11, 15, 31, 16),
			new VersionInfo(CorrectionLevel.H, 29, 30, 19, 15, 26, 16),
			new VersionInfo(CorrectionLevel.H, 30, 30, 23, 15, 25, 16),
			new VersionInfo(CorrectionLevel.H, 31, 30, 23, 15, 28, 16),
			new VersionInfo(CorrectionLevel.H, 32, 30, 19, 15, 35, 16),
			new VersionInfo(CorrectionLevel.H, 33, 30, 11, 15, 46, 16),
			new VersionInfo(CorrectionLevel.H, 34, 30, 59, 16, 1, 17),
			new VersionInfo(CorrectionLevel.H, 35, 30, 22, 15, 41, 16),
			new VersionInfo(CorrectionLevel.H, 36, 30, 2, 15, 64, 16),
			new VersionInfo(CorrectionLevel.H, 37, 30, 24, 15, 46, 16),
			new VersionInfo(CorrectionLevel.H, 38, 30, 42, 15, 32, 16),
			new VersionInfo(CorrectionLevel.H, 39, 30, 10, 15, 67, 16),
			new VersionInfo(CorrectionLevel.H, 40, 30, 20, 15, 61, 16)
		};
	}
}
