using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace B3I_Market.Helpers
{
    public static class FormFileExtensions
    {
        public static string GetMd5Hash(this IFormFile file)
        {
            var stream = file.OpenReadStream();
            var md5 = MD5.Create();
            var bytes= md5.ComputeHash(stream);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}