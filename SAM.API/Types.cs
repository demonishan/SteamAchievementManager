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
using System.Runtime.InteropServices;
namespace SAM.API.Types {
  public enum AccountType : int {
    Invalid = 0,
    Individual = 1,
    Multiset = 2,
    GameServer = 3,
    AnonGameServer = 4,
    Pending = 5,
    ContentServer = 6,
    Clan = 7,
    Chat = 8,
    P2PSuperSeeder = 9,
  }
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct AppDataChanged {
    public uint Id;
    public bool Result;
  }
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct CallbackMessage {
    public int User;
    public int Id;
    public IntPtr ParamPointer;
    public int ParamSize;
  }
  public enum ItemRequestResult : int {
    InvalidValue = -1,
    OK = 0,
    Denied = 1,
    ServerError = 2,
    Timeout = 3,
    Invalid = 4,
    NoMatch = 5,
    UnknownError = 6,
    NotLoggedOn = 7,
  }
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct UserItemsReceived {
    public ulong GameId;
    public int Unknown;
    public int ItemCount;
  }
  public enum UserStatType {
    Invalid = 0,
    Integer = 1,
    Float = 2,
    AverageRate = 3,
    Achievements = 4,
    GroupAchievements = 5,
  }
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct UserStatsReceived {
    public ulong GameId;
    public int Result;
    public ulong SteamIdUser;
  }
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct UserStatsStored {
    public ulong GameId;
    public int Result;
  }
}
