using Maintainify.BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.RepositoryLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics.Eventing.Reader;

namespace Maintainify.BusinessLayer.Services
{
    public class FileHandling : IFileHandling
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;

        public FileHandling(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork)
        {
            _webHostEnvironment = webHostEnvironment;
            _unitOfWork = unitOfWork;
        }

        #region Photo Handling

        public async Task<bool> PathFiles(PathFiles pathFiles)
        {
            try
            {
                if (pathFiles == null)
                {
                    return false;
                }
                pathFiles.Id = Guid.NewGuid().ToString();
                await _unitOfWork.pathFiles.AddAsync(pathFiles);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public async Task<Images> ProfileImage(string userId, string PathType)
        {
            string fileName = "Ellipse 1064.png";
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Files", "Profiles", fileName);
            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                return null; // Return 404 Not Found if the file does not exist
            }

            try
            {
                var path = await _unitOfWork.pathFiles.FindByQuery(x => x.type == PathType).FirstAsync();
                Images images = new Images()
                {
                    ImageName = fileName,
                    pathFiles = path,
                    PathId = path.Id,
                    UserId = userId,
                    Id = Guid.NewGuid().ToString(),
                };
                await _unitOfWork.images.AddAsync(images);
                await _unitOfWork.SaveChangesAsync();

                return images;
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return null;
            }
        }

        public async Task<Images> UploadFile(IFormFile file, string folder , string userId)
        {
            PathFiles pathFiles = await _unitOfWork.pathFiles.FindByQuery(x => x.type == folder).FirstAsync();
            var uploads = Path.Combine(_webHostEnvironment.WebRootPath, pathFiles.Name);
            var uniqueFileName = RandomString(10) + "_" + file.FileName;
            var filePath = Path.Combine(uploads, uniqueFileName);
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }
            Images images = new Images()
            {
                ImageName = uniqueFileName,
                pathFiles = pathFiles,
                PathId = pathFiles.Id,
                UserId = userId,
                Id = Guid.NewGuid().ToString(),
            };
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
                await _unitOfWork.images.AddAsync(images);
                await _unitOfWork.SaveChangesAsync();
            }

            return images;
        }

        public async Task<bool> DeleteFile(Images images)
        {
            try
            {
                _unitOfWork.images.Delete(images);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> PathFile(Images images)
        {
            if (images == null)
                return null;
            PathFiles pathFiles =await _unitOfWork.pathFiles.FindByQuery(x => x.Id == images.PathId).FirstAsync();
            return Path.Combine(_webHostEnvironment.WebRootPath, pathFiles.Name, images.ImageName);
        }

        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string? GetFile(string foo)
        {
            return _webHostEnvironment.WebRootFileProvider.GetFileInfo(foo)?.PhysicalPath;
        }

        #endregion Photo Handling
    }
}
