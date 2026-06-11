package com.tapempire.ads;

import android.app.Activity;
import android.app.Application;
import android.os.Bundle;
import android.os.Process;
import android.os.SystemClock;
import android.util.Log;

public class AdSessionGuard implements Application.ActivityLifecycleCallbacks {

    private static final String TAG = "AdSessionGuard";
    private static AdSessionGuard instance;

    private boolean isAdActive;
    private long timeoutMs;
    private long pausedAtMs;

    public static void init(Activity activity, long timeoutSeconds) {
        if (instance != null) {
            Log.d(TAG, "init: already initialized, skipping");
            return;
        }
        instance = new AdSessionGuard(activity, timeoutSeconds);
        Log.d(TAG, "init: initialized with timeout=" + timeoutSeconds + "s");
    }

    public static void setTimeout(long timeoutSeconds) {
        if (instance == null) return;
        instance.timeoutMs = timeoutSeconds * 1000L;
        Log.d(TAG, "setTimeout: " + timeoutSeconds + "s");
    }

    public static void setAdActive(boolean active) {
        if (instance == null) {
            Log.w(TAG, "setAdActive(" + active + "): instance is null, not initialized");
            return;
        }
        Log.d(TAG, "setAdActive(" + active + ") prev=" + instance.isAdActive);
        instance.isAdActive = active;
        if (!active) {
            instance.pausedAtMs = 0;
        }
    }

    private AdSessionGuard(Activity activity, long timeoutSeconds) {
        this.timeoutMs = timeoutSeconds * 1000L;
        activity.getApplication().registerActivityLifecycleCallbacks(this);
    }

    @Override
    public void onActivityPaused(Activity activity) {
        Log.d(TAG, "onActivityPaused: isAdActive=" + isAdActive);
        if (isAdActive) {
            pausedAtMs = SystemClock.elapsedRealtime();
        }
    }

    @Override
    public void onActivityResumed(Activity activity) {
        Log.d(TAG, "onActivityResumed: pausedAtMs=" + pausedAtMs + " isAdActive=" + isAdActive);
        if (pausedAtMs > 0) {
            long elapsed = SystemClock.elapsedRealtime() - pausedAtMs;
            pausedAtMs = 0;
            Log.d(TAG, "onActivityResumed: elapsed=" + elapsed + "ms timeout=" + timeoutMs + "ms");
            if (elapsed >= timeoutMs) {
                Log.w(TAG, "onActivityResumed: KILLING process, elapsed " + elapsed + "ms >= timeout " + timeoutMs + "ms");
                Process.killProcess(Process.myPid());
            }
        }
    }

    @Override public void onActivityCreated(Activity activity, Bundle savedInstanceState) {}
    @Override public void onActivityStarted(Activity activity) {}
    @Override public void onActivityStopped(Activity activity) {}
    @Override public void onActivitySaveInstanceState(Activity activity, Bundle outState) {}
    @Override public void onActivityDestroyed(Activity activity) {}
}
