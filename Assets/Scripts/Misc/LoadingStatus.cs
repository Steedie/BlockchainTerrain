using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LoadingStatus
{
    public static void SetStatus(string _status)
    {
        GameObject loadingStatus = GameObject.FindGameObjectWithTag("LoadingStatus");
        if (loadingStatus != null)
        {
            Text statusText = loadingStatus.GetComponent<Text>();
            statusText.text = _status;
        }
    }
}
