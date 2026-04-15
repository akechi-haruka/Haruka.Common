using System.Runtime.InteropServices;

namespace Haruka.Common;

public static class NativeMethods {
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true)]
    public static extern int GetPrivateProfileString(string section, string key, string @default, [Out] byte[] retVal, int size, string filePath);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true)]
    public static extern int GetPrivateProfileSectionNames([Out] byte[] lpszReturnBuffer, int nSize, string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true)]
    public static extern int GetPrivateProfileSection(string lpAppName, [Out] byte[] lpszReturnBuffer, int nSize, string lpFileName);
}