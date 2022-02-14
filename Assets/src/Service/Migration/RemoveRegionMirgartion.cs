using System.Collections.Generic;
using src.Model;
using UnityEngine;

namespace src.Service.Migration
{
    internal class RemoveRegionMigration : Migration
    {
        public RemoveRegionMigration()
            : base(new Version[] {new Version(0, 1, 0)},
                new Version(0, 2, 0))
        {
        }

        public override LandDetails Migrate(Land land, LandDetails details)
        {
            details.v = GetTarget().ToString();
            return details;
        }
    }
}