using System;

namespace Source.Service
{
    public static class FileService
    {
        public static string ResolveUrl(string url)
        {
            if (url == null) return null;
            if (!url.StartsWith("ipfs://")) return url;

            var arr = url.Split(new[] {"ipfs://"}, StringSplitOptions.None);

            return IpfsClient.ToPublicGatewayUrl(arr[1]);
            // return IpfsClient.ToUrl(arr[1]);
        }
    }
}