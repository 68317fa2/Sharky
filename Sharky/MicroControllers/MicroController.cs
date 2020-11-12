﻿using SC2APIProtocol;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sharky.MicroControllers
{
    public class MicroController : IMicroController
    {
        Dictionary<UnitTypes, IIndividualMicroController> IndividualMicroControllers;
        IIndividualMicroController IndividualMicroController;

        public MicroController(Dictionary<UnitTypes, IIndividualMicroController> individualMicroControllers, IIndividualMicroController individualMicroController)
        {
            IndividualMicroControllers = individualMicroControllers;
            IndividualMicroController = individualMicroController;
        }

        public List<Action> Attack(List<UnitCommander> commanders, Point2D target, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            var actions = new List<Action>();

            foreach (var commander in commanders)
            {
                Action action;

                if (IndividualMicroControllers.TryGetValue((UnitTypes)commander.UnitCalculation.Unit.UnitType, out var individualMicroController))
                {
                    action = individualMicroController.Attack(commander, target, defensivePoint, groupCenter, frame);
                }
                else
                {
                    action = IndividualMicroController.Attack(commander, target, defensivePoint, groupCenter, frame);
                }

                if (action != null)
                {
                    actions.Add(action);
                }
            }
            return actions;
        }

        public List<Action> Retreat(List<UnitCommander> commanders, Point2D defensivePoint, Point2D groupCenter, int frame)
        {
            var actions = new List<Action>();

            foreach (var commander in commanders)
            {
                Action action;

                if (IndividualMicroControllers.TryGetValue((UnitTypes)commander.UnitCalculation.Unit.UnitType, out var individualMicroController))
                {
                    action = individualMicroController.Retreat(commander, defensivePoint, groupCenter, frame);
                }
                else
                {
                    action = IndividualMicroController.Retreat(commander, defensivePoint, groupCenter, frame);
                }

                if (action != null)
                {
                    actions.Add(action);
                }
            }
            return actions;
        }

        public List<Action> Idle(List<UnitCommander> commanders, Point2D target, Point2D defensivePoint, int frame)
        {
            return new List<Action>();
        }
    }
}
