package com.learnmarathi.app;

import android.os.Bundle;
import android.webkit.WebView;
import android.widget.Toast;

import androidx.activity.OnBackPressedCallback;

import com.getcapacitor.BridgeActivity;

public class MainActivity extends BridgeActivity {

    private static final long EXIT_CONFIRM_WINDOW_MS = 2000;

    private long lastRootBackPress = 0;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Registered after the bridge's own callback, so the dispatcher calls this one first.
        getOnBackPressedDispatcher().addCallback(this, new OnBackPressedCallback(true) {
            @Override
            public void handleOnBackPressed() {
                WebView webView = getBridge().getWebView();
                if (webView != null && webView.canGoBack()) {
                    webView.goBack();
                    return;
                }
                long now = System.currentTimeMillis();
                if (now - lastRootBackPress < EXIT_CONFIRM_WINDOW_MS) {
                    finish();
                } else {
                    lastRootBackPress = now;
                    Toast.makeText(MainActivity.this, R.string.press_back_again, Toast.LENGTH_SHORT).show();
                }
            }
        });
    }
}
