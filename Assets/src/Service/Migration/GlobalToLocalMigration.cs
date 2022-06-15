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
            var pivot = land.startCoordinate.ToVector3();

            var newChanges = new Dictionary<string, Block>();

            foreach (var change in details.changes)
            {
                var globalPos = LandDetails.ParseIntKey(change.Key);
                var local = globalPos - pivot;
                newChanges[LandDetails.FormatIntKey(local)] = change.Value;
            }

            details.changes = newChanges;
            details.v = GetTarget().ToString();
            return details;
        }
    }
}