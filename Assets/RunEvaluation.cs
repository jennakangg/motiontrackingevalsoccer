using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine.UI;
using TMPro;
using System;
// using UnityEditor.Recorder;
// using UnityEditor.Recorder.Input;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing; // For Built-in or URP


public class RunEvaluation : MonoBehaviour
{
    // private RecorderController recorderController;
    // private bool isRecording = false;
    public Material targetMaterial;
    public GameObject cameraObject;
    public GameObject movingSphere; // Changed to a GameObject representing the 3D sphere
    public Vector3 kickForce;
    public RectTransform canvasRectTransform; 
    public GameObject blackScreenPanel; 
    public GameObject answerPanel; 
    public GameObject crosshair; 
    public Button startButton; 
    public Slider progressBar; // Reference to the Slider UI element
    public bool isTrialRunning = false;
    public Vector2 screenCenter;
    public int trialNum = 0;
    public int trialID = 0;
    public float viewingDistanceMeters = 0.5842f;
    public float screenPhysicalWidthMeters = 0.6096f;
    public float screenResolutionWidthPixels = 2560.0f;
    public float screenResolutionHeightPixels = 1440.0f;
    public float contrastThreshold;
    Vector2 sceneDuration;
    float currentDuration;
    private float elapsedTime = 0f;
    private int startIndex = 0;
    private string jsonFilePath = Constants.Constants.TrialsLoadedPath;
    public bool waitingForStartInput = true;
    public GameObject gazePathRecorderObject; 
    private GazeDataRecorder gazePathRecorder;
    private float randomWaitTime;
    private Constants.Constants.ObjectMotionDirAnswers currentObjectMotionDir;
    private Constants.Constants.ObjectMotionDirAnswers answerObjectMotionDir;
    private bool waitingForAnswerInput;
    private Constants.Constants.TrialSections trialSection;

    [System.Serializable]
    public class Segment
    {
        public Vector3 cameraPosition; // Camera's position in 3D'
        public Vector3 cameraRotation; // Camera's position in 3D
        public Vector3 kickForce; // Force to apply to the object
        public Vector2 duration;
        public float contrastThreshold;
        public float ballSize;

        public Segment(Vector3 cameraPosition, Vector3 cameraRotation, Vector3 kickForce, Vector2 duration, float contrastThreshold, float ballSize)
        {
            this.cameraRotation = cameraRotation;
            this.cameraPosition = cameraPosition;
            this.kickForce = kickForce;
            this.duration = duration;
            this.contrastThreshold = contrastThreshold;
            this.ballSize = ballSize;
        }
    }


    private List<TrialStructure> testCases;

    public class TrialStructure
    {
        public int trialID;
        public Vector2 initialBallPlacement;
        public Vector2 initialCrosshairPlacement;
        public float contrastThreshold;
        public List<Segment> segments;

        public TrialStructure(int trialID, Vector2 initialBallPlacement, Vector2 initialCrosshairPlacement, float contrastThreshold, List<Segment> segments)
        {
            this.trialID = trialID;
            this.initialBallPlacement = initialBallPlacement;
            this.initialCrosshairPlacement = initialCrosshairPlacement;
            this.contrastThreshold = contrastThreshold;
            this.segments = segments;
        }
    }


    void Start()
    {
        Debug.Log("Starting study set-up");
        screenCenter = new Vector2(canvasRectTransform.rect.width / 2, canvasRectTransform.rect.height / 2);  
        testCases = LoadTrialsFromJSON(jsonFilePath);
        gazePathRecorder = gazePathRecorderObject.GetComponent<GazeDataRecorder>();
        Debug.Log("Starting study");
        startButton.gameObject.SetActive(false);
        // InitializeRecorder(); // Initialize the recorder
        StartCoroutine(RunTestCases());
    }

// RECORDER LOGIC _-------------------------------------------------------------------------------------------------------------------------------
    // private void InitializeRecorder()
    // {
    //     // Ensure output folder exists
    //     string outputFolder = Path.Combine(Application.dataPath, "Recordings");
    //     if (!Directory.Exists(outputFolder))
    //     {
    //         Directory.CreateDirectory(outputFolder);
    //     }

