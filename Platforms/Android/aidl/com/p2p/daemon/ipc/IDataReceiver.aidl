package com.p2p.daemon.ipc;

interface IDataReceiver {
    // oneway ensures the background service doesn't block while pushing data to the client
    oneway void onPayloadReceived(String senderUniqueId, in byte[] payload);
}