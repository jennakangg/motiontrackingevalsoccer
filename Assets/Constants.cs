using System;
using UnityEngine;

namespace Constants
{
    public static class Constants
    {
        public const string ParticipantName = "jenna";
        public const int StartIndex = 0;
        public const string TrialsLoadedPath = "Assets/TrialsToLoad/generated_trials_main_FINAL_niall_again_1.json";
        public const string DataOutputFileName = "gaze_data_contrast_niall_again_1";
        public const string PathToGazeDataFolder = "C:/Users/jenna/NYU/MotionPerceptoin/motiontrackingunity/Assets/GazeData";

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