using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ServiceLayer.AppConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.RemoteStorage
{
    public class CloudinaryStorageServer : IRemoteStorageService
    {
        /*
        appsetting.json section 
        "Cloudinary": {
            "CloudName": "your cloudinary name",
            "APIKey": "your api",
            "APISecret": "your api secret"
          },
         */

        private string cloudName = string.Empty;
        private string apiKey = string.Empty;
        private string apiSecret = string.Empty;
        
        public CloudinaryStorageServer(ILogger<CloudinaryStorageServer> logger)
        {
            Logger = logger;

            cloudName = AppConfiguration.GetSetting<string>("Cloudinary:CloudName", Logger);
            apiKey = AppConfiguration.GetSetting<string>("Cloudinary:APIKey", Logger);
            apiSecret = AppConfiguration.GetSetting<string>("Cloudinary:APISecret", Logger);
        
            if(!ValidaAccountInf())
            {
                throw new Exception("Cloudinary provider is not initialize correctly!!!");
            }
        }

        public ILogger<CloudinaryStorageServer> Logger { get; }

        public async Task<bool> DeleteData(StoreFileInfo fileInfo)
        {
            try
            {
                Account account = new Account(
                       cloudName,
                       apiKey,
                       apiSecret);

                Cloudinary cloudinary = new Cloudinary(account);
                DeletionParams deletionParams = new DeletionParams(fileInfo.FileId);
                DeletionResult result = await cloudinary.DestroyAsync(deletionParams);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                Logger.LogError(e, e.Message);
                return false;
            }
        }

        public async Task<StoreFileInfo> UploadFile(UploadDataInfo uploadInfo)
        {
            try
            {
                if (uploadInfo.FileInfo == null
                   || uploadInfo.FileInfo.Length == 0
                   || string.IsNullOrEmpty(uploadInfo.FileInfo.FileName))
                {
                    Logger.LogError("File is required");
                    return new StoreFileInfo(false);
                }


                Account account = new Account(
                    cloudName,
                    apiKey,
                    apiSecret);

                Cloudinary cloudinary = new Cloudinary(account);

                var uploadParams = new ImageUploadParams();

                ImageUploadResult uploadResult;
                using (MemoryStream stream = new MemoryStream())
                {
                    uploadInfo.FileInfo.CopyTo(stream);
                    stream.Position = 0;
                    uploadParams.File = new FileDescription(uploadInfo.FileInfo.FileName, stream);
                    if (!string.IsNullOrEmpty(uploadInfo.DirectoryName))
                    {
                        uploadParams.Folder = uploadInfo.DirectoryName;
                    }
                    uploadResult = await cloudinary.UploadAsync(uploadParams);
                }


                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new StoreFileInfo(uploadResult.Uri.AbsoluteUri, uploadResult.PublicId);
                }
                else
                {
                    return new StoreFileInfo(false);
                }
            }
            catch(Exception e)
            {
                Logger.LogError(e, e.Message);
                return new StoreFileInfo(false);
            }
        }

        private bool ValidaAccountInf()
        {
            if(string.IsNullOrEmpty(cloudName))
            {
                Logger.LogError("Cloudinary account is required");
                return false;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                Logger.LogError("Cloudinary apiKey is required");
                return false;
            }
            
            if (string.IsNullOrEmpty(apiSecret))
            {
                Logger.LogError("Cloudinary apiSecret is required");
                return false;
            }

            return true;
        }
    }
}
