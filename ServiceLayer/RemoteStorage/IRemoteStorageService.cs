using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.RemoteStorage
{
    public class StoreFileInfo
    {
        public StoreFileInfo(string fileAddress, string fileId)
            : this(true)
        {
            FileAddress = fileAddress;
            FileId = fileId;
        }

        public StoreFileInfo(bool result)
        {
            BoolResult = result;
        }

        public bool BoolResult { get; private set; }
        public string FileAddress { get; private set; }
        public string FileId { get; private set; }


    }

    public class UploadDataInfo
    {
        public UploadDataInfo(IFormFile fileInfo)
        {
            FileInfo = fileInfo;
        }
        
        public UploadDataInfo(IFormFile fileInfo, string container, string directoryName)
            : this(fileInfo)
        {
            Container = container;
            this.DirectoryName = directoryName;
        }

        public UploadDataInfo(IFormFile fileInfo, string directoryName)
            : this(fileInfo)
        {
            this.DirectoryName = directoryName;
        }

        public IFormFile FileInfo { get; private set; }
        public string Container { get; private set; }
        public string DirectoryName { get; private set; }
    }


    public interface IRemoteStorageService
    {
        public Task<StoreFileInfo> UploadFile(UploadDataInfo uploadInfo);

        public Task<bool> DeleteData(StoreFileInfo fileInfo);
    }
}
