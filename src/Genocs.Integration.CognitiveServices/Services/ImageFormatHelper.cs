using Microsoft.AspNetCore.Http;
using System.Text;

namespace Genocs.Integration.CognitiveServices.Services;

/// <summary>
/// Image type helper.
/// It can check the image format from the file header
/// </summary>
public class ImageFormatHelper
{

    private static readonly string[] ImagesFormats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

    /// <summary>
    /// Get the image format from the header
    /// </summary>
    /// <param name="bytes">byte stream</param>
    /// <returns></returns>
    public static ImageFormat GetImageFormat(byte[] bytes)
    {
        var bmp = Encoding.ASCII.GetBytes("BM");               // BMP
        var gif = Encoding.ASCII.GetBytes("GIF");              // GIF
        var png = new byte[] { 137, 80, 78, 71 };              // PNG
        var tiff = new byte[] { 73, 73, 42 };                  // TIFF
        var tiff2 = new byte[] { 77, 77, 42 };                 // TIFF
        var jpeg = new byte[] { 255, 216, 255, 224 };          // jpeg
        var jpeg2 = new byte[] { 255, 216, 255, 225 };         // jpeg canon

        if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
            return ImageFormat.bmp;

        if (gif.SequenceEqual(bytes.Take(gif.Length)))
            return ImageFormat.gif;

        if (png.SequenceEqual(bytes.Take(png.Length)))
            return ImageFormat.png;

        if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
            return ImageFormat.tiff;

        if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
            return ImageFormat.tiff;

        if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
            return ImageFormat.jpeg;

        if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
            return ImageFormat.jpeg;

        return ImageFormat.unknown;
    }

    /// <summary>
    /// Check if the file contains image
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <returns></returns>
    public static bool IsImage(IFormFile file)
    {
        if (file.ContentType.Contains("image"))
        {
            return true;
        }

        return ImagesFormats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
    }


    /// <summary>
    /// The supportd Image format types  
    /// </summary>
    public enum ImageFormat
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
