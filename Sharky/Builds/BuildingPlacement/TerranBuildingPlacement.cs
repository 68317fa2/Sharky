﻿using SC2APIProtocol;
using Sharky.Managers;
using Sharky.Pathing;
using System;
using System.Linq;
using System.Numerics;

namespace Sharky.Builds.BuildingPlacement
{
    public class TerranBuildingPlacement : IBuildingPlacement
    {
        UnitManager UnitManager;
        UnitDataManager UnitDataManager;
        DebugManager DebugManager;
        MapData MapData;
        BuildingService BuildingService;

        public TerranBuildingPlacement(UnitManager unitManager, UnitDataManager unitDataManager, DebugManager debugManager, MapData mapData, BuildingService buildingService)
        {
            UnitManager = unitManager;
            UnitDataManager = unitDataManager;
            DebugManager = debugManager;
            MapData = mapData;
            BuildingService = buildingService;
        }

        public Point2D FindPlacement(Point2D target, UnitTypes unitType, int size)
        {
            return FindProductionPlacement(target, size, 50);
        }

        public Point2D FindProductionPlacement(Point2D reference, float size, float maxDistance, float minimumMineralProximinity = 5)
        {
            var x = reference.X;
            var y = reference.Y;
            var radius = size / 2f;

            // start at 12 o'clock then rotate around 12 times, increase radius by 1 until it's more than maxDistance
            while (radius < maxDistance / 2.0)
            {
                var fullCircle = Math.PI * 2;
                var sliceSize = fullCircle / (4.0 + radius);
                var angle = 0.0;
                while (angle + (sliceSize / 2) < fullCircle)
                {
                    var point = new Point2D { X = x + (float)(radius * Math.Cos(angle)), Y = y + (float)(radius * Math.Sin(angle)) };
                    //DebugManager.DrawSphere(new Point { X = point.X, Y = point.Y, Z = 12 });

                    if (BuildingService.AreaBuildable(point.X, point.Y, 1.5f) && !BuildingService.Blocked(point.X, point.Y, 1.5f))
                    {
                        var mineralFields = UnitManager.NeutralUnits.Where(u => UnitDataManager.MineralFieldTypes.Contains((UnitTypes)u.Value.Unit.UnitType) || UnitDataManager.GasGeyserTypes.Contains((UnitTypes)u.Value.Unit.UnitType));
                        var clashes = mineralFields.Where(u => Vector2.DistanceSquared(new Vector2(u.Value.Unit.Pos.X, u.Value.Unit.Pos.Y), new Vector2(point.X, point.Y)) < (minimumMineralProximinity * minimumMineralProximinity));

                        if (clashes.Count() == 0)
                        {
                            DebugManager.DrawSphere(new Point { X = point.X, Y = point.Y, Z = 12 });
                            return point;
                        }
                    }
                    angle += sliceSize;
                }
                radius += 1;
            }

            return null;
        }
    }
}
