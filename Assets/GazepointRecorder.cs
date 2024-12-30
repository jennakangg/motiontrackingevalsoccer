using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GazepointUnity;
using Constants;

public class GazeDataRecorder : MonoBehaviour
{
    [SerializeField] private GazepointClient _eyeTracker;

    public GameObject cameraObject;
    public GameObject movingObject;
    public GameObject recorderObject;

    private List<TrialData> gazeDataList = new List<TrialData>(); 
    private bool csvInitialized = false; 

    public int textureWidth = 1920;
    public int textureHeight = 1080;
    public Vector2 mostRecentGazePos = new Vector2(0.0f, 0.0f);

    private struct TrialData 
    {
        public string trialSection;
        public int trialID;
        public int trialNumber;
        public int frameNumber;
        public Vector2 screenBallPosition;
        public GazepointClient.GazeData gazePosition;
        public float velocityTimeChange;

        public TrialData(string trialSec, int trialIndex, int trialNum, int frameNum, Vector2 ballPosition, GazepointClient.GazeData gazePos, float velChangeTime)
        {
            trialSection = trialSec;
            trialID = trialIndex;
            trialNumber = trialNum;
            frameNumber = frameNum;
            screenBallPosition = ballPosition;
            gazePosition = gazePos;
            velocityTimeChange = velChangeTime;
        }
    }

    public void ResetGazePath()
    {
        gazeDataList.Clear(); 
    }

    public Vector2 GetMostRecentGazePos(){
        return new Vector2(_eyeTracker.mostRecentX, _eyeTracker.mostRecentY);
        print("len: " + _eyeTracker.GazeValues.Count);
        if (gazeDataList.Count == 0 && Time.frameCount < 100){
            
            return new Vector2(0f, 0f);
        }
        var b = gazeDataList[gazeDataList.Count - 1].gazePosition;
        return new Vector2(b.BestPogX, b.BestPogY);
        if (_eyeTracker.GazeValues.Count > 0){
            var a = _eyeTracker.GazeValues[_eyeTracker.GazeValues.Count - 1];
            // var a = _eyeTracker.GazeValues[0];
            return new Vector2(a.BestPogX, a.BestPogY);
        }
        else{
            return new Vector2(0f, 0f);
        }
    }

        public void TrackGaze(string trialSection, int trialNumber, int trialID, float velocityTimeChange)
    {    
        int frameNumber = Time.frameCount;

        Camera cameraComponent = cameraObject.GetComponent<Camera>();

        // Get the ball's 3D world position
        Vector3 worldBallPosition = movingObject.transform.position;
        
        // Convert the 3D world position to a 2D screen position
        Vector2 screenBallPosition = cameraComponent.WorldToScreenPoint(worldBallPosition);

        // Iterate over gaze values to collect trial data
        foreach (var gazePos in _eyeTracker.GazeValues) {
            TrialData currTrialData = new TrialData(trialSection, trialID, trialNumber, frameNumber, screenBallPosition, gazePos, velocityTimeChange);
            gazeDataList.Add(currTrialData);
        }

        // Update the most recent gaze position
        mostRecentGazePos = new Vector2(gazeDataList[gazeDataList.Count-1].gazePosition.BestPogX, gazeDataList[gazeDataList.Count-1].gazePosition.BestPogY);
    }

