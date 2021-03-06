using System;
using System.Collections;
using System.Collections.Generic;
using Source.Model;
using Source.Service.Migration;

namespace Source.Service
{
    public class LandDetailsService
    {
        public static readonly LandDetailsService INSTANCE = new LandDetailsService();
        private readonly MigrationService migrationService;

        LandDetailsService()
        {
            migrationService = new MigrationService();
            if (!migrationService.GetLatestVersion().Equals("0.3.0"))
                throw new Exception("Unsupported migration latest version.");
        }

        private LandDetails Create(Land land)
        {
            var details = new LandDetails();
            details.changes = new Dictionary<string, Block>();
            details.metadata = new Dictionary<string, MetaBlockData>();
            details.v = migrationService.GetLatestVersion();
            details.wallet = land.owner;
            details.properties = land.properties;
            return details;
        }

        public IEnumerator GetOrCreate(List<Land> lands, Action<Dictionary<long, LandDetails>> consumer, Action failure)
        {
            if (lands == null) yield break;

            var result = new Dictionary<long, LandDetails>();
            var enums = lands.ConvertAll(land =>
            {
                if (string.IsNullOrWhiteSpace(land.ipfsKey))
                {
                    result[land.id] = Create(land);
                    return null;
                }

                //FIXME create new, if id is not valid
                return IpfsClient.INSATANCE.DownloadJson<LandDetails>(land.ipfsKey,
                    details => result[land.id] = migrationService.Migrate(land, details), failure);
            });
            foreach (var enumerator in enums)
                yield return enumerator;

            consumer.Invoke(result);
        }

        public IEnumerator Save(LandDetails details, Action<string> onSuccess, Action failure)
        {
            yield return IpfsClient.INSATANCE.UploadJson(details, onSuccess, failure);
        }
    }
}