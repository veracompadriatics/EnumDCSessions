using System;
using System.Runtime.InteropServices;
namespace Interop.NetAPI32
{
    public class NetAPI
    {
        public const int MAX_PREFERRED_LENGTH = -1;

        public enum NET_API_STATUS : uint
        {
            NERR_Success = 0,

            /// <summary>
            /// This computer name is invalid.
            /// </summary>
            NERR_InvalidComputer = 2351,

            /// <summary>
            /// This operation is only allowed on the primary domain controller of the domain.
            /// </summary>
            NERR_NotPrimary = 2226,

            /// <summary>
            /// This operation is not allowed on this special group.
            /// </summary>
            NERR_SpeGroupOp = 2234,

            /// <summary>
            /// This operation is not allowed on the last administrative account.
            /// </summary>
            NERR_LastAdmin = 2452,

            /// <summary>
            /// The password parameter is invalid.
            /// </summary>
            NERR_BadPassword = 2203,

            /// <summary>
            /// The password does not meet the password policy requirements. Check the minimum password length, password complexity and password history requirements.
            /// </summary>
            NERR_PasswordTooShort = 2245,

            /// <summary>
            /// The user name could not be found.
            /// </summary>
            NERR_UserNotFound = 2221,

            ERROR_ACCESS_DENIED = 5,
            ERROR_NOT_ENOUGH_MEMORY = 8,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_NAME = 123,
            ERROR_INVALID_LEVEL = 124,
            ERROR_MORE_DATA = 234,
            ERROR_SESSION_CREDENTIAL_CONFLICT = 1219
        }

        [DllImport("Netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern NET_API_STATUS NetFileEnum
                                                (
                                                        string servername,
                                                        string basepath,
                                                        string username,
                                                        int level,
                                                        ref IntPtr bufptr,
                                                        int prefmaxlen,
                                                        out int entriesread,
                                                        out int totalentries,
                                                        IntPtr resume_handle
                                                );

        [DllImport("Netapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int NetFileClose(string servername, int id);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct FILE_INFO_3
        {
            public int fi3_id;
            public int fi3_permission;
            public int fi3_num_locks;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_pathname;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string fi3_username;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SESSION_INFO_10
        {
            public static readonly int SIZE_OF = Marshal.SizeOf(typeof(SESSION_INFO_10));
            public string sesi10_cname;
            public string sesi10_username;
            public uint sesi10_time;
            public uint sesi10_idle_time;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SESSION_INFO_502
        {
            /// <summary>
            /// Unicode string specifying the name of the computer that established the session.
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string si502_cname;

            /// <summary>
            /// <value>Unicode string specifying the name of the user who established the session.</value>
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string si502_username;

            /// <summary>
            /// <value>Specifies the number of files, devices, and pipes opened during the session.</value>
            /// </summary>
            public uint si502_num_opens;

            /// <summary>
            /// <value>Specifies the number of seconds the session has been active. </value>
            /// </summary>
            public uint si502_time;

            /// <summary>
            /// <value>Specifies the number of seconds the session has been idle.</value>
            /// </summary>
            public uint si502_idle_time;

            /// <summary>
            /// <value>Specifies a value that describes how the user established the session.</value>
            /// </summary>
            public uint si502_user_flags;

            /// <summary>
            /// <value>Unicode string that specifies the type of client that established the session.</value>
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string si502_cltype_name;

            /// <summary>
            /// <value>Specifies the name of the transport that the client is using to communicate with the server.</value>
            /// </summary>
            [MarshalAs(UnmanagedType.LPWStr)]
            public string si502_transport;
        }

        [DllImport("netapi32.dll", SetLastError = true)]
        public static extern int NetSessionEnum
                                        (
                                                [In,MarshalAs(UnmanagedType.LPWStr)]
                                                        string ServerName,
                                                [In,MarshalAs(UnmanagedType.LPWStr)]
                                                        string UncClientName,
                                                [In,MarshalAs(UnmanagedType.LPWStr)]
                                                        string UserName,
                                                Int32 Level,
                                                out IntPtr bufptr,
                                                int prefmaxlen,
                                                ref Int32 entriesread,
                                                ref Int32 totalentries,
                                                ref Int32 resume_handle
                                        );

        [DllImport("Netapi32.dll", SetLastError = true)]
        public static extern int NetApiBufferFree(IntPtr Buffer);

        [DllImport("kernel32.dll", EntryPoint = "GetLastError", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern uint GetLastError();       //Recommended use:Marshal.GetLastWin32Error !!

        [DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern uint FormatMessage
                                        (
                                                uint dwFlags,
                                                IntPtr lpSource,
                                                uint dwMessageId,
                                                uint dwLanguageId,
                                                ref string lpBuffer,
                                                uint nSize,
                                                IntPtr Arguments
                                        );

        public static string GetErrorMessage(uint errorCode)
        {
            /*
            Purpose:        GetErrorMessage formats and returns an error message corresponding to the input errorCode.
            See:            http://pinvoke.net/default.aspx/kernel32.FormatMessage
            */

            uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
            uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
            uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
            //uint FORMAT_MESSAGE_ARGUMENT_ARRAY    = ????

            uint messageSize = 255;
            string lpMsgBuf = "";
            uint dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;

            IntPtr ptrlpSource = IntPtr.Zero;
            IntPtr prtArguments = IntPtr.Zero;

            uint retVal = FormatMessage(dwFlags, ptrlpSource, errorCode, 0, ref lpMsgBuf, messageSize, prtArguments);
            if (retVal == 0)
            {
                throw new Exception("Failed to format message for error code " + errorCode + ".");
            }

            return lpMsgBuf;
        }
    }
}