using System;

namespace FlutterSync
{
    [Flags]
    internal enum FlutterModuleBuildConfig
    {
        Default = 0x00,
        Debug   = 0x01,
        Profile = 0x02,
        Release = 0x04
    }
}