﻿using SC2APIProtocol;
using Sharky.Managers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.Builds.MacroServices
{
    public class BuildDefenseService
    {
        MacroData MacroData;
        BuildingBuilder BuildingBuilder;
        UnitDataManager UnitDataManager;
        UnitManager UnitManager;
        BaseManager BaseManager;
        TargetingManager TargetingManager;

        int defensivePointLastFailFrame;

        public BuildDefenseService(MacroData macroData, BuildingBuilder buildingBuilder, UnitDataManager unitDataManager, UnitManager unitManager, BaseManager baseManager, TargetingManager targetingManager)
        {
            MacroData = macroData;
            BuildingBuilder = buildingBuilder;
            UnitDataManager = unitDataManager;
            UnitManager = unitManager;
            BaseManager = baseManager;
            TargetingManager = targetingManager;

            defensivePointLastFailFrame = 0;
        }

        public List<Action> BuildDefensiveBuildings()
        {
            var commands = new List<Action>();

            foreach (var unit in MacroData.BuildDefensiveBuildings)
            {
                if (unit.Value)
                {
                    var unitData = UnitDataManager.BuildingData[unit.Key];
                    var command = BuildingBuilder.BuildBuilding(MacroData, unit.Key, unitData, TargetingManager.DefensePoint);
                    if (command != null)
                    {
                        commands.Add(command);
                        return commands;
                    }
                }
            }

            return commands;
        }

        public List<Action> BuildDefensiveBuildingsAtDefensivePoint()
        {
            var commands = new List<Action>();

            if (defensivePointLastFailFrame < MacroData.Frame - 100)
            {
                foreach (var unit in MacroData.DesiredDefensiveBuildingsAtDefensivePoint)
                {
                    if (unit.Value > 0)
                    {
                        var unitData = UnitDataManager.BuildingData[unit.Key];
                        if (UnitManager.SelfUnits.Count(u => u.Value.Unit.UnitType == (uint)unit.Key && Vector2.DistanceSquared(new Vector2(u.Value.Unit.Pos.X, u.Value.Unit.Pos.Y), new Vector2(TargetingManager.DefensePoint.X, TargetingManager.DefensePoint.Y)) < MacroData.DefensiveBuildingMaximumDistance * MacroData.DefensiveBuildingMaximumDistance) + UnitManager.Commanders.Values.Count(c => c.UnitCalculation.UnitClassifications.Contains(UnitClassification.Worker) && c.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)unitData.Ability)) < unit.Value)
                        {
                            var command = BuildingBuilder.BuildBuilding(MacroData, unit.Key, unitData, TargetingManager.DefensePoint, false, MacroData.DefensiveBuildingMaximumDistance);
                            if (command != null)
                            {
                                commands.Add(command);
                                return commands;
                            }
                            else
                            {
                                defensivePointLastFailFrame = MacroData.Frame;
                            }
                        }
                    }
                }
            }

            return commands;
        }

        public List<Action> BuildDefensiveBuildingsAtEveryBase()
        {
            var commands = new List<Action>();

            foreach (var unit in MacroData.DesiredDefensiveBuildingsAtEveryBase)
            {
                if (unit.Value > 0)
                {
                    var unitData = UnitDataManager.BuildingData[unit.Key];

                    var orderedBuildings = UnitManager.Commanders.Values.Count(c => c.UnitCalculation.UnitClassifications.Contains(UnitClassification.Worker) && c.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)unitData.Ability));
                    foreach (var baseLocation in BaseManager.SelfBases)
                    {
                        if (baseLocation.MineralLineDefenseUnbuildableFrame < MacroData.Frame - 100)
                        {
                            if (UnitManager.SelfUnits.Count(u => u.Value.Unit.UnitType == (uint)unit.Key && Vector2.DistanceSquared(new Vector2(u.Value.Unit.Pos.X, u.Value.Unit.Pos.Y), new Vector2(baseLocation.Location.X, baseLocation.Location.Y)) < MacroData.DefensiveBuildingMaximumDistance * MacroData.DefensiveBuildingMaximumDistance) + orderedBuildings < unit.Value)
                            {
                                var command = BuildingBuilder.BuildBuilding(MacroData, unit.Key, unitData, baseLocation.Location, false, MacroData.DefensiveBuildingMaximumDistance);
                                if (command != null)
                                {
                                    commands.Add(command);
                                    return commands;
                                }
                                else
                                {
                                    baseLocation.MineralLineDefenseUnbuildableFrame = MacroData.Frame;
                                }
                            }
                        }
                    }

                }
            }

            return commands;
        }

        public IEnumerable<Action> BuildDefensiveBuildingsAtEveryMineralLine()
        {
            var commands = new List<SC2APIProtocol.Action>();

            foreach (var unit in MacroData.DesiredDefensiveBuildingsAtEveryMineralLine)
            {
                if (unit.Value > 0)
                {
                    var unitData = UnitDataManager.BuildingData[unit.Key];

                    var orderedBuildings = UnitManager.Commanders.Values.Count(c => c.UnitCalculation.UnitClassifications.Contains(UnitClassification.Worker) && c.UnitCalculation.Unit.Orders.Any(o => o.AbilityId == (uint)unitData.Ability));
                    foreach (var baseLocation in BaseManager.SelfBases)
                    {
                        if (baseLocation.MineralLineDefenseUnbuildableFrame < MacroData.Frame - 100)
                        {
                            if (UnitManager.SelfUnits.Count(u => u.Value.Unit.UnitType == (uint)unit.Key && Vector2.DistanceSquared(new Vector2(u.Value.Unit.Pos.X, u.Value.Unit.Pos.Y), new Vector2(baseLocation.MineralLineLocation.X, baseLocation.MineralLineLocation.Y)) < MacroData.DefensiveBuildingMineralLineMaximumDistance * MacroData.DefensiveBuildingMineralLineMaximumDistance) + orderedBuildings < unit.Value)
                            {
                                var command = BuildingBuilder.BuildBuilding(MacroData, unit.Key, unitData, baseLocation.MineralLineLocation, true, MacroData.DefensiveBuildingMineralLineMaximumDistance);
                                if (command != null)
                                {
                                    commands.Add(command);
                                    return commands;
                                }
                                else
                                {
                                    baseLocation.MineralLineDefenseUnbuildableFrame = MacroData.Frame;
                                }
                            }
                        }
                    }

                }
            }

            return commands;
        }
    }
}