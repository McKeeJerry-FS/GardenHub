using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IImageService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file);
        public string? ConvertByteArrayToFile(byte[]? FileData, string? extension, DefaultImage defaultImage);
    }
}