    public void SaveGazeDataToSingleCSV(int trialNumber, int trialID, string objectDirection, string answerObjectDirection)
    {
        // Define the output folder path for the CSV
        // var csvOutputFolder = Path.Combine(Application.dataPath, "GazeData");
        var csvOutputFolder = Path.Combine($"{Constants.Constants.PathToGazeDataFolder}");
        
        if (!Directory.Exists(csvOutputFolder))
        {
            Directory.CreateDirectory(csvOutputFolder);
        }

        // Define the CSV file path
        var csvFilePath = Path.Combine(csvOutputFolder, $"{Constants.Constants.DataOutputFileName}.csv");

        // Use StreamWriter to write or append data to the CSV file
        using (StreamWriter writer = new StreamWriter(csvFilePath, append: true))
        {
            // Write the CSV header if this is the first trial being recorded
            if (!csvInitialized)
            {
                writer.WriteLine("TrialSection,TrialID,TrialNumber,FrameNumber,ScreenBallPosition,VelocityTimeChange,EndingObjectDirection,AnswerEndingObjectDirection,"
                    + "Counter,CursorX,CursorY,CursorState,"
                    + "LeftEyeX,LeftEyeY,LeftEyeZ,LeftEyePupilDiameter,LeftEyePupilValid,"
                    + "RightEyeX,RightEyeY,RightEyeZ,RightEyePupilDiameter,RightEyePupilValid,"
                    + "FixedPogX,FixedPogY,FixedPogStart,FixedPogDuration,FixedPogId,FixedPogValid,"
                    + "LeftPogX,LeftPogY,LeftPogValid,"
                    + "RightPogX,RightPogY,RightPogValid,"
                    + "BestPogX,BestPogY,BestPogValid,"
                    + "LeftPupilX,LeftPupilY,LeftPupilDiameter,LeftPupilScale,LeftPupilValid,"
                    + "RightPupilX,RightPupilY,RightPupilDiameter,RightPupilScale,RightPupilValid,"
                    + "Time,TimeTick");
                csvInitialized = true;
            }

            Debug.Log($"Len of data to write {gazeDataList.Count}");

            // Write each frame's gaze data from the gazeDataList
            foreach (var data in gazeDataList)
            {
                var gazePosition = data.gazePosition; // Assuming gazePosition is of type GazeData
                string positionString = "(" + data.screenBallPosition.x + " " + data.screenBallPosition.y + ")";
                writer.WriteLine($"{data.trialSection},{data.trialID},{data.trialNumber},{data.frameNumber},{positionString},{data.velocityTimeChange},{objectDirection},{answerObjectDirection},"
                    + $"{gazePosition.Counter},{gazePosition.CursorX},{gazePosition.CursorY},{gazePosition.CursorState},"
                    + $"{gazePosition.LeftEyeX},{gazePosition.LeftEyeY},{gazePosition.LeftEyeZ},{gazePosition.LeftEyePupilDiameter},{gazePosition.LeftEyePupilValid},"
                    + $"{gazePosition.RightEyeX},{gazePosition.RightEyeY},{gazePosition.RightEyeZ},{gazePosition.RightEyePupilDiameter},{gazePosition.RightEyePupilValid},"
                    + $"{gazePosition.FixedPogX},{gazePosition.FixedPogY},{gazePosition.FixedPogStart},{gazePosition.FixedPogDuration},{gazePosition.FixedPogId},{gazePosition.FixedPogValid},"
                    + $"{gazePosition.LeftPogX},{gazePosition.LeftPogY},{gazePosition.LeftPogValid},"
                    + $"{gazePosition.RightPogX},{gazePosition.RightPogY},{gazePosition.RightPogValid},"
                    + $"{gazePosition.BestPogX},{gazePosition.BestPogY},{gazePosition.BestPogValid},"
                    + $"{gazePosition.LeftPupilX},{gazePosition.LeftPupilY},{gazePosition.LeftPupilDiameter},{gazePosition.LeftPupilScale},{gazePosition.LeftPupilValid},"
                    + $"{gazePosition.RightPupilX},{gazePosition.RightPupilY},{gazePosition.RightPupilDiameter},{gazePosition.RightPupilScale},{gazePosition.RightPupilValid},"
                    + $"{gazePosition.Time},{gazePosition.TimeTick}");
            }
        }

        // Log a message indicating that the data has been saved
        Debug.Log($"Gaze data for trial {trialNumber} saved to {csvFilePath}");
    }
}