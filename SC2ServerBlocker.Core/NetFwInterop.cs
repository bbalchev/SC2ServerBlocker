using System;
using System.Runtime.InteropServices;

namespace SC2ServerBlocker
{
    public static class NetFwInterop
    {
        public const int NET_FW_PROFILE2_DOMAIN = 1;
        public const int NET_FW_PROFILE2_PRIVATE = 2;
        public const int NET_FW_PROFILE2_PUBLIC = 4;
        public const int NET_FW_PROFILE2_ALL = unchecked((int)0x7FFFFFFF);

        public const int NET_FW_RULE_DIR_IN = 1;
        public const int NET_FW_RULE_DIR_OUT = 2;

        public const int NET_FW_ACTION_BLOCK = 0;
        public const int NET_FW_ACTION_ALLOW = 1;
    }
}
