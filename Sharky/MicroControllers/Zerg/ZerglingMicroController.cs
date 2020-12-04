﻿using SC2APIProtocol;
using Sharky.Managers;
using Sharky.Pathing;

namespace Sharky.MicroControllers.Zerg
{
    public class ZerglingMicroController : IndividualMicroController
    {
        public ZerglingMicroController(MapDataService mapDataService, UnitDataManager unitDataManager, IUnitManager unitManager, DebugManager debugManager, IPathFinder sharkyPathFinder, SharkyOptions sharkyOptions, MicroPriority microPriority, bool groupUpEnabled)
            : base(mapDataService, unitDataManager, unitManager, debugManager, sharkyPathFinder, sharkyOptions, microPriority, groupUpEnabled)
        {
            AvoidDamageDistance = 5;
        }

        protected override bool PreOffenseOrder(UnitCommander commander, Point2D target, Point2D defensivePoint, Point2D groupCenter, UnitCalculation bestTarget, int frame, out SC2APIProtocol.Action action)
        {
            action = null;

            if (commander.UnitCalculation.Unit.Health < 6)
            {
                if (AvoidDamage(commander, target, defensivePoint, frame, out action))
                {
                    return true;
                }
            }

            return false;
        }
    }
}