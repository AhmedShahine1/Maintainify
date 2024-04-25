using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.BusinessLayer.Interface
{
    public interface IFileHandling
    {
        public Task<string> UploadFile(IFormFile file, string folder, string oldFilePAth = null);

        public Task<string> UploadPhotoByte(byte[] imgBytes, string folderName, string oldFilePAth = null);

        public Task<string> UploadPhotoBase64(string stringFile, string folderName = "Student", string oldFilePAth = null);

        public string GetFile(string imgName);
    }
}
