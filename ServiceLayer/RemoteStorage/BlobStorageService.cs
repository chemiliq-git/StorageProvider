using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using ServiceLayer.AppConfig;
using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceLayer.RemoteStorage
{
    public class BlobStorageService : IRemoteStorageService
    {
        /*
         appsetting.json section
         "BlobAzureSettings": {
            "Connection": "your connection",
            "RegexBlogPattern": "[\\/]{2}[a-z.]*\\/(?<container>[a-z]*)\\/(?<folder>[0-9a-zA-z-]*)((%2F)|(\\/))(?<filename>[a-z0-9A-Z-]*.[\\w]{3})"
          },
         */
        private const string FILE_NAME = "filename";
        private const string CONTAINER = "container";
        private const string FOLDER = "folder";

        private string connection = string.Empty;
        private string pattern = string.Empty;

        public ILogger<BlobStorageService> Logger { get; }

        public BlobStorageService(ILogger<BlobStorageService> logger)
        {
            connection = AppConfiguration.GetBlockConnection(logger);
            pattern = AppConfiguration.GetSetting<string>("BlobAzureSettings:RegexBlogPattern", logger);
            Logger = logger;
        
            if(string.IsNullOrEmpty(connection))
            {
                string message = string.Format("{0} request connection to Azure Store", nameof(BlobStorageService));
                Logger.LogError(message);
                throw new Exception(message);
            }

            if (string.IsNullOrEmpty(pattern))
            {
                string message = string.Format("{0} request patter to parse Azure AbsoluteUrl", nameof(BlobStorageService));
                Logger.LogError(message);
                throw new Exception(message);
            }
        }

            
        public async Task<StoreFileInfo> UploadFile(UploadDataInfo uploadInfo)
        {
            try
            {
                string container = string.Empty;
                if (!string.IsNullOrEmpty(uploadInfo.Container))
                {
                    container = uploadInfo.Container.ToLower();
                }
                else
                {
                    Logger.LogError("Conainer name is required");
                    return new StoreFileInfo(false);
                }

                if(uploadInfo.FileInfo == null
                    || uploadInfo.FileInfo.Length == 0
                    || string.IsNullOrEmpty(uploadInfo.FileInfo.FileName))
                {
                    Logger.LogError("File is required");
                    return new StoreFileInfo(false);
                }


                // Get a reference to a container named "sample-container" and then create it
                BlobContainerClient blobContainer = new BlobContainerClient(connection, container);
                await blobContainer.CreateIfNotExistsAsync();

                // Get a reference to a blob named "sample-file" in a container named "sample-container"
                BlobClient blob = blobContainer.GetBlobClient(
                    GenerateFileName(uploadInfo.DirectoryName, uploadInfo.FileInfo.FileName));

                // Upload local file
                using (MemoryStream stream = new MemoryStream())
                {
                    uploadInfo.FileInfo.CopyTo(stream);
                    stream.Position = 0;
                    Response<BlobContentInfo> response = await blob.UploadAsync(stream,
                       new BlobHttpHeaders
                       {
                           ContentType = uploadInfo.FileInfo.ContentType,
                       },
                        conditions: null);
                }

                return new StoreFileInfo(blob.Uri.AbsoluteUri, blob.Name);
            }
            catch(Exception e)
            {
                return new StoreFileInfo(false);
            }
        }

        public async Task<bool> DeleteData(StoreFileInfo fileInfo)
        {
            try
            {
                string container, folder, fileName;
                if (!ParseBlockName(fileInfo.FileAddress, out container, out folder, out fileName))
                {
                    return false;
                }

                BlobContainerClient blobContainer = new BlobContainerClient(connection, container);
                await blobContainer.CreateIfNotExistsAsync();

                // Get a reference to a blob named "sample-file" in a container named "sample-container"
                BlobClient blob = blobContainer.GetBlobClient(folder + "/" + fileName);
                Response response = await blob.DeleteAsync();
                return true;
            }
            catch(Exception e)
            {
                Logger.LogError(e, e.Message);
                return false;
            }
        }

        private bool ParseBlockName(string fileUrl, out string container, out string folder, out string fileName)
        {
            container = string.Empty;
            folder = string.Empty;
            fileName = string.Empty;

            if (string.IsNullOrEmpty(fileUrl))
            {
                return false;
            }

            Match match = Regex.Match(fileUrl, pattern);

            if (!match.Success)
            {
                return false;
            }
            fileName = match.Groups[FILE_NAME].ToString();
            container = match.Groups[CONTAINER].ToString(); ;
            folder = match.Groups[FOLDER].ToString();

            return true;
        }

        private string GenerateFileName(string directoryName, string fileName)
        {
            string strFileName = string.Empty;
            string[] strName = fileName.Split('.');
            strFileName = !string.IsNullOrEmpty(directoryName) ?
                directoryName + "/" + Guid.NewGuid().ToString() + "." + strName[strName.Length - 1]
                : Guid.NewGuid().ToString() + "." + strName[strName.Length - 1];
            return strFileName;
        }
    }
}
