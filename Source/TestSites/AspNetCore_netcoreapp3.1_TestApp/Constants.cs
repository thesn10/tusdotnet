﻿namespace AspNetCore_netcoreapp3._1_TestApp
{
    /// <summary>
    /// For production it is better to use for example Microsoft.Extensions.Configuration
    /// or Environment Variables or whatever
    /// </summary>
    public static class Constants
    {
        public const string FileDirectory = @"C:\tusfiles\";
        public const int FileExpirationInMinutes = 10;
    }
}