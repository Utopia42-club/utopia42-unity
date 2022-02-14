using System.Collections.Generic;
using src.Model;
using UnityEngine;

namespace src.Service.Migration
{
    internal class GlobalToLocalMigration : Migration
    {
        public GlobalToLocalMigration()
            : base(new Version[] {new Version(0, 0, 0), new Version(0, 0, 1)},
                new Version(0, 1, 0))
        {
        }

        public override LandDetails Migrate(Land land, LandDetails details)
        {
            var pivot = new Vector3Int((int) land.x1, 0, (int) land.y1);

            var newChanges = new Dictionary<string, VoxelChange>();

            foreach (var change in details.changes)
            {
                var globalPos = LandDetails.PraseKey(change.Key);
                var local = globalPos - pivot;
                newChanges[LandDetails.FormatKey(local)] = change.Value;
            }

            details.changes = newChanges;
            details.v = GetTarget().ToString();
            return details;
        }
    }
}