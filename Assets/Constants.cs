using System;
using UnityEngine;

namespace Constants
{
    public static class Constants
    {
        public const string ParticipantName = "jenna";
        public const int StartIndex = 0;
        public const string TrialsLoadedPath = "TrialsToLoad/trials.json";
        public const string DataOutputFileName = "gaze_data_fake";
        public const string PathToGazeDataFolder = "/Users/jennakang/NYU/MotionPerception2/soccerstadium/Assets/GazeData";

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