    //     // Create the Recorder Controller Settings
    //     var settings = new RecorderControllerSettings();
    //     settings.SetRecordModeToManual();

    //     // Initialize the Recorder Controller
    //     recorderController = new RecorderController(settings);
    // }

    // private void StartRecording()
    // {
    //     if (recorderController == null)
    //     {
    //         Debug.LogError("RecorderController is not initialized.");
    //         return;
    //     }

    //     // Ensure output folder exists
    //     string outputFolder = Path.Combine(Application.dataPath, "Recordings");
    //     if (!Directory.Exists(outputFolder))
    //     {
    //         Directory.CreateDirectory(outputFolder);
    //     }

    //     // Create dynamic movie recorder settings
    //     string trialFileName = Path.Combine(outputFolder, $"trial_video_{trialID}_kick_{kickForce}_contrast_{contrastThreshold}");
    //     var movieRecorder = new MovieRecorderSettings
    //     {
    //         OutputFile = trialFileName, // Base filename with trial number
    //         ImageInputSettings = new GameViewInputSettings(),
    //         OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4,
    //     };

    //     // Create new settings and add the movie recorder
    //     var settings = new RecorderControllerSettings();
    //     settings.SetRecordModeToManual();
    //     settings.AddRecorderSettings(movieRecorder);

    //     // Reassign the recorder controller with updated settings
    //     recorderController = new RecorderController(settings);

    //     Debug.Log($"Recording started for trial: {trialID}");
    //     recorderController.PrepareRecording();
    //     recorderController.StartRecording();
    //     isRecording = true;
    // }


    // private void StopRecording()
    // {
    //     if (recorderController == null || !isRecording)
    //     {
    //         Debug.LogWarning("No recording is active to stop.");
    //         return;
    //     }

    //     Debug.Log("Recording stopped.");
    //     recorderController.StopRecording();
    //     isRecording = false;
    // }


// RECORDER LOGIC _-------------------------------------------------------------------------------------------------------------------------------


