using System;
using System.Collections;
using System.Collections.Generic;
using Source.Model;
using Source.Service.Migration;

namespace Source.Service
{
    public class LandDetailsService
    {
        public static readonly LandDetailsService INSTANCE = new();
        private readonly MigrationService migrationService;

        LandDetailsService()
        {
            migrationService = new MigrationService();
            if (!migrationService.GetLatestVersion().Equals("0.3.0"))
                throw new Exception("Unsupported migration latest version.");
        }

        private LandDetails Create(Land land)
        {
            var details = new LandDetails
            {
                changes = new Dictionary<string, Block>(),
                metadata = new Dictionary<string, MetaBlockData>(),
                v = migrationService.GetLatestVersion(),
                wallet = land.owner,
                properties = land.properties
            };
            return details;
        }

        public IEnumerator GetOrCreate(List<Land> lands, Action<GetOrCreateResult> consumer)
        {
            if (lands == null) yield break;

            var result = new GetOrCreateResult();
            var enums = lands.ConvertAll(land =>
            {
                var key = land.ipfsKey?.Trim();
                if (string.IsNullOrWhiteSpace(key) || !IpfsClient.IsKeyValid(key))
                {
                    result.DetailsById[land.id] = Create(land);
                    return null;
                }

                //FIXME create new, if id is not valid
                return IpfsClient.INSATANCE.DownloadJson<LandDetails>(land.ipfsKey,
                    details => result.DetailsById[land.id] = migrationService.Migrate(land, details),
                    () =>
                    {
                        result.DetailsById[land.id] = Create(land);
                        result.IpfsFailures.Add(land.id);
                    });
            });
            foreach (var enumerator in enums)
                yield return enumerator;

            consumer.Invoke(result);
        }

        public IEnumerator Save(LandDetails details, Action<string> onSuccess, Action failure)
        {
            yield return IpfsClient.INSATANCE.UploadJson(details, onSuccess, failure);
        }

        public class GetOrCreateResult
        {
            public readonly Dictionary<long, LandDetails> DetailsById = new();
            public readonly List<long> IpfsFailures = new();
        }
    }
}