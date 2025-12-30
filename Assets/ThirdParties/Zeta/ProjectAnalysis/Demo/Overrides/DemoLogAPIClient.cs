using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeta.ProjectAnalysis;

public class DemoLogAPIClient : LogAPIClient
{
    protected override string GetUserDataJson()
    {
        return PlayerPrefs.GetString("ZETA_PROJECT_USER_ID");
    }

    protected override string GetCurrentLevelName()
    {
        return PlayerPrefs.GetInt("LEVEL", 1).ToString();
    }
}
