package com.p2p.daemon.ipc;
import com.p2p.daemon.ipc.IDataReceiver;

interface IP2PNetworkService {
    int registerClient(String appId, IDataReceiver receiver);
    oneway void sendPayload(String targetUniqueId, in byte[] payload);
    oneway void sendGroupPayload(in List<String> targetIds, in byte[] payload);
}