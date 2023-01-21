using System.Text;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// This helper can check the image format from the file header
/// </summary>
public class ImageFormatHelper
{
    /// <summary>
    /// Get the image format from the header
    /// </summary>
    /// <param name="bytes">byte stream</param>
    /// <returns></returns>
    public static ImageFormatTypes GetImageFormat(byte[] bytes)
    {
        var bmp = Encoding.ASCII.GetBytes("BM");               // BMP
        var gif = Encoding.ASCII.GetBytes("GIF");              // GIF
        var png = new byte[] { 137, 80, 78, 71 };              // PNG
        var tiff = new byte[] { 73, 73, 42 };                  // TIFF
        var tiff2 = new byte[] { 77, 77, 42 };                 // TIFF
        var jpeg = new byte[] { 255, 216, 255, 224 };          // jpeg
        var jpeg2 = new byte[] { 255, 216, 255, 225 };         // jpeg canon

        if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
            return ImageFormatTypes.bmp;

        if (gif.SequenceEqual(bytes.Take(gif.Length)))
            return ImageFormatTypes.gif;

        if (png.SequenceEqual(bytes.Take(png.Length)))
            return ImageFormatTypes.png;

        if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
            return ImageFormatTypes.tiff;

        if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
            return ImageFormatTypes.tiff;

        if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
            return ImageFormatTypes.jpeg;

        if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
            return ImageFormatTypes.jpeg;

        return ImageFormatTypes.unknown;
    }


    /// <summary>
    /// The supportd Image forma types  
    /// </summary>
    public enum ImageFormatTypes
    {
        /// <summary>
        /// The unknows format
        /// </summary>
        unknown,

        /// <summary>
        /// Windows Bitmap image type
        /// </summary>
        bmp,

        /// <summary>
        /// Jpeg image type
        /// </summary>
        jpeg,

        /// <summary>
        /// Gif image type
        /// </summary>
        gif,

        /// <summary>
        /// Tiff image type
        /// </summary>
        tiff,

        /// <summary>
        /// Png image type
        /// </summary>
        png
    }
}
