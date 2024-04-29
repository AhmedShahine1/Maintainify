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
        public Task<bool> PathFiles(PathFiles pathFiles);

        public Task<Images> ProfileImage();

        public Task<Images> UploadFile(IFormFile file, string folder, string oldFilePAth = null);

        public Task<string> PathFile(Images images);
        public string GetFile(string imgName);
    }
}
