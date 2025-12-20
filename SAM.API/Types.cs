using System;
using System.Runtime.InteropServices;
namespace SAM.API.Types;
public enum AccountType : int { Invalid = 0, Individual = 1, Multiset = 2, GameServer = 3, AnonGameServer = 4, Pending = 5, ContentServer = 6, Clan = 7, Chat = 8, P2PSuperSeeder = 9, }
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AppDataChanged { public uint Id; public bool Result; }
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CallbackMessage { public int User; public int Id; public IntPtr ParamPointer; public int ParamSize; }
public enum ItemRequestResult : int { InvalidValue = -1, OK = 0, Denied = 1, ServerError = 2, Timeout = 3, Invalid = 4, NoMatch = 5, UnknownError = 6, NotLoggedOn = 7, }
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UserItemsReceived { public ulong GameId; public int Unknown; public int ItemCount; }
public enum UserStatType { Invalid = 0, Integer = 1, Float = 2, AverageRate = 3, Achievements = 4, GroupAchievements = 5, }
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UserStatsReceived { public ulong GameId; public int Result; public ulong SteamIdUser; }
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UserStatsStored { public ulong GameId; public int Result; }