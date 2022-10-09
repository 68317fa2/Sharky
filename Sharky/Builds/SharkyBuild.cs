﻿using SC2APIProtocol;
using Sharky.Builds.MacroServices;
using Sharky.Chat;
using Sharky.DefaultBot;
using Sharky.MicroTasks;
using Sharky.MicroTasks.Macro;
using System;
using System.Collections.Generic;

namespace Sharky.Builds
{
    public abstract class SharkyBuild : ISharkyBuild
    {
        protected BuildOptions BuildOptions;
        protected SharkyOptions SharkyOptions;
        protected MacroData MacroData;
        protected ActiveUnitData ActiveUnitData;
        protected AttackData AttackData;
        protected MicroTaskData MicroTaskData;

        protected ChatService ChatService;
        protected UnitCountService UnitCountService;

        protected FrameToTimeConverter FrameToTimeConverter;
        
        protected BuildingRequestCancellingService BuildingRequestCancellingService;
        protected UpgradeRequestCancellingService UpgradeRequestCancellingService;

        protected PrePositionBuilderTask PrePositionBuilderTask;

        protected int StartFrame;
        protected bool Started;

        public SharkyBuild(DefaultSharkyBot defaultSharkyBot)
        {
            BuildOptions = defaultSharkyBot.BuildOptions;
            SharkyOptions = defaultSharkyBot.SharkyOptions;
            MacroData = defaultSharkyBot.MacroData;
            ActiveUnitData = defaultSharkyBot.ActiveUnitData;
            AttackData = defaultSharkyBot.AttackData;
            ChatService = defaultSharkyBot.ChatService;
            UnitCountService = defaultSharkyBot.UnitCountService;
            MicroTaskData = defaultSharkyBot.MicroTaskData;
            FrameToTimeConverter = defaultSharkyBot.FrameToTimeConverter;
            BuildingRequestCancellingService = defaultSharkyBot.BuildingRequestCancellingService;
            UpgradeRequestCancellingService = defaultSharkyBot.UpgradeRequestCancellingService;

            if (defaultSharkyBot.MicroTaskData.ContainsKey(typeof(PrePositionBuilderTask).Name))
            {
                PrePositionBuilderTask = (PrePositionBuilderTask)defaultSharkyBot.MicroTaskData[typeof(PrePositionBuilderTask).Name];
            }

            Started = false;
        }

        public SharkyBuild(BuildOptions buildOptions, SharkyOptions sharkyOptions, MacroData macroData, ActiveUnitData activeUnitData, AttackData attackData, MicroTaskData microTaskData,
            ChatService chatService, UnitCountService unitCountService,
            FrameToTimeConverter frameToTimeConverter)
        {
            BuildOptions = buildOptions;
            SharkyOptions = sharkyOptions;
            MacroData = macroData;
            ActiveUnitData = activeUnitData;
            AttackData = attackData;
            ChatService = chatService;
            UnitCountService = unitCountService;
            MicroTaskData = microTaskData;
            FrameToTimeConverter = frameToTimeConverter;

            if (MicroTaskData.ContainsKey(typeof(PrePositionBuilderTask).Name))
            {
                PrePositionBuilderTask = (PrePositionBuilderTask)MicroTaskData[typeof(PrePositionBuilderTask).Name];
            }
        }

        public string Name()
        {
            return GetType().Name;
        }

        public virtual void OnFrame(ResponseObservation observation)
        {
        }

        public virtual void OnAfterFrame() 
        { 
        }

        public virtual void StartBuild(int frame)
        {
            Console.WriteLine($"{frame} {FrameToTimeConverter.GetTime(frame)} Build: {Name()}");
            StartFrame = frame;

            if (!Started)
            {
                if (SharkyOptions.BuildTagsEnabled)
                {
                    ChatService.Tag($"Build-{Name()}");
                }
                Started = true;
            }

            BuildOptions.AllowBlockWall = false;
            BuildOptions.StrictGasCount = false;
            BuildOptions.StrictSupplyCount = false;
            BuildOptions.OnlyBuildWorkersWithExtraMinerals = false;
            BuildOptions.StrictWorkerCount = false;
            BuildOptions.StrictWorkersPerGas = false;
            BuildOptions.StrictWorkersPerGasCount = 3;
            BuildOptions.MaxActiveGasCount = 8;

            AttackData.UseAttackDataManager = true;
            AttackData.AttackTrigger = 1.5f;
            AttackData.RetreatTrigger = 1f;
            AttackData.AttackWhenMaxedOut = true;
            AttackData.AttackWhenOverwhelm = true;
            AttackData.RequireMaxOut = false;

            foreach (var u in MacroData.Units)
            {
                MacroData.DesiredUnitCounts[u] = 0;
            }
            foreach (var u in MacroData.Production)
            {
                MacroData.DesiredProductionCounts[u] = 0;
            }
            foreach (var u in MacroData.Tech)
            {
                MacroData.DesiredTechCounts[u] = 0;
            }
            foreach (var u in MacroData.DefensiveBuildings)
            {
                MacroData.DesiredDefensiveBuildingsCounts[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtDefensivePoint[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtEveryBase[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtNextBase[u] = 0;
                MacroData.DesiredDefensiveBuildingsAtEveryMineralLine[u] = 0;
            }

            if (MacroData.Race == SC2APIProtocol.Race.Protoss)
            {
                MacroData.DesiredProductionCounts[UnitTypes.PROTOSS_NEXUS] = 1;
            }
            else if (MacroData.Race == SC2APIProtocol.Race.Terran)
            {
                MacroData.DesiredProductionCounts[UnitTypes.TERRAN_COMMANDCENTER] = 1;
            }
            else if (MacroData.Race == SC2APIProtocol.Race.Zerg)
            {
                MacroData.DesiredProductionCounts[UnitTypes.ZERG_HATCHERY] = 1;
            }

            if (MicroTaskData.ContainsKey(typeof(AttackTask).Name))
            {
                MicroTaskData[typeof(AttackTask).Name].Enable();
            }
        }

        public virtual void EndBuild(int frame)
        {

        }

        public virtual bool Transition(int frame)
        {
            return false;
        }

        public virtual List<string> CounterTransition(int frame)
        {
            return null;
        }
    }
}
