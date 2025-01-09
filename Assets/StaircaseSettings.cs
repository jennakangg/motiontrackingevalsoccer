using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public struct TrialResponse {
    public float LevelIndex;
    public bool Correct;
    public override string ToString() => $"Level: {LevelIndex}, Response: {Correct}";
}

public static class Staircase {
    public static List<int> StepSigns(List<TrialResponse> responseHistory, int stepUpRequirement, int stepDownRequirement) {
        List<int> stepSigns = new List<int> {};
        int correctBuffer = 0;
        int incorrectBuffer = 0;
        foreach (var response in responseHistory) {
            correctBuffer = response.Correct ? correctBuffer + 1 : correctBuffer;
            incorrectBuffer = response.Correct ? incorrectBuffer : incorrectBuffer + 1;
            if (correctBuffer >= stepDownRequirement) {
                stepSigns.Add(-1);
                correctBuffer = incorrectBuffer = 0;
            } else if (incorrectBuffer == stepUpRequirement) {
                stepSigns.Add(1);
                correctBuffer = incorrectBuffer = 0;
            } else {
                stepSigns.Add(0);
            }
        }
        return stepSigns;
    }
    public static int NumReversals(List<int> stepSigns) {
        int numReversals = 0;
        int stepBuffer = 0;
        foreach (int stepSign in stepSigns) {
            switch(stepSign) {
                case 0:
                    continue;
                case 1:
                case -1:
                    if (stepBuffer != 0 && stepBuffer != stepSign)
                        ++numReversals;
                    break;
                default:
                    throw new Exception($"Invalid stepSign: {stepSign}");
            }
            stepBuffer = stepSign;
        }
        return numReversals;
    }
    public static (bool, float) FindLevelIndexDelta(List<TrialResponse> responseHistory, List<float> reversalStepSizes, int stepUpRequirement, int stepDownRequirement) {
        List<int> stepSigns = StepSigns(responseHistory, stepUpRequirement, stepDownRequirement);

        int numReversals = NumReversals(stepSigns);
        if (numReversals >= reversalStepSizes.Count) {
            return (false, 0);
        }

        int nextStepSign = stepSigns[stepSigns.Count - 1];
        if (nextStepSign == 0) {
            return (true, 0);
        }

        return (true, nextStepSign * reversalStepSizes[numReversals]);
    }
}

public class StaircaseSettings : MonoBehaviour {
    public int StepUpRequirement;
    public int StepDownRequirement;
    public List<float> ReversalStepSizes;
    public bool FindNextLevelIndex(List<TrialResponse> responseHistory, out float newIndexDelta) {
        (bool notCompleted, float indexDelta) = Staircase.FindLevelIndexDelta(responseHistory, ReversalStepSizes, StepUpRequirement, StepDownRequirement);
        newIndexDelta = indexDelta;
        return notCompleted;
    }
}