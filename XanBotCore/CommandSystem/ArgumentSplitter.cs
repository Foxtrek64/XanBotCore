using System;
using System.Runtime.InteropServices;

namespace XanBotCore.CommandSystem
{
    public class ArgumentSplitter
    {

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        /// <summary>
        /// Takes in a raw string and converts it to an array of arguments via Shell32.DLL
        /// </summary>
        /// <param name="commandLine">The raw string</param>
        /// <returns>An array of arguments</returns>
        public static string[] SplitArgs(string commandLine)
        {
            IntPtr argPtr = CommandLineToArgvW(commandLine, out int argc);
            if (argPtr == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argPtr, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argPtr);
            }
        }

    }
}
