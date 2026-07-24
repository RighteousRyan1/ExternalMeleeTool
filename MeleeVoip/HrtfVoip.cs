using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee.Fighter;
using FMOD;
using LiteNetLib;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MeleeVoip;

public class HrtfVoip {
    public NetManager netManager;
    public EventBasedNetListener? netListener;
    public bool isRunning = true;

    public FMOD.System fmodSystem;

    // here for the love of the game
    const int SAMPLE_RATE = 48000;
    const int CHANNELS = 1;

    readonly ConcurrentDictionary<int, RemotePeerAudio> _remotePeers = new();

    public void Initialize(int localPort, bool isServer, string targetIp = "127.0.0.1", int targetPort = 9050) {
        InitFmod();
        InitNetwork(localPort, isServer, targetIp, targetPort);
    }

    void InitFmod() {
        Factory.System_Create(out fmodSystem).Check();

        // INITFLAGS also has other stuff like lowpass... maybe mess with it later?
        fmodSystem.init(32, INITFLAGS.NORMAL, IntPtr.Zero).Check();

        // doppler, dist factor, rolloff all at default 1.0
        fmodSystem.set3DSettings(1.0f, 1.0f, 1.0f).Check();

        Console.WriteLine("[FMOD] Audio engine initialized");
    }

    // this is borked rn
    // ugh. need to make one client both the host and server for this to work. fucking hate ts
    // TODO: setup server management and probably opus
    void InitNetwork(int localPort, bool isServer, string targetIp, int targetPort) {
        netListener = new EventBasedNetListener();
        netManager = new NetManager(netListener);

        // 0 for connecting clients. idk. ive never done true p2p. help!
        netManager.Start(isServer ? localPort : 0);

        netListener.PeerConnectedEvent += peer => {
            Console.WriteLine($"[Network] Connected to peer: {peer.Id}");
            _remotePeers.TryAdd(peer.Id, new RemotePeerAudio(peer.Id, fmodSystem));
        };

        netListener.PeerDisconnectedEvent += (peer, info) => {
            Console.WriteLine($"[Network] Peer disconnected: {peer.Id}");
            if (_remotePeers.TryRemove(peer.Id, out var audioPipeline)) {
                audioPipeline.Dispose();
            }
        };

        // netManager.NatPunchModule.In

        netListener.NetworkReceiveEvent += ClientReceivePayload;

        if (!isServer) {
            Console.WriteLine($"[Network] Connecting to {targetIp}:{targetPort}...");
            netManager.Connect(targetIp, targetPort, "MeleeVoip");
        }
    }

    VECTOR up = new() { x = 0, y = 1, z = 0 };
    VECTOR forward = new() { x = 0, y = 0, z = 1 };
    public void Update3DCoordinates(MatchData md, SlippiOnlineData sod) {
        int myPort = sod.ClientPort;

        // ensures it can be tested in an offline context
        if (myPort == 255) myPort = md.Fighters.First(x => x.SlotKind == SlotKind.Human).Port;
        var myft = md.Fighters[myPort];
        VECTOR listenerPos = myft.Position.ToFMOD();
        VECTOR velocity = myft.VelocityCombined.ToFMOD();

        fmodSystem.set3DListenerAttributes(0, ref listenerPos, ref velocity, ref forward, ref up).Check();

        for (int i = 0; i < _remotePeers.Count; i++) {
            if (!_remotePeers.TryGetValue(i, out var peer)) continue;
            var ft = md.Fighters[i];

            VECTOR peerPos = ft.Position.ToFMOD();
            VECTOR peerVel = ft.VelocityCombined.ToFMOD();
            peer.Update(peerPos, peerVel);
        }
    }

    public void Shutdown() {
        isRunning = false;
        netManager?.Stop();
        foreach (var peer in _remotePeers.Values) peer.Dispose();
        fmodSystem.close().Check();
        fmodSystem.release().Check();
    }

    void ClientReceivePayload(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) {
        if (_remotePeers.TryGetValue(peer.Id, out var audioPipeline)) {
            byte[] rawPcmBytes = reader.GetRemainingBytes();
            audioPipeline.PushAudioData(rawPcmBytes);
        }
    }
}

public class RemotePeerAudio : IDisposable {
    readonly int _peerId;
    readonly FMOD.System _system;
    Sound _voiceStream;
    Channel _channel;

    // thread safe network frames
    readonly ConcurrentQueue<byte> _pcmBuffer = new();

    // contains the pcm read callback to prevent collection
    readonly SOUND_PCMREAD_CALLBACK _readCallback;

    public RemotePeerAudio(int peerId, FMOD.System system) {
        _peerId = peerId;
        _system = system;
        _readCallback = PcmReadCallback;

        // really need to make this changed into opus before sending across the net
        CREATESOUNDEXINFO exinfo = new() {
            cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)),
            numchannels = 1, // TIL: mono for anything hrtf related
            defaultfrequency = 48000,
            format = SOUND_FORMAT.PCM16,
            decodebuffersize = 4800, // 100ms per chunk of audio
            length = uint.MaxValue,  // it will run out at some point....
            pcmreadcallback = _readCallback
        };
        // FMOD_OPENUSER = use a polling callback, aka _readCallback
        _system.createStream((string)null, MODE._3D | MODE.OPENUSER | MODE.LOOP_NORMAL | MODE._3D_LINEARROLLOFF, ref exinfo, out _voiceStream).Check();

        // sets the acceptable HRTF distance falloffs, in meters
        _voiceStream.set3DMinMaxDistance(min: 1.0f, max: 50.0f).Check();

        // plays the stream paused to allow audio buffering to steady out
        _system.playSound(_voiceStream, default, true, out _channel).Check();
        _channel.setPaused(false).Check();
    }

    public void PushAudioData(byte[] pcmData) {
        for (int i = 0; i < pcmData.Length; i++) {
            _pcmBuffer.Enqueue(pcmData[i]);
        }
    }

    public void Update(VECTOR position, VECTOR velocity) {
        _channel.set3DAttributes(ref position, ref velocity).Check();
    }

    // called by fmod
    RESULT PcmReadCallback(IntPtr soundRaw, IntPtr dataPtr, uint lengthInBytes) {
        byte[] managedBuffer = new byte[lengthInBytes];

        for (int i = 0; i < lengthInBytes; i++) {
            if (_pcmBuffer.TryDequeue(out byte sampleByte)) {
                managedBuffer[i] = sampleByte;
            }
            else {
                // in case of underflow just read silence
                managedBuffer[i] = 0;
            }
        }

        Marshal.Copy(managedBuffer, 0, dataPtr, (int)lengthInBytes);
        return RESULT.OK;
    }

    public void Dispose() {
        _channel.stop();
        _voiceStream.release();
        GC.SuppressFinalize(this);
    }
}
