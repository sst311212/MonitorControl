package com.ahzol.monitorcontrol;

import android.util.Log;

import java.io.DataOutputStream;
import java.net.InetSocketAddress;
import java.net.Socket;

public class TcpClient {
    private String host_name;
    private int host_port;

    public TcpClient(String addr, int port) {
        host_name = addr;
        host_port = port;
    }

    public void SendMessage(String message) {
        new Thread(() -> {
            try {
                Socket ss = new Socket(host_name, host_port);
                ss.getOutputStream().write(message.getBytes("utf-8"));
                ss.close();
            } catch (Exception ex) {
                Log.d("[MRC]", ex.getMessage());
            }
        }).start();
    }
}
