package com.tapempire.ads;

import android.content.Context;
import com.google.android.gms.ads.identifier.AdvertisingIdClient;
import com.google.android.gms.ads.identifier.AdvertisingIdClient.Info;

public class AdvertisingId {
    public static String getAdvertisingId(Context context) {
        try {
            Info info = AdvertisingIdClient.getAdvertisingIdInfo(context);
            String id = info.getId();
            if (id == null || id.equals("00000000-0000-0000-0000-000000000000")) {
                return null;
            }
            return id;
        } catch (Exception e) {
            return null;
        }
    }
}
