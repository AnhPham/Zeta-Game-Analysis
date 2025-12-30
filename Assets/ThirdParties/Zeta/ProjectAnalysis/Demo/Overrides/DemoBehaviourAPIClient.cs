using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeta.ProjectAnalysis;

public class DemoBehaviourAPIClient : BehaviourAPIClient
{
    protected override int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt("LEVEL", 1);
    }

    protected override string GetCurrentScreen()
    {
        return "game";
    }
}
