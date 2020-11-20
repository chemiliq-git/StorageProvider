using Microsoft.AspNetCore.Http;
using System.Drawing.Imaging;

namespace ServiceLayer.ImageValidate
{
    public class ImageValidateResult
    {
        public ImageValidateResult(bool isValid, ImageFormat imageFormat)
        {
            IsValid = isValid;
            ImageFormat = imageFormat;
        }

        public bool IsValid { get; private set; }
        public ImageFormat ImageFormat { get; private set; }

    }

    public interface IImageValidatorService
    {
        ImageValidateResult ValidateImageFile(IFormFile validateFile);
    }
}