    private List<TrialStructure> LoadTrialsFromJSON(string filePath)
    {
        List<TrialStructure> trials = new List<TrialStructure>();

        try
        {
            string json = File.ReadAllText(filePath);

            // Parse the JSON array into a list of trials using JsonUtility workaround
            Trial[] parsedTrials = JsonUtility.FromJson<TrialArrayWrapper>($"{{\"trials\":{json}}}").trials;

            foreach (var trial in parsedTrials)
            {
                List<Segment> segments = new List<Segment>();
                foreach (var segment in trial.segments)
                {
                    if (segment.camera_position.Length == 3 && segment.kick_force.Length == 3)
                    {
                        Vector3 cameraPosition = new Vector3(
                            segment.camera_position[0],
                            segment.camera_position[1],
                            segment.camera_position[2]
                        );

                        Vector3 cameraRotation = new Vector3(
                            segment.camera_rotation[0],
                            segment.camera_rotation[1],
                            segment.camera_rotation[2]
                        );

                        Vector3 kickForce = new Vector3(
                            segment.kick_force[0],
                            segment.kick_force[1],
                            segment.kick_force[2]
                        );

                        Vector2 duration = new Vector2(
                            segment.duration[0],
                            segment.duration[1]
                        );

                        Debug.Log($"Camera Position: {cameraPosition}, Kick Force: {kickForce}, Duration: {duration}");

                        segments.Add(new Segment(
                            cameraPosition,
                            cameraRotation,
                            kickForce,
                            duration,
                            segment.contrast_threshold_multiplier,
                            segment.ball_size
                        ));
                    }
                    else
                    {
                        Debug.LogError("Error: camera_position or kick_force array does not have exactly 3 elements.");
                    }
                }

                trials.Add(new TrialStructure(
                    trial.trial_id,
                    trial.initial_ball_placement.ToVector2(),
                    trial.initial_crosshair_placement.ToVector2(),
                    trial.contrast_threshold_multiplier, // Placeholder, as contrast is now per segment
                    segments
                ));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading trials from JSON: {ex.Message}");
        }

        progressBar.maxValue = trials.Count;

        return trials;
    }



    IEnumerator RunTestCases()
    {
        int i = 0;

        while (i < testCases.Count)
        {
            TrialStructure trial = testCases[i];
            Debug.Log($"Starting trial {startIndex + i}");

            trialID = trial.trialID;

            trialNum = startIndex + i;
            progressBar.value = trialNum;
            gazePathRecorder.ResetGazePath();
            waitingForStartInput = true;
            trialSection = Constants.Constants.TrialSections.WAITING_TO_START;

            Debug.Log("Waiting for space bar input to start trial...");

            progressBar.gameObject.SetActive(true);
            answerPanel.SetActive(false);

            yield return new WaitUntil(() => !waitingForStartInput);

            // movingSphere.GetComponent<MoveObjectToTrack3D>().PlaceBallAtPosition(trial.initialBallPlacement);
            trialSection = Constants.Constants.TrialSections.CROSSHAIR_FIXATION;
             // reset ball position 
            movingSphere.transform.position = new Vector3(-0.162f, 0.077f, 0.049f);
            movingSphere.transform.rotation = Quaternion.Euler(new Vector3(-0.133f, -0.093f, -0.133f));

           
            // Set the contrast value
            targetMaterial.SetFloat("_Contrast", trial.contrastThreshold);
            blackScreenPanel.SetActive(false);
            Debug.Log($"CONTRASTTTTT: {trial.contrastThreshold}");
            yield return StartCoroutine(ShowBlackScreenForceFixate(testCases[i].initialCrosshairPlacement));

            crosshair.SetActive(false);


            isTrialRunning = true;
            waitingForStartInput = false;
            
            // StartRecording(); // Start recording for the trial
            Rigidbody rb = movingSphere.GetComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None; // Remove all constraints
            foreach (Segment segment in trial.segments)
            {
               
                movingSphere.transform.position = new Vector3(-0.162f, 0.077f, 0.049f);
                movingSphere.transform.rotation = Quaternion.Euler(new Vector3(-0.133f, -0.093f, -0.133f));
                // Set camera position
                cameraObject.transform.position = segment.cameraPosition;
                cameraObject.transform.rotation = Quaternion.Euler(segment.cameraRotation);
                Debug.Log($"Camera Position: {segment.cameraPosition}");
                Debug.Log($"Camera Rotation: {segment.cameraRotation}");

                // Apply kick force to the object
                Rigidbody sphereRigidbody = movingSphere.GetComponent<Rigidbody>();
                sphereRigidbody.velocity = Vector3.zero; // Reset velocity
                // Vector3 direction = transform.left;
                kickForce = segment.kickForce;
                sphereRigidbody.AddForce(segment.kickForce, ForceMode.Impulse);
                Debug.Log($"Kick Force: {segment.kickForce}");

                if (kickForce.x > 0){
                    currentObjectMotionDir = Constants.Constants.ObjectMotionDirAnswers.RIGHT;
                    Debug.Log($"RIGHT OBJ MOTION");
                }
                else {
                    currentObjectMotionDir = Constants.Constants.ObjectMotionDirAnswers.LEFT;
                    Debug.Log($"LEFT OBJ MOTION");
                }

            
                movingSphere.transform.localScale =  Vector3.one * segment.ballSize;

                // Set scene duration and wait
                sceneDuration = segment.duration;
                currentDuration = UnityEngine.Random.Range(sceneDuration.x, sceneDuration.y);
                elapsedTime = 0f;
                trialSection = Constants.Constants.TrialSections.TRIAL;
                yield return StartCoroutine(WaitForSceneDuration());
            }
            Debug.Log("Stopping recording for the trial...");
            // StopRecording(); // Stop recording for the trial

            // Ask for input on left/right/stop
            // show screen to ask 
             answerPanel.SetActive(true);
            waitingForAnswerInput = true;
            Debug.Log("Waiting for ANSWER to trial...");
            trialSection = Constants.Constants.TrialSections.ANSWER;
            yield return new WaitUntil(() => !waitingForAnswerInput);
            Debug.Log($"Got answer!: TRIAL LENGTH {testCases.Count}, PROGRESS BAR MAX: {progressBar.maxValue}");

            // record input based on left/down/up keys 
            if (answerObjectMotionDir == Constants.Constants.ObjectMotionDirAnswers.UNSURE || !(answerObjectMotionDir == currentObjectMotionDir)){
                // if it is incorrect, reappend this trial to the list of trials, increment things to traverse
                testCases.Add(trial);
                // Update progressbar
                progressBar.maxValue = testCases.Count;
                Debug.Log($"INCORRECT ANSWER OR UNSURE: TRIAL LENGTH {testCases.Count}, PROGRESS BAR MAX: {progressBar.maxValue}");
            } else {
                Debug.Log("CORRECT ANSWER");
            }


            waitingForAnswerInput = false;
            answerPanel.SetActive(false);

            // Reset to wait for space input
            blackScreenPanel.SetActive(true);
            crosshair.SetActive(true);
            waitingForStartInput = true;
            isTrialRunning = false;

            gazePathRecorder.SaveGazeDataToSingleCSV(trialNum, trial.trialID, currentObjectMotionDir.ToString(), answerObjectMotionDir.ToString());
            gazePathRecorder.ResetGazePath();
            i++;
        }
        startButton.gameObject.SetActive(true);
    }

    IEnumerator WaitForSceneDuration()
    {
        yield return new WaitForSeconds(currentDuration);
    }

    IEnumerator WaitForRandomTime()
    {
        progressBar.gameObject.SetActive(false);
        yield return new WaitForSeconds(randomWaitTime);
    }


    [System.Serializable]
    private class TrialArrayWrapper
    {
        public Trial[] trials;
    }

    [System.Serializable]
    private class Trial
    {
        public int trial_id;
        public Position initial_ball_placement;
        public Position initial_crosshair_placement;
        public float contrast_threshold_multiplier; // Matches JSON key
        public SegmentData[] segments;
    }

    [System.Serializable]
    private class Position
    {
        public float x;
        public float y;

        public Vector2 ToVector2() => new Vector2(x, y);
    }

    [System.Serializable]
    private class SegmentData
    {
        public float[] camera_position; // Change to float array
        public float[] camera_rotation; // Change to float array
        public float[] kick_force; // Change to float array
        public float[] duration;
        public float contrast_threshold_multiplier;
        public float ball_size;

        public Segment ToSegment()
        {
            return new Segment(
                new Vector3(camera_position[0], camera_position[1], camera_position[2]), // Convert array to Vector3
                new Vector3(camera_rotation[0], camera_rotation[1], camera_rotation[2]), // Convert array to Vector3
                new Vector3(kick_force[0], kick_force[1], kick_force[2]), // Convert array to Vector3
                new Vector2(duration[0], duration[1]), // Convert array to Vector2
                contrast_threshold_multiplier,
                ball_size
            );
        }
    }

// Calculate eccentricity in degrees given a pixel position
    public float CalculateEccentricity(Vector2 pixelPosition)
    {
        float distanceFromCenter = Vector2.Distance(pixelPosition, screenCenter);
        float viewingDistancePx = (screenResolutionWidthPixels / screenPhysicalWidthMeters) * viewingDistanceMeters;

        // Calculate eccentricity angle in degrees
        float eccentricity = Mathf.Atan2(distanceFromCenter, viewingDistancePx) * Mathf.Rad2Deg;
        return eccentricity;
    }

    void Update() {
        if (isTrialRunning)
        {
            if (elapsedTime < currentDuration || trialSection == Constants.Constants.TrialSections.CROSSHAIR_FIXATION)
            {
                elapsedTime += Time.deltaTime;
                // trial started after black screen
                if (trialSection == Constants.Constants.TrialSections.TRIAL || trialSection == Constants.Constants.TrialSections.CROSSHAIR_FIXATION){
                    if (IsObjectOnScreen())
                    {
                        // Debug.Log("Object on screen, tracking gaze");
                        gazePathRecorder.TrackGaze(trialSection.ToString(), trialNum, trialID, currentDuration);
                    }
                    else
                    {
                        Debug.Log("Not on screen, stopping tracking");
                    }
                }
            }
        }

        // Start trial when space is pressed
        if (waitingForStartInput && Input.GetKeyDown(KeyCode.Space) && !isTrialRunning)
        {
            waitingForStartInput = false;
            isTrialRunning = true;
            elapsedTime = 0f;
        }

        // Answer input chain
        if (waitingForAnswerInput) { 
            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                waitingForAnswerInput = false;
                answerObjectMotionDir = Constants.Constants.ObjectMotionDirAnswers.ZERO;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                waitingForAnswerInput = false;
                answerObjectMotionDir = Constants.Constants.ObjectMotionDirAnswers.LEFT;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                waitingForAnswerInput = false;
                answerObjectMotionDir = Constants.Constants.ObjectMotionDirAnswers.RIGHT;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                waitingForAnswerInput = false;
                answerObjectMotionDir = Constants.Constants.ObjectMotionDirAnswers.UNSURE;
            }
        }
    }

