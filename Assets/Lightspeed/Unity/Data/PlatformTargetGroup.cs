using System;
using System.ComponentModel;
using UnityEngine;

namespace Rhinox.Lightspeed
{
    public enum PlatformTargetGroup
    {
        /// <summary>
        ///   <para>Unknown target.</para>
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///   <para>PC, Mac &amp; Linux standalone target.</para>
        /// </summary>
        Standalone = 1,

        /// <summary>
        ///   <para>Mac/PC webplayer target.</para>
        /// </summary>
        [Obsolete("WebPlayer was removed in 5.4, consider using WebGL", true),
         EditorBrowsable(EditorBrowsableState.Never)]
        WebPlayer = 2,

        /// <summary>
        ///   <para>Apple iOS target.</para>
        /// </summary>
        iOS = 4,

        /// <summary>
        ///   <para>OBSOLETE: Use iOS. Apple iOS target.</para>
        /// </summary>
        [Obsolete("Use iOS instead (UnityUpgradable) -> iOS", true)]
        iPhone = 4,

        [Obsolete("PS3 has been removed in >=5.5")]
        PS3 = 5,

        [Obsolete("XBOX360 has been removed in 5.5")]
        XBOX360 = 6,

        /// <summary>
        ///   <para>Android target.</para>
        /// </summary>
        Android = 7,

        /// <summary>
        ///   <para>WebGL.</para>
        /// </summary>
        WebGL = 13, // 0x0000000D
        [Obsolete("Use WSA instead")] Metro = 14, // 0x0000000E

        /// <summary>
        ///   <para>Windows Store Apps target.</para>
        /// </summary>
        WSA = 14, // 0x0000000E
        [Obsolete("Use WSA instead")] WP8 = 15, // 0x0000000F

        [Obsolete("BlackBerry has been removed as of 5.4")]
        BlackBerry = 16, // 0x00000010

        [Obsolete("Tizen has been removed in 2017.3")]
        Tizen = 17, // 0x00000011

        [Obsolete("PSP2 is no longer supported as of Unity 2018.3")]
        PSP2 = 18, // 0x00000012

        /// <summary>
        ///   <para>Sony Playstation 4 target.</para>
        /// </summary>
        PS4 = 19, // 0x00000013

        [Obsolete("PSM has been removed in >= 5.3")]
        PSM = 20, // 0x00000014

        /// <summary>
        ///   <para>Microsoft Xbox One target.</para>
        /// </summary>
        XboxOne = 21, // 0x00000015

        [Obsolete("SamsungTV has been removed as of 2017.3")]
        SamsungTV = 22, // 0x00000016

        /// <summary>
        ///   <para>Nintendo 3DS target.</para>
        /// </summary>
        [Obsolete("Nintendo 3DS support is unavailable since 2018.1")]
        N3DS = 23, // 0x00000017

        [Obsolete("Wii U support was removed in 2018.1")]
        WiiU = 24, // 0x00000018

        /// <summary>
        ///   <para>Apple's tvOS target.</para>
        /// </summary>
        tvOS = 25, // 0x00000019

        [Obsolete("Facebook support was removed in 2019.3")]
        Facebook = 26, // 0x0000001A

        /// <summary>
        ///   <para>Nintendo Switch target.</para>
        /// </summary>
        Switch = 27, // 0x0000001B
        Lumin = 28, // 0x0000001C

        /// <summary>
        ///   <para>Google Stadia target.</para>
        /// </summary>
        Stadia = 29, // 0x0000001D
    }
    
    public static class PlatformTargetGroupHelper
    {
        public static bool IsCompatibleWith(this PlatformTargetGroup platformTarget, RuntimePlatform runtimePlatform)
        {
            if (platformTarget == PlatformTargetGroup.Unknown)
                return true;

            switch (platformTarget)
            {
                case PlatformTargetGroup.Android:
                    return runtimePlatform == RuntimePlatform.Android;
                case PlatformTargetGroup.Standalone:
                    return runtimePlatform == RuntimePlatform.LinuxPlayer ||
                           runtimePlatform == RuntimePlatform.WindowsPlayer ||
                           runtimePlatform == RuntimePlatform.OSXPlayer || 
                           runtimePlatform == RuntimePlatform.LinuxEditor || 
                           runtimePlatform == RuntimePlatform.WindowsEditor || 
                           runtimePlatform == RuntimePlatform.OSXEditor;
                case PlatformTargetGroup.WebGL:
                    return runtimePlatform == RuntimePlatform.WebGLPlayer;
                case PlatformTargetGroup.WSA:
                    return runtimePlatform == RuntimePlatform.WSAPlayerX64 ||
                           runtimePlatform == RuntimePlatform.WSAPlayerX86 ||
                           runtimePlatform == RuntimePlatform.WSAPlayerARM;
                case PlatformTargetGroup.iOS:
                    return runtimePlatform == RuntimePlatform.IPhonePlayer;
                case PlatformTargetGroup.PS4:
                    return runtimePlatform == RuntimePlatform.PS4;
            }

            return true;
        }
    }
}