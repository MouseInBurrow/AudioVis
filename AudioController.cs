using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace AudioMixerApp
{
    public class AudioSessionInfo
    {
        public string ProcessName { get; set; }
        public string DisplayName { get; set; }
        public uint ProcessId { get; set; }
    }

    [Guid("1C158861-B533-4B30-B1CF-E853E51C59B8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IChannelAudioVolume
    {
        [PreserveSig]
        int GetChannelCount(out uint channelCount);

        [PreserveSig]
        int SetChannelVolume(
            uint channelIndex,
            float level,
            Guid eventContext);

        [PreserveSig]
        int GetChannelVolume(
            uint channelIndex,
            out float level);

        [PreserveSig]
        int SetAllVolumes(
            uint channelCount,
            float[] levels,
            Guid eventContext);

        [PreserveSig]
        int GetAllVolumes(
            uint channelCount,
            float[] levels);
    }

    public static class AudioController
    {
        private static AudioSessionManager GetSessionManager()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                return device.AudioSessionManager;
            }
        }

        public static List<AudioSessionInfo> GetActiveSessions()
        {
            List<AudioSessionInfo> list = new List<AudioSessionInfo>();
            try
            {
                var manager = GetSessionManager();
                var sessions = manager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    if (session == null) continue;

                    uint processId = session.GetProcessID;
                    if (processId == 0) continue;

                    try
                    {
                        Process proc = Process.GetProcessById((int)processId);
                        string name = proc.ProcessName;
                        string title = string.IsNullOrEmpty(proc.MainWindowTitle) ? proc.ProcessName : proc.MainWindowTitle;

                        if (session.IsSystemSoundsSession)
                        {
                            name = "SystemSounds";
                            title = "Системные звуки Windows";
                        }

                        if (!list.Exists(x => x.ProcessName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        {
                            list.Add(new AudioSessionInfo
                            {
                                ProcessName = name,
                                DisplayName = title,
                                ProcessId = processId
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return list;
        }

        public static void SetVolume(string processName, float volume)
        {
            try
            {
                var manager = GetSessionManager();
                var sessions = manager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    if (session == null) continue;

                    uint processId = session.GetProcessID;
                    if (processId == 0) continue;

                    try
                    {
                        Process proc = Process.GetProcessById((int)processId);
                        string name = session.IsSystemSoundsSession ? "SystemSounds" : proc.ProcessName;

                        if (name.Equals(processName, StringComparison.OrdinalIgnoreCase))
                        {
                            session.SimpleAudioVolume.Volume = volume;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void SetBalance(string processName, float balance)
        {
            try
            {
                var manager = GetSessionManager();
                var sessions = manager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    if (session == null) continue;

                    uint processId = session.GetProcessID;
                    if (processId == 0) continue;

                    try
                    {
                        Process proc = Process.GetProcessById((int)processId);
                        string name = session.IsSystemSoundsSession
                            ? "SystemSounds"
                            : proc.ProcessName;

                        if (!name.Equals(processName, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var field = session.GetType().GetField(
                            "audioSessionControlInterface",
                            BindingFlags.NonPublic | BindingFlags.Instance);

                        if (field == null)
                            return;

                        object audioControl = field.GetValue(session);
                        if (audioControl == null)
                            return;

                        IntPtr unk = Marshal.GetIUnknownForObject(audioControl);

                        try
                        {
                            Guid iid = typeof(IChannelAudioVolume).GUID;

                            if (Marshal.QueryInterface(unk, ref iid, out IntPtr ptr) != 0)
                                return;

                            try
                            {
                                var channelVolume =
                                    (IChannelAudioVolume)Marshal.GetObjectForIUnknown(ptr);

                                if (channelVolume.GetChannelCount(out uint channels) != 0 ||
                                    channels < 2)
                                    return;

                                float left, right;

                                if (balance < 0)
                                {
                                    left = 1.0f;
                                    right = 1.0f + balance;
                                }
                                else
                                {
                                    left = 1.0f - balance;
                                    right = 1.0f;
                                }

                                Guid context = Guid.Empty;

                                channelVolume.SetChannelVolume(0, left, context);
                                channelVolume.SetChannelVolume(1, right, context);
                            }
                            finally
                            {
                                Marshal.Release(ptr);
                            }
                        }
                        finally
                        {
                            Marshal.Release(unk);
                        }

                        break;
                    }
                    catch{}
                }
            }
            catch{}
        }
    }
}