    bool IsObjectOnScreen()
    {
        Vector3 objectPos = movingSphere.transform.position;
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(objectPos);
        return viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1 && viewportPos.z > 0;
    }



    public float fixationTimer = 0.0f;
    public System.DateTime fixationStart;
    public float MIN_FIXATION_TIME = 2.0f;
    public float FIXATION_THREHSOLD = 4.0f; // degrees
    IEnumerator ShowBlackScreenForceFixate(Vector2 crosshairPos){
        crosshair.SetActive(true);
        progressBar.gameObject.SetActive(false);
        fixationStart = System.DateTime.Now;
        System.DateTime sinceStartingCrosshair = System.DateTime.Now;
        Rigidbody rb = movingSphere.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = Vector3.zero; // Stop any motion
        rb.constraints = RigidbodyConstraints.FreezeAll; // Freeze position and rotation


        // Don't progress trial unless they fixate on the crosshair for at least 1s with <2deg accuracy
        // // angle = 2 * atan((obj_size * 0.5) / object_distance)
        Vector2 crosshairPosPixels = new Vector2(screenResolutionWidthPixels * crosshairPos.x, screenResolutionHeightPixels * crosshairPos.y);
        float crosshairEcc = CalculateEccentricity(crosshairPosPixels);
        // gazePathRecorder

        bool done = false;
        bool lookingAtCrosshair = false;
        float timeSinceFixation = 0.0f;
        while (!done){
            var curGazePos = gazePathRecorder.GetMostRecentGazePos();
            Vector2 curGazePosPixels = new Vector2(screenResolutionWidthPixels * curGazePos.x, screenResolutionHeightPixels * curGazePos.y);
            float gazeEcc = CalculateEccentricity(curGazePosPixels);
            float fixationAcc = Mathf.Abs(crosshairEcc - gazeEcc); // sorta stupid way of doing this but i think it works? (niall)

            if (fixationAcc < FIXATION_THREHSOLD){
                if (!lookingAtCrosshair){ // First frame of fixating after not fixating
                    lookingAtCrosshair = true;
                    fixationStart = System.DateTime.Now; // reset the timer because they stopped fixating
                }
                if (lookingAtCrosshair){
                    timeSinceFixation = (float)(System.DateTime.Now - fixationStart).TotalSeconds;
                    fixationTimer += timeSinceFixation;
                }
                
                if (timeSinceFixation >= MIN_FIXATION_TIME || (float)(System.DateTime.Now - sinceStartingCrosshair).TotalSeconds > 5){
                    done = true; // Fixated for enough time, or gaze tracker sucks
                }
            }
            else{
                lookingAtCrosshair = false;
                fixationTimer = 0.0f;
                timeSinceFixation = 0.0f;
                fixationStart = System.DateTime.Now;
                yield return null;
            }

            print("cross pos: " + crosshairPosPixels.ToString("F3"));
            print("gaze pos: " + curGazePosPixels.ToString("F3"));
            print("fixation acc: " + fixationAcc.ToString("F5"));
            print("fixation timer: " + fixationTimer.ToString("F5"));
            print("time since fix: " + timeSinceFixation.ToString());
            print("=====");
            yield return null;
        }
        randomWaitTime = UnityEngine.Random.Range(0.0f, 1.0f);
        // // Wait for a random duration between 1 and 2 seconds
        yield return new WaitForSeconds(randomWaitTime);

        blackScreenPanel.SetActive(false);
        crosshair.SetActive(false);

    }
}
 