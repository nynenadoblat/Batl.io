using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Advertising : MonoBehaviour
{
    public System.Action onVideoClosed;

    public static int countGameSession = 1;

    private void Start()
    {
        //Appodeal.setAutoCache(Appodeal.REWARDED_VIDEO, false);
        //Appodeal.initialize("9cb0c515be3554c7fe8f2cb02e5b54e554d20c1865ec0868", Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO, false);
        //Appodeal.setRewardedVideoCallbacks(this);
    }

    public static void ShowVideoAd()
    {
        countGameSession++;
        //if (countGameSession >= 3)
        //{
        //    if (Appodeal.isLoaded(Appodeal.REWARDED_VIDEO))
        //    {
        //        Appodeal.show(Appodeal.REWARDED_VIDEO);
        //        countGameSession = 1;
        //    }
        //    else if (Appodeal.isLoaded(Appodeal.INTERSTITIAL))
        //    {
        //        Appodeal.show(Appodeal.INTERSTITIAL);
        //        countGameSession = 1;
        //    }

        //    Appodeal.cache(Appodeal.REWARDED_VIDEO);

            
        //}
    }

    public void onRewardedVideoLoaded(bool precache)
    {
        
    }

    public void onRewardedVideoFailedToLoad()
    {
        
    }

    public void onRewardedVideoShowFailed()
    {
        
    }

    public void onRewardedVideoShown()
    {
        
    }

    public void onRewardedVideoFinished(double amount, string name)
    {
        
    }

    public void onRewardedVideoClosed(bool finished)
    {
        onVideoClosed?.Invoke();
    }

    public void onRewardedVideoExpired()
    {
        
    }

    public void onRewardedVideoClicked()
    {
        
    }
}
