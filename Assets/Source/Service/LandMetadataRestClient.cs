using System;
using System.Collections;
using Source.Model;
using Source.Utils;

namespace Source.Service
{
    public class LandMetadataRestClient
    {
        public static readonly LandMetadataRestClient INSTANCE = new LandMetadataRestClient();

        public IEnumerator SetLandMetadata(LandMetadata landMetadata, Action success, Action failed)
        {
            string url = Constants.ApiURL + "/land-metadata/set";
            yield return RestClient.Post(url, landMetadata, success, failed);
        }
    }
}