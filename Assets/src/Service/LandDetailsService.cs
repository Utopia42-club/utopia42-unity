using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using src.Model;
using src.Service.Ethereum;
using src.Service.Migration;

namespace src.Service
{
    internal class LandDetailsService
    {
        internal static readonly LandDetailsService INSTANCE = new LandDetailsService();
        private readonly MigrationService migrationService;
        LandDetailsService()
        {
            migrationService = new MigrationService();
            if (!migrationService.GetLatestVersion().Equals("0.2.0"))
                throw new Exception("Unsupported migration latest version.");
        }
        
        public IEnumerator Get(List<Land> lands, Action<Dictionary<long, LandDetails>> consumer, Action failure)
        {
            if (lands == null) yield break;

            var result = new Dictionary<long, LandDetails>();
            var enums = lands.ConvertAll(land =>
            {
                if (land.ipfsKey == null)
                {
                    result[land.id] = null;
                    return null;
                }

                return IpfsClient.INSATANCE.DownloadJson<LandDetails>(land.ipfsKey,
                    details => result[land.id] = migrationService.Migrate(land, details), failure);
            });
            foreach (var enumerator in enums)
                yield return enumerator;

            // land.properties = details.properties;
            // if (details.metadata != null)
            //     ReadMetadata(land, details, metaBlocks);
            // if (details.changes != null)
            //     ReadChanges(land, details, changes);
            
            consumer.Invoke(result);
        }

        public IEnumerator Save(LandDetails details, Action<string> onSuccess, Action failure)
        {
            yield return IpfsClient.INSATANCE.UploadJson(details, onSuccess, failure);
        }
    }
}