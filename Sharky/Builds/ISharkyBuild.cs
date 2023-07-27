﻿namespace Sharky.Builds
{
    public interface ISharkyBuild
    {
        string Name();
        void StartBuild(int frame);
        void EndBuild(int frame);
        void OnFrame(ResponseObservation observation);
        void OnAfterFrame();
        bool Transition(int frame);
        List<string> CounterTransition(int frame);
    }
}
