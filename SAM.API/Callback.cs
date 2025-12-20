using System;
using System.Runtime.InteropServices;
namespace SAM.API {
  public enum CallHandle : ulong {
    Invalid = 0,
  }
  public interface ICallback {
    int Id { get; }
    bool IsServer { get; }
    void Run(IntPtr param);
  }
  public abstract class Callback : ICallback {
    public delegate void CallbackFunction(IntPtr param);
    public event CallbackFunction OnRun;
    public abstract int Id { get; }
    public abstract bool IsServer { get; }
    public void Run(IntPtr param) {
      OnRun?.Invoke(param);
    }
  }
  public abstract class Callback<TParameter> : ICallback where TParameter : struct {
    public delegate void CallbackFunction(TParameter arg);
    public event CallbackFunction OnRun;
    public abstract int Id { get; }
    public abstract bool IsServer { get; }
    public void Run(IntPtr pvParam) {
      var data = Marshal.PtrToStructure<TParameter>(pvParam);
      OnRun?.Invoke(data);
    }
  }
  public class AppDataChanged : Callback<Types.AppDataChanged> {
    public override int Id => 1001;
    public override bool IsServer => false;
  }
  public class UserStatsReceived : Callback<Types.UserStatsReceived> {
    public override int Id => 1101;
    public override bool IsServer => false;
  }
}