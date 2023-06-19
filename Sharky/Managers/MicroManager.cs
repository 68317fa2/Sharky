﻿using SC2APIProtocol;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sharky.Managers
{
    public class MicroManager : SharkyManager
    {      
        ActiveUnitData ActiveUnitData;
        MicroTaskData MicroTaskData;
        SharkyOptions SharkyOptions;

        public MicroManager(ActiveUnitData activeUnitData, MicroTaskData microTaskData, SharkyOptions sharkyOptions)
        {
            ActiveUnitData = activeUnitData;
            MicroTaskData = microTaskData;
            SharkyOptions = sharkyOptions;
        }

        public override bool NeverSkip { get => true; }

        public override IEnumerable<Action> OnFrame(ResponseObservation observation)
        {
            var frame = (int)observation.Observation.GameLoop;

            var actions = new List<Action>();
            foreach (var microTask in MicroTaskData.Values.Where(m => m.Enabled).OrderBy(m => m.Priority))
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                microTask.RemoveDeadUnits(ActiveUnitData.DeadUnits);
                microTask.ClaimUnits(ActiveUnitData.Commanders);

                if (frame > 10 && SharkyOptions.LogPerformance && stopwatch.ElapsedMilliseconds > 100)
                {
                    System.Console.WriteLine($"{frame} ClaimUnits {microTask.GetType().Name} {stopwatch.ElapsedMilliseconds} ms, average: {microTask.TotalFrameTime / frame} ms");
                }

                if (!SkipFrame)
                {
                    LogMissingCommanders(observation, microTask);

                    var taskActions = microTask.PerformActions(frame);

                    taskActions = FilterActions(observation, microTask, taskActions);

                    actions.AddRange(taskActions);
                }
                stopwatch.Stop();
                var time = stopwatch.ElapsedMilliseconds;
                microTask.TotalFrameTime += time;

                if (frame > 10 && SharkyOptions.LogPerformance && time > 1 && time > microTask.LongestFrame)
                {
                    microTask.LongestFrame = time;
                    System.Console.WriteLine($"{frame} {microTask.GetType().Name} {time} ms, average: {microTask.TotalFrameTime / frame} ms");
                }
                if (frame > 10 && SharkyOptions.LogPerformance && time > 100)
                {
                    System.Console.WriteLine($"{frame} {microTask.GetType().Name} {time} ms, average: {microTask.TotalFrameTime / frame} ms");
                }
            }
            if (SkipFrame)
            {
                SkipFrame = false;
            }
            return actions;
        }

        private static List<Action> FilterActions(ResponseObservation observation, MicroTasks.IMicroTask microTask, IEnumerable<Action> taskActions)
        {
            var filteredActions = new List<SC2APIProtocol.Action>();
            var tags = new List<ulong>();
            foreach (var action in taskActions)
            {
                if (action?.ActionRaw?.UnitCommand?.UnitTags == null)
                {
                    filteredActions.Add(action);
                }
                else if (action.ActionRaw.UnitCommand.UnitTags.All(tag => !observation.Observation.RawData.Units.Any(u => u.Tag == tag)))
                {
                    if (microTask.GetType().Name != "MiningTask") 
                    {
                        // System.Console.WriteLine($"{observation.Observation.GameLoop} {microTask.GetType().Name}, ignored uncontrollable unit order {action.ActionRaw.UnitCommand.AbilityId} for tags {string.Join(" ", action.ActionRaw.UnitCommand.UnitTags)}");
                    }
                }
                else if (!action.ActionRaw.UnitCommand.QueueCommand)
                {
                    if (!tags.Any(tag => action.ActionRaw.UnitCommand.UnitTags.Any(t => t == tag)))
                    {
                        filteredActions.Add(action);
                        tags.AddRange(action.ActionRaw.UnitCommand.UnitTags);
                    }
                    else
                    {
                        // System.Console.WriteLine($"{observation.Observation.GameLoop} {microTask.GetType().Name}, ignored conflicting order {action.ActionRaw.UnitCommand.AbilityId} for tags {string.Join(" ", action.ActionRaw.UnitCommand.UnitTags)}");
                    }
                }
                else
                {
                    filteredActions.Add(action);
                }
            }

            return filteredActions;
        }

        void LogMissingCommanders(ResponseObservation observation, MicroTasks.IMicroTask microTask)
        {
            return;
            if (microTask.GetType().Name == "MiningTask") { return; }
            var missingCommanders = microTask.UnitCommanders.Where(c => !observation.Observation.RawData.Units.Any(u => u.Tag == c.UnitCalculation.Unit.Tag));
            foreach (var missingCommander in missingCommanders)
            {
                System.Console.WriteLine($"{observation.Observation.GameLoop} {microTask.GetType().Name}, missing {missingCommander.UnitCalculation.Unit.UnitType}, tag {missingCommander.UnitCalculation.Unit.Tag}");
            }
        }

        public override void OnEnd(ResponseObservation observation, Result result)
        {
            base.OnEnd(observation, result);

            if (observation != null && SharkyOptions.LogPerformance)
            {
                foreach (var microTask in MicroTaskData.Values.Where(m => m.TotalFrameTime > 0).OrderBy(m => m.TotalFrameTime))
                {
                    System.Console.WriteLine($" {microTask.GetType().Name} {microTask.TotalFrameTime} ms, average: {microTask.TotalFrameTime / observation.Observation.GameLoop} ms");
                }

                System.Console.WriteLine($"{observation.Observation.GameLoop} {GetType().Name} {TotalFrameTime} ms, average: {TotalFrameTime / observation.Observation.GameLoop} ms");
            }
        }
    }
}
