using System;
using System.Collections;
using src.Model;
using src.Utils;

namespace src.Service
{
    public class WorldSliceService
    {
        private IEnumerator Load(SerializableVector3Int start, SerializableVector3Int end,
            Action<WorldSlice> consumer, Action failed)
        {
            string url = Constants.ApiURL + "/world/slice";
            var slice = new WorldSlice
            {
                startCoordinate = start,
                endCoordinate = end
            };
            yield return RestClient.Post(url, slice, consumer, failed);
        }
    }
}