using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using SAM.API.Interfaces;
namespace SAM.API {
  public interface INativeWrapper {
    void SetupFunctions(IntPtr objectAddress);
  }
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
  internal struct NativeClass {
    public IntPtr VirtualTable;
  }
  internal class NativeStrings {
    public sealed class StringHandle : SafeHandleZeroOrMinusOneIsInvalid {
      internal StringHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle) => SetHandle(preexistingHandle);
      public IntPtr Handle => handle;
      protected override bool ReleaseHandle() {
        if (handle == IntPtr.Zero) return false;
        Marshal.FreeHGlobal(handle);
        handle = IntPtr.Zero;
        return true;
      }
    }
    public static unsafe StringHandle StringToStringHandle(string value) {
      if (value == null) return new StringHandle(IntPtr.Zero, true);
      var bytes = Encoding.UTF8.GetBytes(value);
      var length = bytes.Length;
      var p = Marshal.AllocHGlobal(length + 1);
      Marshal.Copy(bytes, 0, p, bytes.Length);
      ((byte*)p)[length] = 0;
      return new StringHandle(p, true);
    }
    public static unsafe string PointerToString(sbyte* bytes) {
      if (bytes == null) return null;
      int running = 0;
      var b = bytes;
      if (*b == 0) return string.Empty;
      while ((*b++) != 0) running++;
      return new string(bytes, 0, running, Encoding.UTF8);
    }
    public static unsafe string PointerToString(byte* bytes) => PointerToString((sbyte*)bytes);
    public static unsafe string PointerToString(IntPtr nativeData) => PointerToString((sbyte*)nativeData.ToPointer());
    public static unsafe string PointerToString(sbyte* bytes, int length) {
      if (bytes == null) return null;
      int running = 0;
      var b = bytes;
      if (length == 0 || *b == 0) return string.Empty;
      while ((*b++) != 0 && running < length) running++;
      return new string(bytes, 0, running, Encoding.UTF8);
    }
    public static unsafe string PointerToString(byte* bytes, int length) => PointerToString((sbyte*)bytes, length);
    public static unsafe string PointerToString(IntPtr nativeData, int length) => PointerToString((sbyte*)nativeData.ToPointer(), length);
  }
  public abstract class NativeWrapper<TNativeFunctions> : INativeWrapper {
    protected IntPtr ObjectAddress;
    protected TNativeFunctions Functions;
    public override string ToString() => $"Steam Interface<{typeof(TNativeFunctions)}> #{ObjectAddress.ToInt32():X8}";
    public void SetupFunctions(IntPtr objectAddress) {
      ObjectAddress = objectAddress;
      var iface = (NativeClass)Marshal.PtrToStructure(ObjectAddress, typeof(NativeClass));
      Functions = (TNativeFunctions)Marshal.PtrToStructure(iface.VirtualTable, typeof(TNativeFunctions));
    }
    private readonly Dictionary<IntPtr, Delegate> _FunctionCache = new();
    protected Delegate GetDelegate<TDelegate>(IntPtr pointer) {
      if (_FunctionCache.TryGetValue(pointer, out var function) == false) {
        function = Marshal.GetDelegateForFunctionPointer(pointer, typeof(TDelegate));
        _FunctionCache[pointer] = function;
      }
      return function;
    }
    protected TDelegate GetFunction<TDelegate>(IntPtr pointer) where TDelegate : class => (TDelegate)(object)GetDelegate<TDelegate>(pointer);
    protected void Call<TDelegate>(IntPtr pointer, params object[] args) => GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
    protected TReturn Call<TReturn, TDelegate>(IntPtr pointer, params object[] args) => (TReturn)GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
  }
}
namespace SAM.API.Wrappers {
  public class SteamApps001 : NativeWrapper<ISteamApps001> {
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetAppData(IntPtr self, uint appId, IntPtr key, IntPtr value, int valueLength);
    public string GetAppData(uint appId, string key) {
      using (var nativeHandle = NativeStrings.StringToStringHandle(key)) {
        const int valueLength = 1024;
        var valuePointer = Marshal.AllocHGlobal(valueLength);
        int result = Call<int, NativeGetAppData>(Functions.GetAppData, ObjectAddress, appId, nativeHandle.Handle, valuePointer, valueLength);
        var value = result == 0 ? null : NativeStrings.PointerToString(valuePointer, valueLength);
        Marshal.FreeHGlobal(valuePointer);
        return value;
      }
    }
  }
  public class SteamApps008 : NativeWrapper<ISteamApps008> {
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedApp(IntPtr self, uint gameId);
    public bool IsSubscribedApp(uint gameId) => Call<bool, NativeIsSubscribedApp>(Functions.IsSubscribedApp, ObjectAddress, gameId);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetCurrentGameLanguage(IntPtr self);
    public string GetCurrentGameLanguage() => NativeStrings.PointerToString(Call<IntPtr, NativeGetCurrentGameLanguage>(Functions.GetCurrentGameLanguage, ObjectAddress));
  }
  public class SteamClient018 : NativeWrapper<ISteamClient018> {
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeCreateSteamPipe(IntPtr self);
    public int CreateSteamPipe() => Call<int, NativeCreateSteamPipe>(Functions.CreateSteamPipe, ObjectAddress);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeReleaseSteamPipe(IntPtr self, int pipe);
    public bool ReleaseSteamPipe(int pipe) => Call<bool, NativeReleaseSteamPipe>(Functions.ReleaseSteamPipe, ObjectAddress, pipe);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeCreateLocalUser(IntPtr self, ref int pipe, Types.AccountType type);
    public int CreateLocalUser(ref int pipe, Types.AccountType type) => GetFunction<NativeCreateLocalUser>(Functions.CreateLocalUser)(ObjectAddress, ref pipe, type);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeConnectToGlobalUser(IntPtr self, int pipe);
    public int ConnectToGlobalUser(int pipe) => Call<int, NativeConnectToGlobalUser>(Functions.ConnectToGlobalUser, ObjectAddress, pipe);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeReleaseUser(IntPtr self, int pipe, int user);
    public void ReleaseUser(int pipe, int user) => Call<NativeReleaseUser>(Functions.ReleaseUser, ObjectAddress, pipe, user);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeSetLocalIPBinding(IntPtr self, uint host, ushort port);
    public void SetLocalIPBinding(uint host, ushort port) => Call<NativeSetLocalIPBinding>(Functions.SetLocalIPBinding, ObjectAddress, host, port);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetISteamUser(IntPtr self, int user, int pipe, IntPtr version);
    private TClass GetISteamUser<TClass>(int user, int pipe, string version) where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = Call<IntPtr, NativeGetISteamUser>(Functions.GetISteamUser, ObjectAddress, user, pipe, nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    public SteamUser012 GetSteamUser012(int user, int pipe) => GetISteamUser<SteamUser012>(user, pipe, "SteamUser012");
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetISteamUserStats(IntPtr self, int user, int pipe, IntPtr version);
    private TClass GetISteamUserStats<TClass>(int user, int pipe, string version) where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = Call<IntPtr, NativeGetISteamUserStats>(Functions.GetISteamUserStats, ObjectAddress, user, pipe, nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    public SteamUserStats013 GetSteamUserStats013(int user, int pipe) => GetISteamUserStats<SteamUserStats013>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION013");
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetISteamUtils(IntPtr self, int pipe, IntPtr version);
    public TClass GetISteamUtils<TClass>(int pipe, string version) where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = Call<IntPtr, NativeGetISteamUtils>(Functions.GetISteamUtils, ObjectAddress, pipe, nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    public SteamUtils005 GetSteamUtils004(int pipe) => GetISteamUtils<SteamUtils005>(pipe, "SteamUtils005");
    private delegate IntPtr NativeGetISteamApps(int user, int pipe, IntPtr version);
    private TClass GetISteamApps<TClass>(int user, int pipe, string version) where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = Call<IntPtr, NativeGetISteamApps>(Functions.GetISteamApps, user, pipe, nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    public SteamApps001 GetSteamApps001(int user, int pipe) => GetISteamApps<SteamApps001>(user, pipe, "STEAMAPPS_INTERFACE_VERSION001");
    public SteamApps008 GetSteamApps008(int user, int pipe) => GetISteamApps<SteamApps008>(user, pipe, "STEAMAPPS_INTERFACE_VERSION008");
  }
  public class SteamUser012 : NativeWrapper<ISteamUser012> {
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeLoggedOn(IntPtr self);
    public bool IsLoggedIn() => Call<bool, NativeLoggedOn>(Functions.LoggedOn, ObjectAddress);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeGetSteamId(IntPtr self, out ulong steamId);
    public ulong GetSteamId() {
      GetFunction<NativeGetSteamId>(Functions.GetSteamID)(ObjectAddress, out ulong steamId);
      return steamId;
    }
  }
  public class SteamUserStats013 : NativeWrapper<ISteamUserStats013> {
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetStatInt(IntPtr self, IntPtr name, out int data);
    public bool GetStatValue(string name, out int value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return GetFunction<NativeGetStatInt>(Functions.GetStatInteger)(ObjectAddress, nativeName.Handle, out value);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetStatFloat(IntPtr self, IntPtr name, out float data);
    public bool GetStatValue(string name, out float value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return GetFunction<NativeGetStatFloat>(Functions.GetStatFloat)(ObjectAddress, nativeName.Handle, out value);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetStatInt(IntPtr self, IntPtr name, int data);
    public bool SetStatValue(string name, int value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return Call<bool, NativeSetStatInt>(Functions.SetStatInteger, ObjectAddress, nativeName.Handle, value);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetStatFloat(IntPtr self, IntPtr name, float data);
    public bool SetStatValue(string name, float value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return Call<bool, NativeSetStatFloat>(Functions.SetStatFloat, ObjectAddress, nativeName.Handle, value);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetAchievement(IntPtr self, IntPtr name, [MarshalAs(UnmanagedType.I1)] out bool isAchieved);
    public bool GetAchievement(string name, out bool isAchieved) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return GetFunction<NativeGetAchievement>(Functions.GetAchievement)(ObjectAddress, nativeName.Handle, out isAchieved);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetAchievement(IntPtr self, IntPtr name);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeClearAchievement(IntPtr self, IntPtr name);
    public bool SetAchievement(string name, bool state) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        if (state) return Call<bool, NativeSetAchievement>(Functions.SetAchievement, ObjectAddress, nativeName.Handle);
        return Call<bool, NativeClearAchievement>(Functions.ClearAchievement, ObjectAddress, nativeName.Handle);
      }
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetAchievementAndUnlockTime(IntPtr self, IntPtr name, [MarshalAs(UnmanagedType.I1)] out bool isAchieved, out uint unlockTime);
    public bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out uint unlockTime) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return GetFunction<NativeGetAchievementAndUnlockTime>(Functions.GetAchievementAndUnlockTime)(ObjectAddress, nativeName.Handle, out isAchieved, out unlockTime);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeStoreStats(IntPtr self);
    public bool StoreStats() => Call<bool, NativeStoreStats>(Functions.StoreStats, ObjectAddress);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetAchievementIcon(IntPtr self, IntPtr name);
    public int GetAchievementIcon(string name) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return Call<int, NativeGetAchievementIcon>(Functions.GetAchievementIcon, ObjectAddress, nativeName.Handle);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetAchievementDisplayAttribute(IntPtr self, IntPtr name, IntPtr key);
    public string GetAchievementDisplayAttribute(string name, string key) {
      using (var nativeName = NativeStrings.StringToStringHandle(name))
      using (var nativeKey = NativeStrings.StringToStringHandle(key)) {
        return NativeStrings.PointerToString(Call<IntPtr, NativeGetAchievementDisplayAttribute>(Functions.GetAchievementDisplayAttribute, ObjectAddress, nativeName.Handle, nativeKey.Handle));
      }
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate CallHandle NativeRequestUserStats(IntPtr self, ulong steamIdUser);
    public CallHandle RequestUserStats(ulong steamIdUser) => Call<CallHandle, NativeRequestUserStats>(Functions.RequestUserStats, ObjectAddress, steamIdUser);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate CallHandle NativeRequestGlobalAchievementPercentages(IntPtr self);
    public CallHandle RequestGlobalAchievementPercentages() => Call<CallHandle, NativeRequestGlobalAchievementPercentages>(Functions.RequestGlobalAchievementPercentages, ObjectAddress);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetAchievementAchievedPercent(IntPtr self, IntPtr name, out float percent);
    public bool GetAchievementAchievedPercent(string name, out float percent) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) return GetFunction<NativeGetAchievementAchievedPercent>(Functions.GetAchievementAchievedPercent)(ObjectAddress, nativeName.Handle, out percent);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeResetAllStats(IntPtr self, [MarshalAs(UnmanagedType.I1)] bool achievementsToo);
    public bool ResetAllStats(bool achievementsToo) => Call<bool, NativeResetAllStats>(Functions.ResetAllStats, ObjectAddress, achievementsToo);
  }
  public class SteamUtils005 : NativeWrapper<ISteamUtils005> {
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetConnectedUniverse(IntPtr self);
    public int GetConnectedUniverse() => Call<int, NativeGetConnectedUniverse>(Functions.GetConnectedUniverse, ObjectAddress);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetIPCountry(IntPtr self);
    public string GetIPCountry() => NativeStrings.PointerToString(Call<IntPtr, NativeGetIPCountry>(Functions.GetIPCountry, ObjectAddress));
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetImageSize(IntPtr self, int index, out int width, out int height);
    public bool GetImageSize(int index, out int width, out int height) => GetFunction<NativeGetImageSize>(Functions.GetImageSize)(ObjectAddress, index, out width, out height);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetImageRGBA(IntPtr self, int index, byte[] buffer, int length);
    public bool GetImageRGBA(int index, byte[] data) {
      if (data == null) throw new ArgumentNullException(nameof(data));
      return GetFunction<NativeGetImageRGBA>(Functions.GetImageRGBA)(ObjectAddress, index, data, data.Length);
    }
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate uint NativeGetAppId(IntPtr self);
    public uint GetAppId() => Call<uint, NativeGetAppId>(Functions.GetAppID, ObjectAddress);
  }
}
