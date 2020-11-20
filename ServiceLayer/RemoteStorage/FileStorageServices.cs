using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using ServiceLayer.AppConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.RemoteStorage
{
    public class FileStorageServices : IRemoteStorageService
    {
        private readonly ILogger<FileStorageServices> logger;
        private string connection = string.Empty;
        private string pattern = string.Empty;

        public FileStorageServices(ILogger<FileStorageServices> logger)
        {
            connection = AppConfiguration.GetBlockConnection(logger);
            this.logger = logger;
        }

        public Task<bool> DeleteData(StoreFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public async Task<StoreFileInfo> UploadFile(UploadDataInfo uploadInfo)
        {
            //Azure request only lowerCase container name
            string container = uploadInfo.Container.ToLower();
            // Get a reference to a share and then create it
            ShareClient share = new ShareClient(connection, container);
            share.CreateIfNotExists();

            // Get a reference to a directory and create it
            ShareDirectoryClient directory = share.GetDirectoryClient(uploadInfo.DirectoryName);
            await directory.CreateIfNotExistsAsync();

            // Get a reference to a file and upload it
            ShareFileClient file = directory.GetFileClient(GenerateFileName(uploadInfo.FileInfo.FileName));
            using (MemoryStream stream = new MemoryStream())
            {
                uploadInfo.FileInfo.CopyTo(stream);
                Response<ShareFileInfo> uploadFile = file.Create(stream.Length);
                file.UploadRange(
                    new HttpRange(0, stream.Length),
                    stream);

                return new StoreFileInfo(string.Empty, uploadFile.Value.SmbProperties.FileId);
            }
        }

        private string GenerateFileName(string fileName)
        {
            string strFileName = string.Empty;
            string[] strName = fileName.Split('.');
            strFileName = Guid.NewGuid().ToString() + "." + strName[strName.Length - 1];
            return strFileName;
        }
    }
}
