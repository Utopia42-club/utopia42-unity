using System;

namespace src.Service
{
    public static class FileService
    {
        public static string ResolveUrl(string url)
        {
            if (!url.StartsWith("ipfs://")) return url;

            var arr = url.Split(new[] {"ipfs://"}, StringSplitOptions.None);
            return IpfsClient.getUrl(arr[1]);
        }
    }
}