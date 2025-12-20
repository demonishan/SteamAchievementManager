using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
namespace SAM.API;
public static class Steam {
  private struct Native {
    [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    internal static extern IntPtr GetProcAddress(IntPtr module, string name);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetDllDirectory(string path);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool FreeLibrary(IntPtr module);
    internal const uint LoadWithAlteredSearchPath = 8;
  }
  private static IntPtr _Handle = IntPtr.Zero;
  private static NativeCreateInterface _CallCreateInterface;
  private static NativeSteamGetCallback _CallSteamBGetCallback;
  private static NativeSteamFreeLastCallback _CallSteamFreeLastCallback;
  public static void Unload() {
    if (_Handle == IntPtr.Zero) return;
    Native.FreeLibrary(_Handle);
    _Handle = IntPtr.Zero;
    _CallCreateInterface = null;
    _CallSteamBGetCallback = null;
    _CallSteamFreeLastCallback = null;
  }
  private static Delegate GetExportDelegate<TDelegate>(IntPtr module, string name) {
    IntPtr address = Native.GetProcAddress(module, name);
    return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
  }
  private static TDelegate GetExportFunction<TDelegate>(IntPtr module, string name) where TDelegate : class =>
    (TDelegate)(object)GetExportDelegate<TDelegate>(module, name);
  public static string GetInstallPath() =>
    (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null) ??
    (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Valve\Steam", "InstallPath", null);
  [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
  private delegate IntPtr NativeCreateInterface(string version, IntPtr returnCode);
  public static TClass CreateInterface<TClass>(string version) where TClass : INativeWrapper, new() {
    IntPtr address = _CallCreateInterface(version, IntPtr.Zero);
    if (address == IntPtr.Zero) return default;
    TClass instance = new();
    instance.SetupFunctions(address);
    return instance;
  }
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.I1)]
  private delegate bool NativeSteamGetCallback(int pipe, out Types.CallbackMessage message, out int call);
  public static bool GetCallback(int pipe, out Types.CallbackMessage message, out int call) =>
    _CallSteamBGetCallback(pipe, out message, out call);
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.I1)]
  private delegate bool NativeSteamFreeLastCallback(int pipe);
  public static bool FreeLastCallback(int pipe) => _CallSteamFreeLastCallback(pipe);
  public static bool Load() {
    if (_Handle != IntPtr.Zero) return true;
    string path = GetInstallPath();
    if (path == null) return false;
    Native.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));
    path = Path.Combine(path, "steamclient.dll");
    IntPtr module = Native.LoadLibraryEx(path, IntPtr.Zero, Native.LoadWithAlteredSearchPath);
    if (module == IntPtr.Zero) return false;
    _CallCreateInterface = GetExportFunction<NativeCreateInterface>(module, "CreateInterface");
    if (_CallCreateInterface == null) return false;
    _CallSteamBGetCallback = GetExportFunction<NativeSteamGetCallback>(module, "Steam_BGetCallback");
    if (_CallSteamBGetCallback == null) return false;
    _CallSteamFreeLastCallback = GetExportFunction<NativeSteamFreeLastCallback>(module, "Steam_FreeLastCallback");
    if (_CallSteamFreeLastCallback == null) return false;
    _Handle = module;
    return true;
  }
}