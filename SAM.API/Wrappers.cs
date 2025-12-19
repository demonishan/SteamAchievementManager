/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */
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
      internal StringHandle(IntPtr preexistingHandle, bool ownsHandle)
          : base(ownsHandle) {
        this.SetHandle(preexistingHandle);
      }
      public IntPtr Handle {
        get { return this.handle; }
      }
      protected override bool ReleaseHandle() {
        if (handle != IntPtr.Zero) {
          Marshal.FreeHGlobal(handle);
          handle = IntPtr.Zero;
          return true;
        }
        return false;
      }
    }
    public static unsafe StringHandle StringToStringHandle(string value) {
      if (value == null) {
        return new StringHandle(IntPtr.Zero, true);
      }
      var bytes = Encoding.UTF8.GetBytes(value);
      var length = bytes.Length;
      var p = Marshal.AllocHGlobal(length + 1);
      Marshal.Copy(bytes, 0, p, bytes.Length);
      ((byte*)p)[length] = 0;
      return new StringHandle(p, true);
    }
    public static unsafe string PointerToString(sbyte* bytes) {
      if (bytes == null) {
        return null;
      }
      int running = 0;
      var b = bytes;
      if (*b == 0) {
        return string.Empty;
      }
      while ((*b++) != 0) {
        running++;
      }
      return new string(bytes, 0, running, Encoding.UTF8);
    }
    public static unsafe string PointerToString(byte* bytes) {
      return PointerToString((sbyte*)bytes);
    }
    public static unsafe string PointerToString(IntPtr nativeData) {
      return PointerToString((sbyte*)nativeData.ToPointer());
    }
    public static unsafe string PointerToString(sbyte* bytes, int length) {
      if (bytes == null) {
        return null;
      }
      int running = 0;
      var b = bytes;
      if (length == 0 || *b == 0) {
        return string.Empty;
      }
      while ((*b++) != 0 && running < length) {
        running++;
      }
      return new string(bytes, 0, running, Encoding.UTF8);
    }
    public static unsafe string PointerToString(byte* bytes, int length) {
      return PointerToString((sbyte*)bytes, length);
    }
    public static unsafe string PointerToString(IntPtr nativeData, int length) {
      return PointerToString((sbyte*)nativeData.ToPointer(), length);
    }
  }
  public abstract class NativeWrapper<TNativeFunctions> : INativeWrapper {
    protected IntPtr ObjectAddress;
    protected TNativeFunctions Functions;
    public override string ToString() {
      return $"Steam Interface<{typeof(TNativeFunctions)}> #{this.ObjectAddress.ToInt32():X8}";
    }
    public void SetupFunctions(IntPtr objectAddress) {
      this.ObjectAddress = objectAddress;
      var iface = (NativeClass)
          Marshal.PtrToStructure(this.ObjectAddress, typeof(NativeClass));
      this.Functions = (TNativeFunctions)
          Marshal.PtrToStructure(iface.VirtualTable, typeof(TNativeFunctions));
    }
    private readonly Dictionary<IntPtr, Delegate> _FunctionCache = new();
    protected Delegate GetDelegate<TDelegate>(IntPtr pointer) {
      if (this._FunctionCache.TryGetValue(pointer, out var function) == false) {
        function = Marshal.GetDelegateForFunctionPointer(pointer, typeof(TDelegate));
        this._FunctionCache[pointer] = function;
      }
      return function;
    }
    protected TDelegate GetFunction<TDelegate>(IntPtr pointer)
        where TDelegate : class {
      return (TDelegate)((object)this.GetDelegate<TDelegate>(pointer));
    }
    protected void Call<TDelegate>(IntPtr pointer, params object[] args) {
      this.GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
    }
    protected TReturn Call<TReturn, TDelegate>(IntPtr pointer, params object[] args) {
      return (TReturn)this.GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
    }
  }
}
namespace SAM.API.Wrappers {
  public class SteamApps001 : NativeWrapper<ISteamApps001> {
    #region GetAppData
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetAppData(
        IntPtr self,
        uint appId,
        IntPtr key,
        IntPtr value,
        int valueLength);
    public string GetAppData(uint appId, string key) {
      using (var nativeHandle = NativeStrings.StringToStringHandle(key)) {
        const int valueLength = 1024;
        var valuePointer = Marshal.AllocHGlobal(valueLength);
        int result = this.Call<int, NativeGetAppData>(
            this.Functions.GetAppData,
            this.ObjectAddress,
            appId,
            nativeHandle.Handle,
            valuePointer,
            valueLength);
        var value = result == 0 ? null : NativeStrings.PointerToString(valuePointer, valueLength);
        Marshal.FreeHGlobal(valuePointer);
        return value;
      }
    }
    #endregion
  }
  public class SteamApps008 : NativeWrapper<ISteamApps008> {
    #region IsSubscribed
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeIsSubscribedApp(IntPtr self, uint gameId);
    public bool IsSubscribedApp(uint gameId) {
      return this.Call<bool, NativeIsSubscribedApp>(this.Functions.IsSubscribedApp, this.ObjectAddress, gameId);
    }
    #endregion
    #region GetCurrentGameLanguage
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetCurrentGameLanguage(IntPtr self);
    public string GetCurrentGameLanguage() {
      var languagePointer = this.Call<IntPtr, NativeGetCurrentGameLanguage>(
          this.Functions.GetCurrentGameLanguage,
          this.ObjectAddress);
      return NativeStrings.PointerToString(languagePointer);
    }
    #endregion
  }
  public class SteamClient018 : NativeWrapper<ISteamClient018> {
    #region CreateSteamPipe
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeCreateSteamPipe(IntPtr self);
    public int CreateSteamPipe() {
      return this.Call<int, NativeCreateSteamPipe>(this.Functions.CreateSteamPipe, this.ObjectAddress);
    }
    #endregion
    #region ReleaseSteamPipe
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeReleaseSteamPipe(IntPtr self, int pipe);
    public bool ReleaseSteamPipe(int pipe) {
      return this.Call<bool, NativeReleaseSteamPipe>(this.Functions.ReleaseSteamPipe, this.ObjectAddress, pipe);
    }
    #endregion
    #region CreateLocalUser
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeCreateLocalUser(IntPtr self, ref int pipe, Types.AccountType type);
    public int CreateLocalUser(ref int pipe, Types.AccountType type) {
      var call = this.GetFunction<NativeCreateLocalUser>(this.Functions.CreateLocalUser);
      return call(this.ObjectAddress, ref pipe, type);
    }
    #endregion
    #region ConnectToGlobalUser
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeConnectToGlobalUser(IntPtr self, int pipe);
    public int ConnectToGlobalUser(int pipe) {
      return this.Call<int, NativeConnectToGlobalUser>(
          this.Functions.ConnectToGlobalUser,
          this.ObjectAddress,
          pipe);
    }
    #endregion
    #region ReleaseUser
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeReleaseUser(IntPtr self, int pipe, int user);
    public void ReleaseUser(int pipe, int user) {
      this.Call<NativeReleaseUser>(this.Functions.ReleaseUser, this.ObjectAddress, pipe, user);
    }
    #endregion
    #region SetLocalIPBinding
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeSetLocalIPBinding(IntPtr self, uint host, ushort port);
    public void SetLocalIPBinding(uint host, ushort port) {
      this.Call<NativeSetLocalIPBinding>(this.Functions.SetLocalIPBinding, this.ObjectAddress, host, port);
    }
    #endregion
    #region GetISteamUser
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetISteamUser(IntPtr self, int user, int pipe, IntPtr version);
    private TClass GetISteamUser<TClass>(int user, int pipe, string version)
        where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = this.Call<IntPtr, NativeGetISteamUser>(
            this.Functions.GetISteamUser,
            this.ObjectAddress,
            user,
            pipe,
            nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    #endregion
    #region GetSteamUser012
    public SteamUser012 GetSteamUser012(int user, int pipe) {
      return this.GetISteamUser<SteamUser012>(user, pipe, "SteamUser012");
    }
    #endregion
    #region GetISteamUserStats
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetISteamUserStats(IntPtr self, int user, int pipe, IntPtr version);
    private TClass GetISteamUserStats<TClass>(int user, int pipe, string version)
        where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = this.Call<IntPtr, NativeGetISteamUserStats>(
            this.Functions.GetISteamUserStats,
            this.ObjectAddress,
            user,
            pipe,
            nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    #endregion
    #region GetSteamUserStats013
    public SteamUserStats013 GetSteamUserStats013(int user, int pipe) {
      return this.GetISteamUserStats<SteamUserStats013>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION013");
    }
    #endregion
    #region GetISteamUtils
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetISteamUtils(IntPtr self, int pipe, IntPtr version);
    public TClass GetISteamUtils<TClass>(int pipe, string version)
        where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = this.Call<IntPtr, NativeGetISteamUtils>(
            this.Functions.GetISteamUtils,
            this.ObjectAddress,
            pipe,
            nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    #endregion
    #region GetSteamUtils004
    public SteamUtils005 GetSteamUtils004(int pipe) {
      return this.GetISteamUtils<SteamUtils005>(pipe, "SteamUtils005");
    }
    #endregion
    #region GetISteamApps
    private delegate IntPtr NativeGetISteamApps(int user, int pipe, IntPtr version);
    private TClass GetISteamApps<TClass>(int user, int pipe, string version)
        where TClass : INativeWrapper, new() {
      using (var nativeVersion = NativeStrings.StringToStringHandle(version)) {
        IntPtr address = this.Call<IntPtr, NativeGetISteamApps>(
            this.Functions.GetISteamApps,
            user,
            pipe,
            nativeVersion.Handle);
        TClass result = new();
        result.SetupFunctions(address);
        return result;
      }
    }
    #endregion
    #region GetSteamApps001
    public SteamApps001 GetSteamApps001(int user, int pipe) {
      return this.GetISteamApps<SteamApps001>(user, pipe, "STEAMAPPS_INTERFACE_VERSION001");
    }
    #endregion
    #region GetSteamApps008
    public SteamApps008 GetSteamApps008(int user, int pipe) {
      return this.GetISteamApps<SteamApps008>(user, pipe, "STEAMAPPS_INTERFACE_VERSION008");
    }
    #endregion
  }
  public class SteamUser012 : NativeWrapper<ISteamUser012> {
    #region IsLoggedIn
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeLoggedOn(IntPtr self);
    public bool IsLoggedIn() {
      return this.Call<bool, NativeLoggedOn>(this.Functions.LoggedOn, this.ObjectAddress);
    }
    #endregion
    #region GetSteamID
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate void NativeGetSteamId(IntPtr self, out ulong steamId);
    public ulong GetSteamId() {
      var call = this.GetFunction<NativeGetSteamId>(this.Functions.GetSteamID);
      ulong steamId;
      call(this.ObjectAddress, out steamId);
      return steamId;
    }
    #endregion
  }
  public class SteamUserStats013 : NativeWrapper<ISteamUserStats013> {
    #region GetStatValue (int)
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetStatInt(IntPtr self, IntPtr name, out int data);
    public bool GetStatValue(string name, out int value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        var call = this.GetFunction<NativeGetStatInt>(this.Functions.GetStatInteger);
        return call(this.ObjectAddress, nativeName.Handle, out value);
      }
    }
    #endregion
    #region GetStatValue (float)
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetStatFloat(IntPtr self, IntPtr name, out float data);
    public bool GetStatValue(string name, out float value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        var call = this.GetFunction<NativeGetStatFloat>(this.Functions.GetStatFloat);
        return call(this.ObjectAddress, nativeName.Handle, out value);
      }
    }
    #endregion
    #region SetStatValue (int)
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetStatInt(IntPtr self, IntPtr name, int data);
    public bool SetStatValue(string name, int value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        return this.Call<bool, NativeSetStatInt>(
            this.Functions.SetStatInteger,
            this.ObjectAddress,
            nativeName.Handle,
            value);
      }
    }
    #endregion
    #region SetStatValue (float)
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetStatFloat(IntPtr self, IntPtr name, float data);
    public bool SetStatValue(string name, float value) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        return this.Call<bool, NativeSetStatFloat>(
            this.Functions.SetStatFloat,
            this.ObjectAddress,
            nativeName.Handle,
            value);
      }
    }
    #endregion
    #region GetAchievement
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetAchievement(
        IntPtr self,
        IntPtr name,
        [MarshalAs(UnmanagedType.I1)] out bool isAchieved);
    public bool GetAchievement(string name, out bool isAchieved) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        var call = this.GetFunction<NativeGetAchievement>(this.Functions.GetAchievement);
        return call(this.ObjectAddress, nativeName.Handle, out isAchieved);
      }
    }
    #endregion
    #region SetAchievement
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeSetAchievement(IntPtr self, IntPtr name);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeClearAchievement(IntPtr self, IntPtr name);
    public bool SetAchievement(string name, bool state) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        if (state == false) {
          return this.Call<bool, NativeClearAchievement>(
              this.Functions.ClearAchievement,
              this.ObjectAddress,
              nativeName.Handle);
        }
        return this.Call<bool, NativeSetAchievement>(
            this.Functions.SetAchievement,
            this.ObjectAddress,
            nativeName.Handle);
      }
    }
    #endregion
    #region GetAchievementAndUnlockTime
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetAchievementAndUnlockTime(
        IntPtr self,
        IntPtr name,
        [MarshalAs(UnmanagedType.I1)] out bool isAchieved,
        out uint unlockTime);
    public bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out uint unlockTime) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        var call = this.GetFunction<NativeGetAchievementAndUnlockTime>(this.Functions.GetAchievementAndUnlockTime);
        return call(this.ObjectAddress, nativeName.Handle, out isAchieved, out unlockTime);
      }
    }
    #endregion
    #region StoreStats
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeStoreStats(IntPtr self);
    public bool StoreStats() {
      return this.Call<bool, NativeStoreStats>(this.Functions.StoreStats, this.ObjectAddress);
    }
    #endregion
    #region GetAchievementIcon
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetAchievementIcon(IntPtr self, IntPtr name);
    public int GetAchievementIcon(string name) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        return this.Call<int, NativeGetAchievementIcon>(
            this.Functions.GetAchievementIcon,
            this.ObjectAddress,
            nativeName.Handle);
      }
    }
    #endregion
    #region GetAchievementDisplayAttribute
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetAchievementDisplayAttribute(IntPtr self, IntPtr name, IntPtr key);
    public string GetAchievementDisplayAttribute(string name, string key) {
      using (var nativeName = NativeStrings.StringToStringHandle(name))
      using (var nativeKey = NativeStrings.StringToStringHandle(key)) {
        var result = this.Call<IntPtr, NativeGetAchievementDisplayAttribute>(
            this.Functions.GetAchievementDisplayAttribute,
            this.ObjectAddress,
            nativeName.Handle,
            nativeKey.Handle);
        return NativeStrings.PointerToString(result);
      }
    }
    #endregion
    #region RequestUserStats
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate CallHandle NativeRequestUserStats(IntPtr self, ulong steamIdUser);
    public CallHandle RequestUserStats(ulong steamIdUser) {
      return this.Call<CallHandle, NativeRequestUserStats>(this.Functions.RequestUserStats, this.ObjectAddress, steamIdUser);
    }
    #endregion
    #region RequestGlobalAchievementPercentages
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate CallHandle NativeRequestGlobalAchievementPercentages(IntPtr self);
    public CallHandle RequestGlobalAchievementPercentages() {
      return this.Call<CallHandle, NativeRequestGlobalAchievementPercentages>(this.Functions.RequestGlobalAchievementPercentages, this.ObjectAddress);
    }
    #endregion
    #region GetAchievementAchievedPercent
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetAchievementAchievedPercent(IntPtr self, IntPtr name, out float percent);
    public bool GetAchievementAchievedPercent(string name, out float percent) {
      using (var nativeName = NativeStrings.StringToStringHandle(name)) {
        var call = this.GetFunction<NativeGetAchievementAchievedPercent>(this.Functions.GetAchievementAchievedPercent);
        return call(this.ObjectAddress, nativeName.Handle, out percent);
      }
    }
    #endregion
    #region ResetAllStats
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeResetAllStats(IntPtr self, [MarshalAs(UnmanagedType.I1)] bool achievementsToo);
    public bool ResetAllStats(bool achievementsToo) {
      return this.Call<bool, NativeResetAllStats>(
          this.Functions.ResetAllStats,
          this.ObjectAddress,
          achievementsToo);
    }
    #endregion
  }
  public class SteamUtils005 : NativeWrapper<ISteamUtils005> {
    #region GetConnectedUniverse
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int NativeGetConnectedUniverse(IntPtr self);
    public int GetConnectedUniverse() {
      return this.Call<int, NativeGetConnectedUniverse>(this.Functions.GetConnectedUniverse, this.ObjectAddress);
    }
    #endregion
    #region GetIPCountry
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate IntPtr NativeGetIPCountry(IntPtr self);
    public string GetIPCountry() {
      var result = this.Call<IntPtr, NativeGetIPCountry>(this.Functions.GetIPCountry, this.ObjectAddress);
      return NativeStrings.PointerToString(result);
    }
    #endregion
    #region GetImageSize
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetImageSize(IntPtr self, int index, out int width, out int height);
    public bool GetImageSize(int index, out int width, out int height) {
      var call = this.GetFunction<NativeGetImageSize>(this.Functions.GetImageSize);
      return call(this.ObjectAddress, index, out width, out height);
    }
    #endregion
    #region GetImageRGBA
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool NativeGetImageRGBA(IntPtr self, int index, byte[] buffer, int length);
    public bool GetImageRGBA(int index, byte[] data) {
      if (data == null) {
        throw new ArgumentNullException("data");
      }
      var call = this.GetFunction<NativeGetImageRGBA>(this.Functions.GetImageRGBA);
      return call(this.ObjectAddress, index, data, data.Length);
    }
    #endregion
    #region GetAppID
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate uint NativeGetAppId(IntPtr self);
    public uint GetAppId() {
      return this.Call<uint, NativeGetAppId>(this.Functions.GetAppID, this.ObjectAddress);
    }
    #endregion
  }
}
