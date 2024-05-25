using Maintainify.Core.Entity.ApplicationData;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Maintainify.BusinessLayer.Interface
{
    public interface IFileHandling
    {
        public Task<bool> AddPathFiles(PathFiles pathFiles);

        public Task<Images> ProfileImage(string userId, string PathType);

        public Task<Images> UploadFile(IFormFile file, string folder, string userId);

        Task<IEnumerable<Images>> GetFile(string folder, string userId);

        public Task<bool> DeleteFile(Images images);

        public Task<string> PathFile(Images images);
        public string GetFile(string imgName);
    }
}
