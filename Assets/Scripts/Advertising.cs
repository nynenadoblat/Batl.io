using System;
using UnityEngine;

public class Advertising : MonoBehaviour
{
    public static int countGameSession = 1;
    public Action onVideoClosed;

    public static void ShowVideoAd()
    {
        countGameSession++;
        // Placeholder for future ad network integration (Unity Ads / LevelPlay)
    }

    public void onRewardedVideoClosed(bool finished)
    {
        onVideoClosed?.Invoke();
    }
}
