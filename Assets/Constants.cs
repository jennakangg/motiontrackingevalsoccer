using System;
using UnityEngine;

namespace Constants
{
    public static class Constants
    {
        public const string ParticipantName = "jenna";
        public const int StartIndex = 0;
        public const string TrialsLoadedPath = "Assets/TrialsToLoad/generated_trials_eval_soccer_kenny_0.json";
        public const string DataOutputFileName = "gaze_data_eval_soccer_kenny_0";
        public const string PathToGazeDataFolder = "C:/Users/jk8659/NYU/research/MotionPerception2/motiontrackingevalsoccer/Assets/GazeData";

        public enum TrialSections
        {
            WAITING_TO_START,         
            CROSSHAIR_FIXATION,   
            TRIAL,       
            ANSWER
        }

        public enum ObjectMotionDirAnswers
        {
            LEFT,          
            RIGHT,    
            ZERO,       
            UNSURE
        }
    }
}