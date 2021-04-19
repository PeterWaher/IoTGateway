using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Images.Exif
{
	/// <summary>
	/// Defined EXIF Tag names
	/// 
	/// Source:
	/// https://www.loc.gov/preservation/digital/formats/content/tiff_tags.shtml
	/// </summary>
	public enum ExifTagName
	{
		/// <summary>
		/// A general indication of the kind of data contained in this subfile.
		/// </summary>
		NewSubfileType = 0x00FE,

		/// <summary>
		/// A general indication of the kind of data contained in this subfile.
		/// </summary>
		SubfileType = 0x00FF,

		/// <summary>
		/// The number of columns in the image, i.e., the number of pixels per row.
		/// </summary>
		ImageWidth = 0x0100,

		/// <summary>
		/// The number of rows of pixels in the image.
		/// </summary>
		ImageLength = 0x0101,

		/// <summary>
		/// Number of bits per component.
		/// </summary>
		BitsPerSample = 0x0102,

		/// <summary>
		/// Compression scheme used on the image data.
		/// </summary>
		Compression = 0x0103,

		/// <summary>
		/// The color space of the image data.
		/// </summary>
		PhotometricInterpretation = 0x0106,

		/// <summary>
		/// For black and white TIFF files that represent shades of gray, the technique used to convert from gray to black and white pixels.
		/// </summary>
		Threshholding = 0x0107,

		/// <summary>
		/// The width of the dithering or halftoning matrix used to create a dithered or halftoned bilevel file.
		/// </summary>
		CellWidth = 0x0108,

		/// <summary>
		/// The length of the dithering or halftoning matrix used to create a dithered or halftoned bilevel file.
		/// </summary>
		CellLength = 0x0109,

		/// <summary>
		/// The logical order of bits within a byte.
		/// </summary>
		FillOrder = 0x010A,

		/// <summary>
		/// The name of the document from which this image was scanned.
		/// </summary>
		DocumentName = 0x010D,

		/// <summary>
		/// A string that describes the subject of the image.
		/// </summary>
		ImageDescription = 0x010E,

		/// <summary>
		/// The scanner manufacturer.
		/// </summary>
		Make = 0x010F,

		/// <summary>
		/// The scanner model name or number.
		/// </summary>
		Model = 0x0110,

		/// <summary>
		/// For each strip, the byte offset of that strip.
		/// </summary>
		StripOffsets = 0x0111,

		/// <summary>
		/// The orientation of the image with respect to the rows and columns.
		/// </summary>
		Orientation = 0x0112,

		/// <summary>
		/// The number of components per pixel.
		/// </summary>
		SamplesPerPixel = 0x0115,

		/// <summary>
		/// The number of rows per strip.
		/// </summary>
		RowsPerStrip = 0x0116,

		/// <summary>
		/// For each strip, the number of bytes in the strip after compression.
		/// </summary>
		StripByteCounts = 0x0117,

		/// <summary>
		/// The minimum component value used.
		/// </summary>
		MinSampleValue = 0x0118,

		/// <summary>
		/// The maximum component value used.
		/// </summary>
		MaxSampleValue = 0x0119,

		/// <summary>
		/// The number of pixels per ResolutionUnit in the ImageWidth direction.
		/// </summary>
		XResolution = 0x011A,

		/// <summary>
		/// The number of pixels per ResolutionUnit in the ImageLength direction.
		/// </summary>
		YResolution = 0x011B,

		/// <summary>
		/// How the components of each pixel are stored.
		/// </summary>
		PlanarConfiguration = 0x011C,

		/// <summary>
		/// The name of the page from which this image was scanned.
		/// </summary>
		PageName = 0x011D,

		/// <summary>
		/// X position of the image.
		/// </summary>
		XPosition = 0x011E,

		/// <summary>
		/// Y position of the image.
		/// </summary>
		YPosition = 0x011F,

		/// <summary>
		/// For each string of contiguous unused bytes in a TIFF file, the byte offset of the string.
		/// </summary>
		FreeOffsets = 0x0120,

		/// <summary>
		/// For each string of contiguous unused bytes in a TIFF file, the number of bytes in the string.
		/// </summary>
		FreeByteCounts = 0x0121,

		/// <summary>
		/// The precision of the information contained in the GrayResponseCurve.
		/// </summary>
		GrayResponseUnit = 0x0122,

		/// <summary>
		/// For grayscale data, the optical density of each possible pixel value.
		/// </summary>
		GrayResponseCurve = 0x0123,

		/// <summary>
		/// Options for Group 3 Fax compression
		/// </summary>
		T4Options = 0x0124,

		/// <summary>
		/// Options for Group 4 Fax compression
		/// </summary>
		T6Options = 0x0125,

		/// <summary>
		/// The unit of measurement for XResolution and YResolution.
		/// </summary>
		ResolutionUnit = 0x0128,

		/// <summary>
		/// The page number of the page from which this image was scanned.
		/// </summary>
		PageNumber = 0x0129,

		/// <summary>
		/// Describes a transfer function for the image in tabular style.
		/// </summary>
		TransferFunction = 0x012D,

		/// <summary>
		/// Name and version number of the software package(s) used to create the image.
		/// </summary>
		Software = 0x0131,

		/// <summary>
		/// Date and time of image creation.
		/// </summary>
		DateTime = 0x0132,

		/// <summary>
		/// Person who created the image.
		/// </summary>
		Artist = 0x013B,

		/// <summary>
		/// The computer and/or operating system in use at the time of image creation.
		/// </summary>
		HostComputer = 0x013C,

		///A mathematical operator that is applied to the image data before an encoding scheme is applied.
		Predictor = 0x013D,

		/// <summary>
		/// The chromaticity of the white point of the image.	
		/// </summary>
		WhitePoint = 0x013E,

		/// <summary>
		/// The chromaticities of the primaries of the image.
		/// </summary>
		PrimaryChromaticities = 0x013F,

		/// <summary>
		/// A color map for palette color images.
		/// </summary>
		ColorMap = 0x0140,

		/// <summary>
		/// Conveys to the halftone function the range of gray levels within a colorimetrically-specified image that should retain tonal detail.
		/// </summary>
		HalftoneHints = 0x0141,

		/// <summary>
		/// The tile width in pixels.This is the number of columns in each tile.
		/// </summary>
		TileWidth = 0x0142,

		/// <summary>
		/// The tile length (height) in pixels.This is the number of rows in each tile.
		/// </summary>
		TileLength = 0x0143,

		/// <summary>
		/// For each tile, the byte offset of that tile, as compressed and stored on disk.
		/// </summary>
		TileOffsets = 0x0144,

		/// <summary>
		/// For each tile, the number of (compressed) bytes in that tile.
		/// </summary>
		TileByteCounts = 0x0145,

		/// <summary>
		/// Used in the TIFF-F standard, denotes the number of 'bad' scan lines encountered by the facsimile device.
		/// </summary>
		BadFaxLines = 0x0146,

		/// <summary>
		/// Used in the TIFF-F standard, indicates if 'bad' lines encountered during reception are stored in the data, or if 'bad' lines have been replaced by the receiver.
		/// </summary>
		CleanFaxData = 0x0147,

		/// <summary>
		/// Used in the TIFF-F standard, denotes the maximum number of consecutive 'bad' scanlines received.
		/// </summary>
		ConsecutiveBadFaxLines = 0x0148,

		/// <summary>
		/// Offset to child IFDs.
		/// </summary>
		SubIFDs = 0x014A,

		/// <summary>
		/// The set of inks used in a separated (PhotometricInterpretation= 5) image.
		/// </summary>
		InkSet = 0x014C,

		/// <summary>
		/// The name of each ink used in a separated image.
		/// </summary>
		InkNames = 0x014D,

		/// <summary>
		/// The number of inks.
		/// </summary>
		NumberOfInks = 0x014E,

		/// <summary>
		/// The component values that correspond to a 0% dot and 100% dot.
		/// </summary>
		DotRange = 0x0150,

		/// <summary>
		/// A description of the printing environment for which this separation is intended.
		/// </summary>
		TargetPrinter = 0x0151,

		/// <summary>
		/// Description of extra components.
		/// </summary>
		ExtraSamples = 0x0152,

		/// <summary>
		/// Specifies how to interpret each data sample in a pixel.	
		/// </summary>
		SampleFormat = 0x0153,

		/// <summary>
		/// Specifies the minimum sample value.
		/// </summary>
		SMinSampleValue = 0x0154,

		/// <summary>
		/// Specifies the maximum sample value.	
		/// </summary>
		SMaxSampleValue = 0x0155,

		/// <summary>
		/// Expands the range of the TransferFunction.
		/// </summary>
		TransferRange = 0x0156,

		/// <summary>
		/// Mirrors the essentials of PostScript's path creation functionality.
		/// </summary>
		ClipPath = 0x0157,

		/// <summary>
		/// The number of units that span the width of the image, in terms of integer ClipPath coordinates.
		/// </summary>
		XClipPathUnits = 0x0158,

		/// <summary>
		/// The number of units that span the height of the image, in terms of integer ClipPath coordinates.
		/// </summary>
		YClipPathUnits = 0x0159,

		/// <summary>
		/// Aims to broaden the support for indexed images to include support for any color space.
		/// </summary>
		Indexed = 0x015A,

		/// <summary>
		/// JPEG quantization and/or Huffman tables.
		/// </summary>
		JPEGTables = 0x015B,

		/// <summary>
		/// OPI-related.
		/// </summary>
		OPIProxy = 0x015F,

		/// <summary>
		/// Used in the TIFF-FX standard to point to an IFD containing tags that are globally applicable to the complete TIFF file.	
		/// </summary>
		GlobalParametersIFD = 0x0190,

		/// <summary>
		/// Used in the TIFF-FX standard, denotes the type of data stored in this file or IFD.
		/// </summary>
		ProfileType = 0x0191,

		/// <summary>
		/// Used in the TIFF-FX standard, denotes the 'profile' that applies to this file.
		/// </summary>
		FaxProfile = 0x0192,

		/// <summary>
		/// Used in the TIFF-FX standard, indicates which coding methods are used in the file.
		/// </summary>
		CodingMethods = 0x0193,

		/// <summary>
		/// Used in the TIFF-FX standard, denotes the year of the standard specified by the FaxProfile field.
		/// </summary>
		VersionYear = 0x0194,

		/// <summary>
		/// Used in the TIFF-FX standard, denotes the mode of the standard specified by the FaxProfile field.
		/// </summary>
		ModeNumber = 0x0195,

		/// <summary>
		/// Used in the TIFF-F and TIFF-FX standards, holds information about the ITULAB (PhotometricInterpretation = 10) encoding.
		/// </summary>
		Decode = 0x01B1,

		/// <summary>
		/// Defined in the Mixed Raster Content part of RFC 2301, is the default color needed in areas where no image is available.
		/// </summary>
		DefaultImageColor = 0x01B2,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGProc = 0x0200,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGInterchangeFormat = 0x0201,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGInterchangeFormatLength = 0x0202,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGRestartInterval = 0x0203,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGLosslessPredictors = 0x0205,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGPointTransforms = 0x0206,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGQTables = 0x0207,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGDCTables = 0x0208,

		/// <summary>
		/// Old-style JPEG compression field. TechNote2 invalidates this part of the specification.
		/// </summary>
		JPEGACTables = 0x0209,

		/// <summary>
		/// The transformation from RGB to YCbCr image data.
		/// </summary>
		YCbCrCoefficients = 0x0211,

		/// <summary>
		/// Specifies the subsampling factors used for the chrominance components of a YCbCr image.
		/// </summary>
		YCbCrSubSampling = 0x0212,

		/// <summary>
		/// Specifies the positioning of subsampled chrominance components relative to luminance samples.
		/// </summary>
		YCbCrPositioning = 0x0213,

		/// <summary>
		/// Specifies a pair of headroom and footroom image data values (codes) for each pixel component.
		/// </summary>
		ReferenceBlackWhite = 0x0214,

		/// <summary>
		/// Defined in the Mixed Raster Content part of RFC 2301, used to replace RowsPerStrip for IFDs with variable-sized strips.
		/// </summary>
		StripRowCounts = 0x022F,

		/// <summary>
		/// XML packet containing XMP metadata
		/// </summary>
		XMP = 0x02BC,

		/// <summary>
		/// Ratings tag used by Windows Exif private IFD
		/// </summary>
		Image_Rating = 0x4746,

		/// <summary>
		/// Ratings tag used by Windows, value as percent Exif private IFD
		/// </summary>
		Image_RatingPercent = 0x4749,

		/// <summary>
		/// OPI-related.
		/// </summary>
		ImageID = 0x800D,

		/// <summary>
		/// Annotation data, as used in 'Imaging for Windows'.
		/// </summary>
		Wang_Annotation = 0x80A4,

		/// <summary>
		/// For camera raw files from sensors with CFA overlay.
		/// </summary>
		CFARepeatPatternDim = 0x828D,

		/// <summary>
		/// For camera raw files from sensors with CFA overlay.
		/// </summary>
		CFAPattern = 0x828E,

		/// <summary>
		/// Encodes camera battery level at time of image capture.
		/// </summary>
		BatteryLevel = 0x828F,

		/// <summary>
		/// Copyright notice.Baseline Also used by HD Photo.
		/// </summary>
		Copyright = 0x8298,

		/// <summary>
		/// Exposure time, given in seconds.
		/// </summary>
		ExposureTime = 0x829A,

		/// <summary>
		/// The F number.
		/// </summary>
		FNumber = 0x829D,

		/// <summary>
		/// Specifies the pixel data format encoding in the Molecular Dynamics GEL file format.
		/// </summary>
		MD_FileTag = 0x82A5,

		/// <summary>
		/// Specifies a scale factor in the Molecular Dynamics GEL file format.	
		/// </summary>
		MD_ScalePixel = 0x82A6,

		/// <summary>
		/// Used to specify the conversion from 16bit to 8bit in the Molecular Dynamics GEL file format.
		/// </summary>
		MD_ColorTable = 0x82A7,

		/// <summary>
		/// Name of the lab that scanned this file, as used in the Molecular Dynamics GEL file format.	
		/// </summary>
		MD_LabName = 0x82A8,

		/// <summary>
		/// Information about the sample, as used in the Molecular Dynamics GEL file format.	
		/// </summary>
		MD_SampleInfo = 0x82A9,

		/// <summary>
		/// Date the sample was prepared, as used in the Molecular Dynamics GEL file format.
		/// </summary>
		MD_PrepDate = 0x82AA,

		/// <summary>
		/// Time the sample was prepared, as used in the Molecular Dynamics GEL file format.
		/// </summary>
		MD_PrepTime = 0x82AB,

		/// <summary>
		/// Units for data in this file, as used in the Molecular Dynamics GEL file format.
		/// </summary>
		MD_FileUnits = 0x82AC,

		/// <summary>
		/// Used in interchangeable GeoTIFF_1_0 files.
		/// </summary>
		ModelPixelScaleTag = 0x830E,

		/// <summary>
		/// IPTC-NAA (International Press Telecommunications Council-Newspaper Association of America) metadata.
		/// </summary>
		IPTC_NAA = 0x83BB,

		/// <summary>
		/// 	Intergraph Application specific storage.
		/// </summary>
		INGR_Packet_Data_Tag = 0x847E,

		/// <summary>
		/// Intergraph Application specific flags.
		/// </summary>
		INGR_Flag_Registers = 0x847F,

		/// <summary>
		/// Originally part of Intergraph's GeoTIFF tags, but likely understood by IrasB only.
		/// </summary>
		IrasB_Transformation_Matrix = 0x8480,

		/// <summary>
		/// Originally part of Intergraph's GeoTIFF tags, but now used in interchangeable GeoTIFF_1_0 files.
		/// </summary>
		ModelTiepointTag = 0x8482,

		/// <summary>
		/// Site where image created.
		/// </summary>
		Site = 34016,

		/// <summary>
		/// Sequence of colors if other than CMYK.
		/// </summary>
		ColorSequence = 34017,

		/// <summary>
		/// Certain inherited headers.
		/// </summary>
		IT8Header = 34018,

		/// <summary>
		/// Type of raster padding used, if any.
		/// </summary>
		RasterPadding = 34019,

		/// <summary>
		/// Number of bits for short run length encoding.
		/// </summary>
		BitsPerRunLength = 34020,

		/// <summary>
		/// Number of bits for long run length encoding.
		/// </summary>
		BitsPerExtendedRunLength = 34021,

		/// <summary>
		/// Color value in a color pallette.
		/// </summary>
		ColorTable = 34022,

		/// <summary>
		/// Indicates if image (foreground) color or transparency is specified.
		/// </summary>
		ImageColorIndicator = 34023,

		/// <summary>
		/// Background color specification.
		/// </summary>
		BackgroundColorIndicator = 34024,

		/// <summary>
		/// Specifies image (foreground) color.
		/// </summary>
		ImageColorValue = 34025,

		/// <summary>
		/// Specifies background color.
		/// </summary>
		BackgroundColorValue = 34026,

		/// <summary>
		/// Specifies data values for 0 percent and 100 percent pixel intensity.
		/// </summary>
		PixelIntensityRange = 34027,

		/// <summary>
		/// Specifies if transparency is used in HC file.
		/// </summary>
		TransparencyIndicator = 34028,

		/// <summary>
		/// Specifies ASCII table or other reference per ISO 12641 and ISO 12642.
		/// </summary>
		ColorCharacterization = 34029,

		/// <summary>
		/// Indicates the type of information in an HC file.
		/// </summary>
		HCUsage = 34030,

		/// <summary>
		/// Indicates whether or not trapping has been applied to the file.
		/// </summary>
		TrapIndicator = 34031,

		/// <summary>
		/// Specifies CMYK equivalent for specific separations.
		/// </summary>
		CMYKEquivalent = 34032,

		/// <summary>
		/// Reserved for future use
		/// </summary>
		Reserved1 = 34033,

		/// <summary>
		/// Reserved for future use
		/// </summary>
		Reserved2 = 34034,

		/// <summary>
		/// Reserved for future use
		/// </summary>
		Reserved3 = 34035,

		/// <summary>
		/// Used in interchangeable GeoTIFF_1_0 files.
		/// </summary>
		ModelTransformationTag = 0x85D8,

		/// <summary>
		/// Collection of Photoshop 'Image Resource Blocks'.
		/// </summary>
		Photoshop = 0x8649,

		/// <summary>
		/// A pointer to the Exif IFD.	Private Also used by HD Photo.
		/// </summary>
		Exif_IFD = 0x8769,

		/// <summary>
		/// ICC profile data.	
		/// </summary>
		InterColorProfile = 0x8773,

		/// <summary>
		/// Defined in the Mixed Raster Content part of RFC 2301, used to denote the particular function of this Image in the mixed raster scheme.
		/// </summary>
		ImageLayer = 0x87AC,

		/// <summary>
		/// Used in interchangeable GeoTIFF_1_0 files.Private Mandatory in GeoTIFF_1_0
		/// </summary>
		GeoKeyDirectoryTag = 0x87AF,

		/// <summary>
		/// Used in interchangeable GeoTIFF_1_0 files.
		/// </summary>
		GeoDoubleParamsTag = 0x87B0,

		/// <summary>
		/// Used in interchangeable GeoTIFF_1_0 files.
		/// </summary>
		GeoAsciiParamsTag = 0x87B1,

		/// <summary>
		/// The class of the program used by the camera to set exposure when the picture is taken.
		/// </summary>
		ExposureProgram = 0x8822,

		/// <summary>
		/// Indicates the spectral sensitivity of each channel of the camera used.
		/// </summary>
		SpectralSensitivity = 0x8824,

		/// <summary>
		/// A pointer to the Exif-related GPS Info IFD.
		/// </summary>
		GPSInfo = 0x8825,

		/// <summary>
		/// Indicates the ISO Speed and ISO Latitude of the camera or input device as specified in ISO 12232.
		/// </summary>
		ISOSpeedRatings = 0x8827,

		/// <summary>
		/// Indicates the Opto-Electric Conversion Function(OECF) specified in ISO 14524.
		/// </summary>
		OECF = 0x8828,

		/// <summary>
		/// Indicates the field number of multifield images.
		/// </summary>
		Interlace = 0x8829,

		/// <summary>
		/// Encodes time zone of camera clock relative to GMT.
		/// </summary>
		TimeZoneOffset = 0x882A,

		/// <summary>
		/// Number of seconds image capture was delayed from button press.
		/// </summary>
		SelfTimeMode = 0x882B,

		/// <summary>
		/// The SensitivityType tag indicates PhotographicSensitivity tag, which one of the parameters of ISO 12232. Although it is an optional tag, it should be recorded when a PhotographicSensitivity tag is recorded.Value = 4, 5, 6, or 7 may be used in case that the values of plural parameters are the same.
		/// </summary>
		SensitivityType = 0x8830,

		/// <summary>
		/// This tag indicates the standard output sensitivity value of a camera or input device defined in ISO 12232. When recording this tag, the PhotographicSensitivity and SensitivityType tags shall also be recorded.
		/// </summary>
		StandardOutputSensitivity = 0x8831,

		/// <summary>
		/// This tag indicates the recommended exposure index value of a camera or input device defined in ISO 12232. When recording this tag, the PhotographicSensitivity and SensitivityType tags shall also be recorded.
		/// </summary>
		RecommendedExposureIndex = 0x8832,

		/// <summary>
		/// This tag indicates the ISO speed value of a camera or input device that is defined in ISO 12232. When recording this tag, the PhotographicSensitivity and SensitivityType tags shall also be recorded.
		/// </summary>
		ISOSpeed = 0x8833,

		/// <summary>
		/// This tag indicates the ISO speed latitude yyy value of a camera or input device that is defined in ISO 12232. However, this tag shall not be recorded without ISOSpeed and ISOSpeedLatitudezzz.
		/// </summary>
		ISOSpeedLatitudeyyy = 0x8834,

		/// <summary>
		/// This tag indicates the ISO speed latitude zzz value of a camera or input device that is defined in ISO 12232. However, this tag shall not be recorded without ISOSpeed and ISOSpeedLatitudeyyy.
		/// </summary>
		ISOSpeedLatitudezzz = 0x8835,

		/// <summary>
		/// Used by HylaFAX.
		/// </summary>
		HylaFAX_FaxRecvParams = 0x885C,

		/// <summary>
		/// Used by HylaFAX.
		/// </summary>
		HylaFAX_FaxSubAddress = 0x885D,

		/// <summary>
		/// Used by HylaFAX.
		/// </summary>
		HylaFAX_FaxRecvTime = 0x885E,

		/// <summary>
		/// The version of the supported Exif standard.
		/// </summary>
		ExifVersion = 0x9000,

		/// <summary>
		/// The date and time when the original image data was generated.
		/// </summary>
		DateTimeOriginal = 0x9003,

		/// <summary>
		/// The date and time when the image was stored as digital data.
		/// </summary>
		DateTimeDigitized = 0x9004,

		/// <summary>
		/// Specific to compressed data; specifies the channels and complements PhotometricInterpretation
		/// </summary>
		ComponentsConfiguration = 0x9101,

		/// <summary>
		/// Specific to compressed data; states the compressed bits per pixel.
		/// </summary>
		CompressedBitsPerPixel = 0x9102,

		/// <summary>
		/// Shutter speed.
		/// </summary>
		ShutterSpeedValue = 0x9201,

		/// <summary>
		/// The lens aperture.
		/// </summary>
		ApertureValue = 0x9202,

		/// <summary>
		/// The value of brightness.
		/// </summary>
		BrightnessValue = 0x9203,

		/// <summary>
		/// The exposure bias.
		/// </summary>
		ExposureBiasValue = 0x9204,

		/// <summary>
		/// The smallest F number of the lens.
		/// </summary>
		MaxApertureValue = 0x9205,

		/// <summary>
		/// The distance to the subject, given in meters
		/// </summary>
		SubjectDistance = 0x9206,

		/// <summary>
		/// The metering mode.
		/// </summary>
		MeteringMode = 0x9207,

		/// <summary>
		/// The kind of light source.
		/// </summary>
		LightSource = 0x9208,

		/// <summary>
		/// Indicates the status of flash when the image was shot.
		/// </summary>
		Flash = 0x9209,

		/// <summary>
		/// The actual focal length of the lens, in mm.
		/// </summary>
		FocalLength = 0x920A,

		/// <summary>
		/// Amount of flash energy (BCPS).
		/// </summary>
		FlashEnergy = 0x920B,

		/// <summary>
		/// SFR of the camera.	
		/// </summary>
		SpatialFrequencyResponse = 0x920C,

		/// <summary>
		/// Noise measurement values.
		/// </summary>
		Noise = 0x920D,

		/// <summary>
		/// Number of pixels per FocalPlaneResolutionUnit (37392) in ImageWidth direction for main image.
		/// </summary>
		FocalPlaneXResolution = 0x920E,

		/// <summary>
		/// Number of pixels per FocalPlaneResolutionUnit (37392) in ImageLength direction for main image.
		/// </summary>
		FocalPlaneYResolution = 0x920F,

		/// <summary>
		/// Unit of measurement for FocalPlaneXResolution(37390) and FocalPlaneYResolution(37391).	
		/// </summary>
		FocalPlaneResolutionUnit = 0x9210,

		/// <summary>
		/// Number assigned to an image, e.g., in a chained image burst.
		/// </summary>
		ImageNumber = 0x9211,

		/// <summary>
		/// Security classification assigned to the image.
		/// </summary>
		SecurityClassification = 0x9212,

		/// <summary>
		/// Record of what has been done to the image.	
		/// </summary>
		ImageHistory = 0x9213,

		/// <summary>
		/// Indicates the location and area of the main subject in the overall scene.
		/// </summary>
		SubjectLocation = 0x9214,

		/// <summary>
		/// Encodes the camera exposure index setting when image was captured.
		/// </summary>
		ExposureIndex = 0x9215,

		/// <summary>
		/// For current spec, tag value equals 1 0 0 0.
		/// </summary>
		TIFF_EPStandardID = 0x9216,

		/// <summary>
		/// Type of image sensor.
		/// </summary>
		SensingMethod = 0x9217,

		/// <summary>
		/// Manufacturer specific information.
		/// </summary>
		MakerNote = 0x927C,

		/// <summary>
		/// Keywords or comments on the image; complements ImageDescription.
		/// </summary>
		UserComment = 0x9286,

		/// <summary>
		/// A tag used to record fractions of seconds for the DateTime tag.
		/// </summary>
		SubsecTime = 0x9290,

		/// <summary>
		/// A tag used to record fractions of seconds for the DateTimeOriginal tag.
		/// </summary>
		SubsecTimeOriginal = 0x9291,

		/// <summary>
		/// A tag used to record fractions of seconds for the DateTimeDigitized tag.
		/// </summary>
		SubsecTimeDigitized = 0x9292,

		/// <summary>
		/// Used by Adobe Photoshop.
		/// </summary>
		ImageSourceData = 0x935C,

		/// <summary>
		/// Title tag used by Windows, encoded in UCS2 
		/// </summary>
		XPTitle = 0x9c9b,

		/// <summary>
		/// Comment tag used by Windows, encoded in UCS2 
		/// </summary>
		XPComment = 0x9c9c,

		/// <summary>
		/// Author tag used by Windows, encoded in UCS2 
		/// </summary>
		XPAuthor = 0x9c9d,

		/// <summary>
		/// Keywords tag used by Windows, encoded in UCS2 
		/// </summary>
		XPKeywords = 0x9c9e,

		/// <summary>
		/// Subject tag used by Windows, encoded in UCS2 
		/// </summary>
		XPSubject = 0x9c9f,

		/// <summary>
		/// The Flashpix format version supported by a FPXR file.
		/// </summary>
		FlashpixVersion = 0xA000,

		/// <summary>
		/// The color space information tag is always recorded as the color space specifier.
		/// </summary>
		ColorSpace = 0xA001,

		/// <summary>
		/// Specific to compressed data; the valid width of the meaningful image.
		/// </summary>
		PixelXDimension = 0xA002,

		/// <summary>
		/// Specific to compressed data; the valid height of the meaningful image.
		/// </summary>
		PixelYDimension = 0xA003,

		/// <summary>
		/// Used to record the name of an audio file related to the image data.
		/// </summary>
		RelatedSoundFile = 0xA004,

		/// <summary>
		/// IFD A pointer to the Exif-related Interoperability IFD.
		/// </summary>
		Interoperability = 0xA005,

		/// <summary>
		/// Indicates the strobe energy at the time the image is captured, as measured in Beam Candle Power Seconds
		/// </summary>
		FlashEnergy2 = 0xA20B,

		/// <summary>
		/// Records the camera or input device spatial frequency table and SFR values in the direction of image width, image height, and diagonal direction, as specified in ISO 12233.
		/// </summary>
		SpatialFrequencyResponse2 = 0xA20C,

		/// <summary>
		/// Indicates the number of pixels in the image width (X) direction per FocalPlaneResolutionUnit on the camera focal plane.
		/// </summary>
		FocalPlaneXResolution2 = 0xA20E,

		/// <summary>
		/// Indicates the number of pixels in the image height (Y) direction per FocalPlaneResolutionUnit on the camera focal plane.
		/// </summary>
		FocalPlaneYResolution2 = 0xA20F,

		/// <summary>
		/// Indicates the unit for measuring FocalPlaneXResolution and FocalPlaneYResolution.	
		/// </summary>
		FocalPlaneResolutionUnit2 = 0xA210,

		/// <summary>
		/// Indicates the location of the main subject in the scene.	
		/// </summary>
		SubjectLocation2 = 0xA214,

		/// <summary>
		/// Indicates the exposure index selected on the camera or input device at the time the image is captured.
		/// </summary>
		ExposureIndex2 = 0xA215,

		/// <summary>
		/// Indicates the image sensor type on the camera or input device.
		/// </summary>
		SensingMethod2 = 0xA217,

		/// <summary>
		/// Indicates the image source.
		/// </summary>
		FileSource = 0xA300,

		/// <summary>
		/// Indicates the type of scene.
		/// </summary>
		SceneType = 0xA301,

		/// <summary>
		/// Indicates the color filter array (CFA) geometric pattern of the image sensor when a one-chip color area sensor is used.
		/// </summary>
		CFAPattern2 = 0xA302,

		/// <summary>
		/// Indicates the use of special processing on image data, such as rendering geared to output.	
		/// </summary>
		CustomRendered = 0xA401,

		/// <summary>
		/// Indicates the exposure mode set when the image was shot.	
		/// </summary>
		ExposureMode = 0xA402,

		/// <summary>
		/// Indicates the white balance mode set when the image was shot.
		/// </summary>
		WhiteBalance = 0xA403,

		/// <summary>
		/// Indicates the digital zoom ratio when the image was shot.	
		/// </summary>
		DigitalZoomRatio = 0xA404,

		/// <summary>
		/// Indicates the equivalent focal length assuming a 35mm film camera, in mm.
		/// </summary>
		FocalLengthIn35mmFilm = 0xA405,

		/// <summary>
		/// Indicates the type of scene that was shot.	
		/// </summary>
		SceneCaptureType = 0xA406,

		/// <summary>
		/// Indicates the degree of overall image gain adjustment.	
		/// </summary>
		GainControl = 0xA407,

		/// <summary>
		/// Indicates the direction of contrast processing applied by the camera when the image was shot.
		/// </summary>
		Contrast = 0xA408,

		/// <summary>
		/// Indicates the direction of saturation processing applied by the camera when the image was shot.
		/// </summary>
		Saturation = 0xA409,

		/// <summary>
		/// Indicates the direction of sharpness processing applied by the camera when the image was shot.
		/// </summary>
		Sharpness = 0xA40A,

		/// <summary>
		/// This tag indicates information on the picture-taking conditions of a particular camera model.
		/// </summary>
		DeviceSettingDescription = 0xA40B,

		/// <summary>
		/// Indicates the distance to the subject.	
		/// </summary>
		SubjectDistanceRange = 0xA40C,

		/// <summary>
		/// Indicates an identifier assigned uniquely to each image.	 
		/// </summary>
		ImageUniqueID = 0xA420,

		/// <summary>
		/// Camera owner name as ASCII string.	
		/// </summary>
		CameraOwnerName = 0xa430,

		/// <summary>
		/// Camera body serial number as ASCII string.	
		/// </summary>
		BodySerialNumber = 0xa431,

		/// <summary>
		/// This tag notes minimum focal length, maximum focal length, minimum F number in the minimum focal length, and minimum F number in the maximum focal length, which are specification information for the lens that was used in photography.When the minimum F number is unknown, the notation is 0/0.	
		/// </summary>
		LensSpecification = 0xa432,

		/// <summary>
		/// Lens manufacturer name as ASCII string.	
		/// </summary>
		LensMake = 0xa433,

		/// <summary>
		/// Lens model name and number as ASCII string.	
		/// </summary>
		LensModel = 0xa434,

		/// <summary>
		/// Lens serial number as ASCII string.	
		/// </summary>
		LensSerialNumber = 0xa434,

		/// <summary>
		/// Used by the GDAL library, holds an XML list of name = value 'metadata' values about the image as a whole, and about specific samples.
		/// </summary>
		GDAL_METADATA = 0xA480,

		/// <summary>
		/// Used by the GDAL library, contains an ASCII encoded nodata or background pixel value.
		/// </summary>
		GDAL_NODATA = 0xA481,

		/// <summary>
		/// A 128-bit Globally Unique Identifier (GUID) that identifies the image pixel format.
		/// </summary>
		PixelFormat = 0xBC01,

		/// <summary>
		/// Specifies the transformation to be applied when decoding the image to present the desired representation.
		/// </summary>
		Transformation = 0xBC02,

		/// <summary>
		/// Specifies that image data is uncompressed.
		/// </summary>
		Uncompressed = 0xBC03,

		/// <summary>
		/// Specifies the image type of each individual frame in a multi-frame file.
		/// </summary>
		ImageType = 0xBC04,

		/// <summary>
		/// Specifies the number of columns in the transformed photo, or the number of pixels per scan line.
		/// </summary>
		ImageWidth2 = 0xBC80,

		/// <summary>
		/// Specifies the number of pixels or scan lines in the transformed photo.
		/// </summary>
		ImageHeight = 0xBC81,

		/// <summary>
		/// Specifies the horizontal resolution of a transformed image expressed in pixels per inch.
		/// </summary>
		WidthResolution = 0xBC82,

		/// <summary>
		/// Specifies the vertical resolution of a transformed image expressed in pixels per inch.
		/// </summary>
		HeightResolution = 0xBC83,

		/// <summary>
		/// Specifies the byte offset pointer to the beginning of the photo data, relative to the beginning of the file.
		/// </summary>
		ImageOffset = 0xBCC0,

		/// <summary>
		/// Specifies the size of the photo in bytes.
		/// </summary>
		ImageByteCount = 0xBCC1,

		/// <summary>
		/// Specifies the byte offset pointer the beginning of the planar alpha channel data, relative to the beginning of the file.
		/// </summary>
		AlphaOffset = 0xBCC2,

		/// <summary>
		/// Specifies the size of the alpha channel data in bytes.
		/// </summary>
		AlphaByteCount = 0xBCC3,

		/// <summary>
		/// Signifies the level of data that has been discarded from the image as a result of a compressed domain transcode to reduce the file size.
		/// </summary>
		ImageDataDiscard = 0xBCC4,

		/// <summary>
		/// Signifies the level of data that has been discarded from the planar alpha channel as a result of a compressed domain transcode to reduce the file size.
		/// </summary>
		AlphaDataDiscard = 0xBCC5,

		/// <summary>
		/// Used in the Oce scanning process.
		/// </summary>
		Oce_Scanjob_Description = 0xC427,

		/// <summary>
		/// Used in the Oce scanning process.
		/// </summary>
		Oce_Application_Selector = 0xC428,

		/// <summary>
		/// Used in the Oce scanning process.
		/// </summary>
		Oce_Identification_Number = 0xC429,

		/// <summary>
		/// Used in the Oce scanning process.
		/// </summary>
		Oce_ImageLogic_Characteristics = 0xC42A,

		/// <summary>
		/// Description needed.	
		/// </summary>
		PrintImageMatching = 0xc4a5,

		/// <summary>
		/// Encodes DNG four-tier version number; for version 1.1.0.0, the tag contains the bytes 1, 1, 0, 0. Used in IFD 0 of DNG files.
		/// </summary>
		DNGVersion = 0xC612,

		/// <summary>
		/// Defines oldest version of spec with which file is compatible.Used in IFD 0 of DNG files.
		/// </summary>
		DNGBackwardVersion = 0xC613,

		/// <summary>
		/// Unique, non-localized nbame for camera model.Used in IFD 0 of DNG files.
		/// </summary>
		UniqueCameraModel = 0xC614,

		/// <summary>
		/// Similar to 50708, with localized camera name.Used in IFD 0 of DNG files.
		/// </summary>
		LocalizedCameraModel = 0xC615,

		/// <summary>
		/// Mapping between values in the CFAPattern tag and the plane numbers in LinearRaw space.Used in Raw IFD of DNG files.
		/// </summary>
		CFAPlaneColor = 0xC616,

		/// <summary>
		/// Spatial layout of the CFA.Used in Raw IFD of DNG files.
		/// </summary>
		CFALayout = 0xC617,

		/// <summary>
		/// Lookup table that maps stored values to linear values.Used in Raw IFD of DNG files.
		/// </summary>
		LinearizationTable = 0xC618,

		/// <summary>
		/// Repeat pattern size for BlackLevel tag.Used in Raw IFD of DNG files.
		/// </summary>
		BlackLevelRepeatDim = 0xC619,

		/// <summary>
		/// Specifies the zero light encoding level.Used in Raw IFD of DNG files.
		/// </summary>
		BlackLevel = 0xC61A,

		/// <summary>
		/// Specifies the difference between zero light encoding level for each column and the baseline zero light encoding level.Used in Raw IFD of DNG files.
		/// </summary>
		BlackLevelDeltaH = 0xC61B,

		/// <summary>
		/// Specifies the difference between zero light encoding level for each row and the baseline zero light encoding level.Used in Raw IFD of DNG files.
		/// </summary>
		BlackLevelDeltaV = 0xC61C,

		/// <summary>
		/// Specifies the fully saturated encoding level for the raw sample values.Used in Raw IFD of DNG files.
		/// </summary>
		WhiteLevel = 0xC61D,

		/// <summary>
		/// For cameras with non-square pixels, specifies the default scale factors for each direction to convert the image to square pixels.Used in Raw IFD of DNG files.
		/// </summary>
		DefaultScale = 0xC61E,

		/// <summary>
		/// Specifies the origin of the final image area, ignoring the extra pixels at edges used to prevent interpolation artifacts.Used in Raw IFD of DNG files.
		/// </summary>
		DefaultCropOrigin = 0xC61F,

		/// <summary>
		/// Specifies size of final image area in raw image coordinates.Used in Raw IFD of DNG files.
		/// </summary>
		DefaultCropSize = 0xC620,

		/// <summary>
		/// Defines a transformation matrix that converts XYZ values to reference camera native color space values, under the first calibration illuminant.Used in IFD 0 of DNG files.
		/// </summary>
		ColorMatrix1 = 0xC621,

		/// <summary>
		/// Defines a transformation matrix that converts XYZ values to reference camera native color space values, under the second calibration illuminant.Used in IFD 0 of DNG files.
		/// </summary>
		ColorMatrix2 = 0xC622,

		/// <summary>
		/// Defines a calibration matrix that transforms reference camera native space values to individual camera native space values under the first calibration illuminant.Used in IFD 0 of DNG files.
		/// </summary>
		CameraCalibration1 = 0xC623,

		/// <summary>
		/// Defines a calibration matrix that transforms reference camera native space values to individual camera native space values under the second calibration illuminant.Used in IFD 0 of DNG files.
		/// </summary>
		CameraCalibration2 = 0xC624,

		/// <summary>
		/// Defines a dimensionality reduction matrix for use as the first stage in converting color camera native space values to XYZ values, under the first calibration illuminant.Used in IFD 0 of DNG files.
		/// </summary>
		ReductionMatrix1 = 0xC625,

		/// <summary>
		/// Defines a dimensionality reduction matrix for use as the first stage in converting color camera native space values to XYZ values, under the second calibration illuminant.Used in IFD 0 of DNG files.
		/// </summary>
		ReductionMatrix2 = 0xC626,

		/// <summary>
		/// Pertaining to white balance, defines the gain, either analog or digital, that has been applied to the stored raw values.Used in IFD 0 of DNG files.
		/// </summary>
		AnalogBalance = 0xC627,

		/// <summary>
		/// Specifies the selected white balance at the time of capture, encoded as the coordinates of a perfectly neutral color in linear reference space values.Used in IFD 0 of DNG files.
		/// </summary>
		AsShotNeutral = 0xC628,

		/// <summary>
		/// Specifies the selected white balance at the time of capture, encoded as x-y chromaticity coordinates.Used in IFD 0 of DNG files.
		/// </summary>
		AsShotWhiteXY = 0xC629,

		/// <summary>
		/// Specifies in EV units how much to move the zero point for exposure compensation.Used in IFD 0 of DNG files.
		/// </summary>
		BaselineExposure = 0xC62A,

		/// <summary>
		/// Specifies the relative noise of the camera model at a baseline ISO value of 100, compared to reference camera model.Used in IFD 0 of DNG files.
		/// </summary>
		BaselineNoise = 0xC62B,

		/// <summary>
		/// Specifies the relative amount of sharpening required for this camera model, compared to reference camera model.Used in IFD 0 of DNG files.
		/// </summary>
		BaselineSharpness = 0xC62C,

		/// <summary>
		/// For CFA images, specifies, in arbitrary units, how closely the values of the green pixels in the blue/green rows track the values of the green pixels in the red/green rows. Used in Raw IFD of DNG files.	
		/// </summary>
		BayerGreenSplit = 0xC62D,

		/// <summary>
		/// Specifies the fraction of the encoding range above which the response may become significantly non-linear. Used in IFD 0 of DNG files.	
		/// </summary>
		LinearResponseLimit = 0xC62E,

		/// <summary>
		/// Serial number of camera. Used in IFD 0 of DNG files.	
		/// </summary>
		CameraSerialNumber = 0xC62F,

		/// <summary>
		/// Information about the lens. Used in IFD 0 of DNG files.	
		/// </summary>
		LensInfo = 0xC630,

		/// <summary>
		/// Normally for non-CFA images, provides a hint about how much chroma blur ought to be applied. Used in Raw IFD of DNG files.	
		/// </summary>
		ChromaBlurRadius = 0xC631,

		/// <summary>
		/// Provides a hint about the strength of the camera's anti-aliasing filter. Used in Raw IFD of DNG files.	
		/// </summary>
		AntiAliasStrength = 0xC632,

		/// <summary>
		/// Used by Adobe Camera Raw to control sensitivity of its shadows slider. Used in IFD 0 of DNG files.	
		/// </summary>
		ShadowScale = 50739,

		/// <summary>
		/// Provides a way for camera manufacturers to store private data in DNG files for use by their own raw convertors.Used in IFD 0 of DNG files.
		/// </summary>
		DNGPrivateData = 0xC634,

		/// <summary>
		/// Lets the DNG reader know whether the Exif MakerNote tag is safe to preserve.Used in IFD 0 of DNG files.
		/// </summary>
		MakerNoteSafety = 0xC635,

		/// <summary>
		/// Illuminant used for first set of calibration tags.Used in IFD 0 of DNG files.
		/// </summary>
		CalibrationIlluminant1 = 0xC65A,

		/// <summary>
		/// CalibrationIlluminant2  Illuminant used for second set of calibration tags.Used in IFD 0 of DNG files.
		/// </summary>
		CalibrationIlluminant2 = 0xC65B,

		/// <summary>
		/// Specifies the amount by which the values of the DefaultScale tag need to be multiplied to achieve best quality image size.Used in Raw IFD of DNG files.
		/// </summary>
		BestQualityScale = 0xC65C,

		/// <summary>
		/// Contains a 16-byte unique identifier for the raw image file in the DNG file.Used in IFD 0 of DNG files.
		/// </summary>
		RawDataUniqueID = 50781,

		/// <summary>
		/// Alias Sketchbook Pro layer usage description.Private
		/// </summary>
		Alias_Layer_Metadata = 0xC660,

		/// <summary>
		/// Name of original file if the DNG file results from conversion from a non-DNG raw file.Used in IFD 0 of DNG files.
		/// </summary>
		OriginalRawFileName = 50827,

		/// <summary>
		/// If the DNG file was converted from a non-DNG raw file, then this tag contains the original raw data. Used in IFD 0 of DNG files.
		/// </summary>
		OriginalRawFileData = 50828,

		/// <summary>
		/// Defines the active (non-masked) pixels of the sensor. Used in Raw IFD of DNG files.
		/// </summary>
		ActiveArea = 50829,

		/// <summary>
		/// List of non-overlapping rectangle coordinates of fully masked pixels, which can optimally be used by DNG readers to measure the black encoding level. Used in Raw IFD of DNG files.
		/// </summary>
		MaskedAreas = 50830,

		/// <summary>
		/// Contains ICC profile that, in conjunction with the AsShotPreProfileMatrix tag, specifies a default color rendering from camera color space coordinates (linear reference values) into the ICC profile connection space. Used in IFD 0 of DNG files.
		/// </summary>
		AsShotICCProfile = 50831,

		/// <summary>
		/// Specifies a matrix that should be applied to the camera color space coordinates before processing the values through the ICC profile specified in the AsShotICCProfile tag. Used in IFD 0 of DNG files.
		/// </summary>
		AsShotPreProfileMatrix = 50832,

		/// <summary>
		/// The CurrentICCProfile and CurrentPreProfileMatrix tags have the same purpose and usage as
		/// </summary>
		CurrentICCProfile = 50833,

		/// <summary>
		/// The CurrentICCProfile and CurrentPreProfileMatrix tags have the same purpose and usage as
		/// </summary>
		CurrentPreProfileMatrix = 50834,

		/// <summary>
		/// The DNG color model documents a transform between camera colors and CIE XYZ values. This tag describes the colorimetric reference for the CIE XYZ values. 0 = The XYZ values are scene-referred. 1 = The XYZ values are output-referred, using the ICC profile perceptual dynamic range.Used in IFD 0 of DNG files.
		/// </summary>
		ColorimetricReference = 0xC6BF,

		/// <summary>
		/// A UTF-8 encoded string associated with the CameraCalibration1 and CameraCalibration2 tags.Used in IFD 0 of DNG files.
		/// </summary>
		CameraCalibrationSignature = 0xC6F3,

		/// <summary>
		/// A UTF-8 encoded string associated with the camera profile tags.Used in IFD 0 or CameraProfile IFD of DNG files.
		/// </summary>
		ProfileCalibrationSignature = 0xC6F4,

		/// <summary>
		/// A list of file offsets to extra Camera Profile IFDs.The format of a camera profile begins with a 16-bit byte order mark (MM or II) followed by a 16-bit "magic" number equal to 0x4352 (CR), a 32-bit IFD offset, and then a standard TIFF format IFD.Used in IFD 0 of DNG files.
		/// </summary>
		ExtraCameraProfiles = 0xC6F5,

		/// <summary>
		/// A UTF-8 encoded string containing the name of the "as shot" camera profile, if any.Used in IFD 0 of DNG files.
		/// </summary>
		AsShotProfileName = 0xC6F6,

		/// <summary>
		/// Indicates how much noise reduction has been applied to the raw data on a scale of 0.0 to 1.0. A 0.0 value indicates that no noise reduction has been applied.A 1.0 value indicates that the "ideal" amount of noise reduction has been applied, i.e.that the DNG reader should not apply additional noise reduction by default. A value of 0/0 indicates that this parameter is unknown.Used in Raw IFD of DNG files.
		/// </summary>
		NoiseReductionApplied = 0xC6F7,

		/// <summary>
		/// A UTF-8 encoded string containing the name of the camera profile. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileName = 0xC6F8,

		/// <summary>
		/// Specifies the number of input samples in each dimension of the hue/saturation/value mapping tables. The data for these tables are stored in ProfileHueSatMapData1 and ProfileHueSatMapData2 tags. Allowed values include the following: HueDivisions >= 1; SaturationDivisions >= 2; ValueDivisions >=1. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileHueSatMapDims = 0xC6F9,

		/// <summary>
		/// ProfileHueSatMapData1 Contains the data for the first hue/saturation/value mapping table. Each entry of the table contains three 32-bit IEEE floating-point values. The first entry is hue shift in degrees; the second entry is saturation scale factor; and the third entry is a value scale factor. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileHueSatMapData1 = 0xC6FA,

		/// <summary>
		/// ProfileHueSatMapData2 Contains the data for the second hue/saturation/value mapping table. Each entry of the table contains three 32-bit IEEE floating-point values. The first entry is hue shift in degrees; the second entry is saturation scale factor; and the third entry is a value scale factor. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileHueSatMapData2 = 0xC6FB,

		/// <summary>
		/// Contains a default tone curve that can be applied while processing the image as a starting point for user adjustments. The curve is specified as a list of 32-bit IEEE floating-point value pairs in linear gamma. Each sample has an input value in the range of 0.0 to 1.0, and an output value in the range of 0.0 to 1.0. The first sample is required to be (0.0, 0.0), and the last sample is required to be (1.0, 1.0). Interpolated the curve using a cubic spline.Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileToneCurve = 0xC6FC,

		/// <summary>
		/// Contains information about the usage rules for the associated camera profile.The valid values and meanings are: 0 = “allow copying”; 1 = “embed if used”; 2 = “embed never”; and 3 = “no restrictions”. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileEmbedPolicy = 0xC6FD,

		/// <summary>
		/// Contains information about the usage rules for the associated camera profile.The valid values and meanings are: 0 = “allow copying”; 1 = “embed if used”; 2 = “embed never”; and 3 = “no restrictions”. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileCopyright = 0xC6FE,

		/// <summary>
		/// Defines a matrix that maps white balanced camera colors to XYZ D50 colors.Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ForwardMatrix1 = 0xC714,

		/// <summary>
		/// Defines a matrix that maps white balanced camera colors to XYZ D50 colors.Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ForwardMatrix2 = 0xC715,

		/// <summary>
		/// A UTF-8 encoded string containing the name of the application that created the preview stored in the IFD.Used in Preview IFD of DNG files.
		/// </summary>
		PreviewApplicationName = 0xC716,

		/// <summary>
		/// A UTF-8 encoded string containing the version number of the application that created the preview stored in the IFD.Used in Preview IFD of DNG files.
		/// </summary>
		PreviewApplicationVersion = 0xC717,

		/// <summary>
		/// A UTF-8 encoded string containing the name of the conversion settings(for example, snapshot name) used for the preview stored in the IFD.Used in Preview IFD of DNG files.
		/// </summary>
		PreviewSettingsName = 0xC718,

		/// <summary>
		/// A unique ID of the conversion settings(for example, MD5 digest) used to render the preview stored in the IFD.Used in Preview IFD of DNG files.
		/// </summary>
		PreviewSettingsDigest = 0xC719,

		/// <summary>
		/// This tag specifies the color space in which the rendered preview in this IFD is stored.The valid values include: 0 = Unknown; 1 = Gray Gamma 2.2; 2 = sRGB; 3 = Adobe RGB; and 4 = ProPhoto RGB. Used in Preview IFD of DNG files.
		/// </summary>
		PreviewColorSpace = 0xC71A,

		/// <summary>
		/// This tag is an ASCII string containing the name of the date/time at which the preview stored in the IFD was rendered, encoded using ISO 8601 format.Used in Preview IFD of DNG files.
		/// </summary>
		PreviewDateTime = 0xC71B,

		/// <summary>
		/// MD5 digest of the raw image data.All pixels in the image are processed in row-scan order.Each pixel is zero padded to 16 or 32 bits deep (16-bit for data less than or equal to 16 bits deep, 32-bit otherwise). The data is processed in little-endian byte order.Used in IFD 0 of DNG files.
		/// </summary>
		RawImageDigest = 0xC71C,

		/// <summary>
		/// MD5 digest of the data stored in the OriginalRawFileData tag.Used in IFD 0 of DNG files.
		/// </summary>
		OriginalRawFileDigest = 0xC71D,

		/// <summary>
		/// Normally, pixels within a tile are stored in simple row-scan order.This tag specifies that the pixels within a tile should be grouped first into rectangular blocks of the specified size.These blocks are stored in row-scan order. Within each block, the pixels are stored in row-scan order. Used in Raw IFD of DNG files.
		/// </summary>
		SubTileBlockSize = 0xC71E,

		/// <summary>
		/// Specifies that rows of the image are stored in interleaved order.The value of the tag specifies the number of interleaved fields. Used in Raw IFD of DNG files.
		/// </summary>
		RowInterleaveFactor = 0xC71F,

		/// <summary>
		/// Specifies the number of input samples in each dimension of a default "look" table.The data for this table is stored in the ProfileLookTableData tag.Allowed values include: HueDivisions >= 1; SaturationDivisions >= 2; and ValueDivisions >= 1. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileLookTableDims = 0xC725,

		/// <summary>
		/// Default "look" table that can be applied while processing the image as a starting point for user adjustment. This table uses the same format as the tables stored in the ProfileHueSatMapData1 and ProfileHueSatMapData2 tags, and is applied in the same color space. However, it should be applied later in the processing pipe, after any exposure compensation and/or fill light stages, but before any tone curve stage. Each entry of the table contains three 32-bit IEEE floating-point values. The first entry is hue shift in degrees, the second entry is a saturation scale factor, and the third entry is a value scale factor. Used in IFD 0 or Camera Profile IFD of DNG files.
		/// </summary>
		ProfileLookTableData = 0xC726,

		/// <summary>
		/// Specifies the list of opcodes (image processing operation codes) that should be applied to the raw image, as read directly from the file. Used in Raw IFD of DNG files.
		/// </summary>
		OpcodeList1 = 0xC740,

		/// <summary>
		/// Specifies the list of opcodes (image processing operation codes) that should be applied to the raw image, just after it has been mapped to linear reference values. Used in Raw IFD of DNG files.
		/// </summary>
		OpcodeList2 = 0xC741,

		/// <summary>
		/// Specifies the list of opcodes (image processing operation codes) that should be applied to the raw image, just after it has been demosaiced. Used in Raw IFD of DNG files.
		/// </summary>
		OpcodeList3 = 0xC74E,

		/// <summary>
		/// Describes the amount of noise in a raw image; models the amount of signal-dependent photon (shot) noise and signal-independent sensor readout noise, two common sources of noise in raw images. Used in Raw IFD of DNG files.
		/// </summary>
		NoiseProfile = 0xC761,

		/// <summary>
		/// If this file is a proxy for a larger original DNG file, this tag specifics the default final size of the larger original file from which this proxy was generated. The default value for this tag is default final size of the current DNG file, which is DefaultCropSize * DefaultScale.
		/// </summary>
		OriginalDefaultFinalSize = 0xC791,

		/// <summary>
		/// If this file is a proxy for a larger original DNG file, this tag specifics the best quality final size of the larger original file from which this proxy was generated. The default value for this tag is the OriginalDefaultFinalSize, if specified. Otherwise the default value for this tag is the best quality size of the current DNG file, which is DefaultCropSize * DefaultScale * BestQualityScale.	
		/// </summary>
		OriginalBestQualityFinalSize = 0xC792,

		/// <summary>
		/// If this file is a proxy for a larger original DNG file, this tag specifics the DefaultCropSize of the larger original file from which this proxy was generated. The default value for this tag is the OriginalDefaultFinalSize, if specified. Otherwise, the default value for this tag is the DefaultCropSize of the current DNG file.	
		/// </summary>
		OriginalDefaultCropSize = 0xC793,

		/// <summary>
		/// Provides a way for color profiles to specify how indexing into a 3D HueSatMap is performed during raw conversion. This tag is not applicable to 2.5D HueSatMap tables (i.e., where the Value dimension is 1). The currently defined values are: 0 = Linear encoding (method described in DNG spec); 1 = sRGB encoding (method described in DNG spec).	
		/// </summary>
		ProfileHueSatMapEncoding = 0xC7A3,

		/// <summary>
		/// Provides a way for color profiles to specify how indexing into a 3D LookTable is performed during raw conversion. This tag is not applicable to a 2.5D LookTable (i.e., where the Value dimension is 1). The currently defined values are: 0 = Linear encoding (method described in DNG spec); 1 = sRGB encoding (method described in DNG spec).	
		/// </summary>
		ProfileLookTableEncoding = 0xC7A4,

		/// <summary>
		/// Provides a way for color profiles to increase or decrease exposure during raw conversion. BaselineExposureOffset specifies the amount (in EV units) to add to th e BaselineExposure tag during image rendering. For example, if the BaselineExposure value fo r a given camera model is +0.3, and the BaselineExposureOffset value for a given camera profile used to render an image for that camera model is -0.7, then th e actual default exposure value used during rendering will be + 0.3 - 0.7 = -0.4.
		/// </summary>
		BaselineExposureOffset = 0xC7A5,

		/// <summary>
		/// This optional tag in a color profile provides a hint to the raw converter regarding how to handle the black point(e.g., flare subtraction) during rendering.The currently defined values are: 0 = Auto; 1 = None.If set to Auto, the raw converter should perform black subtraction during rendering.The amount and method of black subtraction may be automatically determined and may be image - dependent.If set to None, the raw converter should not perform any black subtraction during rendering.This may be desirable when using color lookup tables(e.g., LookTable) or tone curves in camera profiles to perform a fixed, consistent level of black subtraction.
		/// </summary>
		DefaultBlackRender = 0xC7A6,

		/// <summary>
		/// This tag is a modified MD5 digest of the raw image data.It has been updated from the algorithm used to compute the RawImageDigest tag be more multi-processor friendly, and to support lossy compression algorithms. The details of the algorithm used to compute this tag are documented in the Adobe DNG SDK source code.
		/// </summary>
		NewRawImageDigest = 0xC7A7,

		/// <summary>
		/// The gain(what number the sample values are multiplied by) between the main raw IFD and the preview IFD containing this tag
		/// </summary>
		RawToPreviewGain = 0xC7A8,

		/// <summary>
		/// Specifies a default user crop rectangle in relative coordinates. The values must satisfy: 0.0 &lt;= top &lt; bottom &lt;= 1.0; 0.0 &lt;= left &lt; right &lt;= 1.0.The default values of(top = 0, left = 0, bottom = 1, right = 1) correspond exactly to the default crop rectangle(as specified by the DefaultCropOrigin and DefaultCropSize tags).
		/// </summary>
		DefaultUserCrop = 0xC7B5
	}
}
