using QRCoder;
using System.Drawing;

namespace iTaxSuite.Library.Extensions
{
    public class FileBinUtils
    {
        public static byte[] GenerateQRCode(string qrText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qrText))
                    throw new ArgumentException($"QRText is blank or null");

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode pngByteQRCode = new PngByteQRCode(qrCodeData);
                byte[] pngImgBytes = pngByteQRCode.GetGraphic(20);

                return pngImgBytes;
            }
            catch (Exception ex)
            {
                UI.Error(ex, $"GenerateQRCode : {ex.GetBaseException().Message}");
                return null;
            }
        }

        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null; // Or throw an exception for invalid input
            }

            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                // Image.FromStream creates an Image from the specified data stream.
                // It's crucial to dispose of the MemoryStream when done, 
                // hence the 'using' statement.
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }
    }
}
