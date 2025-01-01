// using UnityEngine;
// using UnityEditor.Recorder;
// using UnityEditor.Recorder.Input;
// using System.IO;

// public class RecordBallMovement : MonoBehaviour
// {
//     public Camera sceneCamera;         // Camera to record from
//     public Rigidbody ballRigidbody;   // Ball's Rigidbody
//     public float movementForce = 2f; // Force to apply to the ball
//     public KeyCode startKey = KeyCode.R; // Key to start recording

//     private RecorderController recorderController;
//     private bool isRecording = false;

//     void Start()
//     {
//         InitializeRecorder();
//     }

//     void Update()
//     {
//         // Start recording when the start key is pressed
//         if (Input.GetKeyDown(startKey) && !isRecording)
//         {
//             StartRecording();
//             ballRigidbody.AddForce(-Vector3.forward * movementForce, ForceMode.Impulse);
//         }

//         // Stop recording when the ball goes off-screen
//         if (isRecording && !IsBallVisible(sceneCamera, ballRigidbody.gameObject))
//         {
//             StopRecording();
//         }
//     }

//     private void InitializeRecorder()
//     {
//         // Ensure output folder exists
//         string outputFolder = Path.Combine(Application.dataPath, "Recordings");
//         if (!Directory.Exists(outputFolder))
//         {
//             Directory.CreateDirectory(outputFolder);
//         }

//         // Set up the movie recorder settings
//         var movieRecorder = new MovieRecorderSettings
//         {
//             OutputFile = Path.Combine(outputFolder, $"ball_video_{movementForce}"), // Base filename
//             ImageInputSettings = new GameViewInputSettings(),
//             OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4,
//         };

//         // Configure the AudioInputSettings if needed
//         var audioInputSettings = movieRecorder.AudioInputSettings as AudioInputSettings;

//         // Create the Recorder Controller Settings
//         var settings = new RecorderControllerSettings();
//         settings.AddRecorderSettings(movieRecorder);
//         settings.SetRecordModeToManual();

//         // Initialize the Recorder Controller
//         recorderController = new RecorderController(settings);
//     }


//     private void StartRecording()
//     {
//         if (recorderController == null)
//         {
//             Debug.LogError("RecorderController is not initialized.");
//             return;
//         }

//         Debug.Log("Recording started.");
//         recorderController.PrepareRecording();
//         recorderController.StartRecording();
//         isRecording = true;
//     }

//     private void StopRecording()
//     {
//         if (recorderController == null || !isRecording)
//         {
//             Debug.LogWarning("No recording is active to stop.");
//             return;
//         }

//         Debug.Log("Recording stopped.");
//         recorderController.StopRecording();
//         isRecording = false;
//     }

//     private bool IsBallVisible(Camera cam, GameObject obj)
//     {
//         Vector3 viewportPoint = cam.WorldToViewportPoint(obj.transform.position);
//         return true;
//         return viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1;
//     }
// }
