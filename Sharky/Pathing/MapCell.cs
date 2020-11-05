﻿namespace Sharky.Pathing
{
    public class MapCell
    {
        public float TerrainHeight { get; set; }
        public bool Walkable { get; set; }
        public bool Buildable { get; set; }
        public bool CurrentlyBuildable { get; set; }
        public bool PoweredBySelfPylon { get; set; }
        public int NumberOfEnemies { get; set; }
        public int NumberOfAllies { get; set; }
        public float EnemyGroundDpsInRange { get; set; }
        public float EnemyAirDpsInRange { get; set; }
        public float SelfGroundDpsInRange { get; set; }
        public float SelfAirDpsInRange { get; set; }
        public bool InEnemyVision { get; set; }
        public bool InSelfVision { get; set; }
    }
}