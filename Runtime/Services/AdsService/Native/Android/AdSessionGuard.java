package com.tapempire.ads;

import android.app.Activity;
import android.app.Application;
import android.os.Bundle;
import android.os.Process;
import android.os.SystemClock;

public class AdSessionGuard implements Application.ActivityLifecycleCallbacks {

    private static AdSessionGuard instance;

    private boolean isAdActive;
    private long timeoutMs;
    private long pausedAtMs;

    public static void init(Activity activity, long timeoutSeconds) {
        if (instance != null) return;
        instance = new AdSessionGuard(activity, timeoutSeconds);
    }

    public static void setTimeout(long timeoutSeconds) {
        if (instance == null) return;
        instance.timeoutMs = timeoutSeconds * 1000L;
    }

    public static void setAdActive(boolean active) {
        if (instance == null) return;
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
        if (isAdActive) {
            pausedAtMs = SystemClock.elapsedRealtime();
        }
    }

    @Override
    public void onActivityResumed(Activity activity) {
        if (pausedAtMs > 0) {
            long elapsed = SystemClock.elapsedRealtime() - pausedAtMs;
            pausedAtMs = 0;
            if (elapsed >= timeoutMs) {
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
