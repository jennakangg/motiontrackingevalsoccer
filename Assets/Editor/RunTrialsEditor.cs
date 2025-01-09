using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RunTrials))]
public class TimeSeriesDataEditor : Editor {
    float inspectorHeight = 300f;
    float gutterSize = 30f;
    float paddingFraction = 0.1f;
    float dotRadius = 3f;
    public override void OnInspectorGUI() {
        // Draw the default inspector to show the list
        DrawDefaultInspector();

        EditorGUILayout.LabelField("Response History");

        DrawPlot();
        DrawSaveButton();
    }
    void DrawPlot() {
            // Retrieve the target data and check if it's null
            var runTrials = (RunTrials)target;
            if (runTrials.responseHistory == null || runTrials.responseHistory.Count == 0)
            {
                EditorGUILayout.HelpBox("responseHistory list is empty.", MessageType.Info);
                return;
            }


            // Force inspector to update when responseHistory changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            // Calculate the range for the Y-axis (value axis) using the LevelIndex field
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            foreach (var entry in runTrials.responseHistory)
            {
                minValue = Mathf.Min(minValue, entry.LevelIndex);
                maxValue = Mathf.Max(maxValue, entry.LevelIndex);
            }

            float valueRange = maxValue - minValue;

            minValue -= valueRange * paddingFraction / 2;
            maxValue += valueRange * paddingFraction / 2;
            valueRange = maxValue - minValue;

            // Define plot dimensions with gutters
            float inspectorWidth = EditorGUIUtility.currentViewWidth - 20f; // overflows by 20 for some reason
            Rect inspectorRect = GUILayoutUtility.GetRect(inspectorWidth, inspectorHeight);

            float plotWidth = inspectorWidth - gutterSize * 2;
            float plotHeight = inspectorHeight - gutterSize * 2;

            // Draw plot background
            Rect plotRect = new Rect(inspectorRect.x + gutterSize, inspectorRect.y + gutterSize, plotWidth, plotHeight);
            EditorGUI.DrawRect(plotRect, Color.white);

            // Draw gridlines and axes
            Handles.color = Color.gray;

            GUIStyle xLabelStyle = new GUIStyle(GUI.skin.label);
            xLabelStyle.alignment = TextAnchor.UpperCenter;
            int verticalLines = Mathf.Min(runTrials.responseHistory.Count, 10);
            for (int i = 0; i <= verticalLines; i++)
            {
                float x = plotRect.xMin + i * plotWidth / verticalLines;
                Handles.DrawLine(new Vector3(x, plotRect.yMin), new Vector3(x, plotRect.yMax));
                
                // Draw X-axis tick mark
                float tickValue = i * (runTrials.responseHistory.Count - 1) / verticalLines;
                Handles.Label(new Vector3(x, plotRect.yMax + 5), tickValue.ToString(), xLabelStyle);
            }

            GUIStyle yLabelStyle = new GUIStyle(GUI.skin.label);
            yLabelStyle.alignment = TextAnchor.MiddleLeft;
            int horizontalLines = 5;
            for (int i = 0; i <= horizontalLines; i++)
            {
                float y = plotRect.yMin + i * plotHeight / horizontalLines;
                Handles.DrawLine(new Vector3(plotRect.xMin, y), new Vector3(plotRect.xMax, y));
                
                float tickValue = maxValue - i * valueRange / horizontalLines;
                Handles.Label(new Vector3(plotRect.xMin - gutterSize, y), tickValue.ToString("F1"), yLabelStyle);
            }

            // Draw the time series plot based on LevelIndex
            for (int i = 1; i < runTrials.responseHistory.Count; i++)
            {
                // Calculate the positions in the plot space
                float x0 = plotRect.xMin + (i - 1) * plotWidth / (runTrials.responseHistory.Count - 1);
                float x1 = plotRect.xMin + i * plotWidth / (runTrials.responseHistory.Count - 1);

                float y0 = plotRect.yMax - ((runTrials.responseHistory[i - 1].LevelIndex - minValue) / valueRange * plotHeight);
                float y1 = plotRect.yMax - ((runTrials.responseHistory[i].LevelIndex - minValue) / valueRange * plotHeight);

                bool correct0 = runTrials.responseHistory[i-1].Correct;
                bool correct1 = runTrials.responseHistory[i].Correct;

                // Draw the line between two points
                Handles.color = Color.black;
                Handles.DrawLine(new Vector3(x0, y0), new Vector3(x1, y1));

                // Draw the points
                if (i == 1) {
                    Handles.color = correct0 ? Color.green : Color.red;
                    Handles.DrawSolidDisc(new Vector3(x0, y0), Vector3.forward, dotRadius);
                }
                Handles.color = correct1 ? Color.green : Color.red;
                Handles.DrawSolidDisc(new Vector3(x1, y1), Vector3.forward, dotRadius);
            }

            // Reset handle color
            Handles.color = Color.white;
    }
    void DrawSaveButton() {
        // Add a button for saving to CSV
        if (GUILayout.Button("Save Response History to CSV"))
        {
            // Prompt for the file path
            string path = EditorUtility.SaveFilePanel("Save Response History as CSV", "", "responseHistory.csv", "csv");

            // If the user didn't cancel, save the data
            if (!string.IsNullOrEmpty(path))
            {
                SaveresponseHistoryToCSV((RunTrials)target, path);
            }
        }

    }
    private void SaveresponseHistoryToCSV(RunTrials recorder, string path)
    {
        List<TrialResponse> responseHistory = recorder.responseHistory;
        using (StreamWriter writer = new StreamWriter(path))
        {
            // Write header
            writer.WriteLine("LevelIndex,Correct");

            // Write each TrialResponse to the CSV
            foreach (TrialResponse response in responseHistory)
            {
                writer.WriteLine($"{response.LevelIndex},{response.Correct}");
            }
        }

        // Refresh the asset database to make the file visible in the Unity Editor
        AssetDatabase.Refresh();
    }
}