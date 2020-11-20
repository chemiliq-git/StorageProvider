using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace ServiceLayer.AppConfig
{
    public static class AppConfiguration
    {
        private static IConfiguration currentConfig;

        public static void SetConfig(IConfiguration configuration)
        {
            ///tortoiseGit
			currentConfig = configuration;
        }


        public static T GetSetting<T>(string settingName, ILogger logger)
        {
            try
            {
                return currentConfig.GetValue<T>(settingName);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Error reading setting '{0}", settingName));
                throw (ex);
            }
        }

        public static string GetBlockConnection(ILogger logger)
        {
            try
            {
                return GetSetting<string>("BlobAzureSettings:Connection", logger);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Error reading setting '{0}'", "BlobAzureSettings:Connection"));
                throw (ex);
            }
        }

    }
}
