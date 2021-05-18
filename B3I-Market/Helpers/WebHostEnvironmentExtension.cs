using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace B3I_Market.Helpers
{
    public static class WebHostEnvironmentExtensions
    {
        public static async Task<string> AddFile(this IWebHostEnvironment environment, IFormFile file, string _path)
        {
            var existingFile = UniqFile(environment, file, _path);
            if (file != null && existingFile == null)
            {
                string path = _path + Path.GetFileName(file.FileName);
                using (var fileStream = new FileStream(environment.WebRootPath + path, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }

                return path;
            }
            else
            {
                return _path + existingFile.Name;
            }
        }

        public static void DeletePhoto(this IWebHostEnvironment environment, string photoPath)
        {
            string path = environment.WebRootPath + photoPath;
            File.Delete(path);
        }

        private static IFileInfo UniqFile(this IWebHostEnvironment environment, IFormFile file, string path)
        {
            var hash = file.GetMd5Hash();
            var allhash = GetAllFilesMd5Hash(environment, path);
            if (allhash.Keys.Contains(hash))
            {
                return allhash[hash];
            }
            else
            {
                return null;
            }


        }
        private static Dictionary<string, IFileInfo> GetAllFilesMd5Hash(this IWebHostEnvironment environment, string path)
        {
            using (var md5 = MD5.Create())
            {
                var provider = environment.WebRootFileProvider;
                var files = provider.GetDirectoryContents(path).ToList();
                var streams = new List<Stream>();
                Dictionary<string, IFileInfo> result = new Dictionary<string, IFileInfo>();
                var dict = new Dictionary<IFileInfo, Stream>();
                try
                {
                    files.ForEach(p => dict.Add(p, p.CreateReadStream()));
                    files.ForEach(p =>
                        result.Add(BitConverter.ToString(md5.ComputeHash(dict[p])).Replace("-", "")
                            .ToLowerInvariant(), p));
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
                    foreach (var dictValue in dict.Values)
                    {
                        dictValue.Close();
                    }
                }

                return result;
            }




        }
    }
}