using Domain.Environment;
using Microsoft.AspNetCore.Http;

namespace Business.Services
{
    public class UploadHandler(StorageSetting storageSetting)
    {
        public readonly string _baseDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), storageSetting.PublicPath);

        public async Task<(bool IsSuccess, string MessageOrFilePath)> UploadAsync(
        IFormFile file,
        string subFolder,
        long sizeLimit = 5 * 1024 * 1024,
        string[]? validExtensions = null)
        {
            validExtensions ??= [".jpg", ".png"];

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!validExtensions.Contains(extension))
            {
                return (false, $"Extension is not valid ({string.Join(',', validExtensions)})");
            }

            if (file.Length > sizeLimit)
            {
                return (false, $"Maximum size can be {sizeLimit / (1024 * 1024)}MB");
            }

            string fileName = Guid.NewGuid().ToString() + extension;
            string uploadPath = Path.Combine(_baseDirectoryPath, subFolder);

            Directory.CreateDirectory(uploadPath);

            string subFilePath = Path.Combine(subFolder, fileName).Replace('\\', '/');
            string fullPath = Path.Combine(uploadPath, fileName);

            await using FileStream stream = new(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return (true, subFilePath);
        }

        public bool Remove(string filePath)
        {
            string fullPath = Path.Combine(_baseDirectoryPath, filePath);

            if (!File.Exists(fullPath))
            {
                return false;
            }

            File.Delete(fullPath);

            return true;
        }

        public (bool Exists, string MessageOrFullFilePath) GetFullFilePath(string filePath)
        {
            string fullPath = Path.Combine(_baseDirectoryPath, filePath);

            if (!File.Exists(fullPath))
            {
                return (false, "File not found");
            }

            return (true, fullPath);
        }
    }
}
