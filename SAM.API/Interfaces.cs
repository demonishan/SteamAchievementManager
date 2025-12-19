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
namespace SAM.API.Interfaces {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ISteamApps001 {
        public IntPtr GetAppData;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ISteamApps008 {
        public IntPtr IsSubscribed;
        public IntPtr IsLowViolence;
        public IntPtr IsCybercafe;
        public IntPtr IsVACBanned;
        public IntPtr GetCurrentGameLanguage;
        public IntPtr GetAvailableGameLanguages;
        public IntPtr IsSubscribedApp;
        public IntPtr IsDlcInstalled;
        public IntPtr GetEarliestPurchaseUnixTime;
        public IntPtr IsSubscribedFromFreeWeekend;
        public IntPtr GetDLCCount;
        public IntPtr GetDLCDataByIndex;
        public IntPtr InstallDLC;
        public IntPtr UninstallDLC;
        public IntPtr RequestAppProofOfPurchaseKey;
        public IntPtr GetCurrentBetaName;
        public IntPtr MarkContentCorrupt;
        public IntPtr GetInstalledDepots;
        public IntPtr GetAppInstallDir;
        public IntPtr IsAppInstalled;
        public IntPtr GetAppOwner;
        public IntPtr GetLaunchQueryParam;
        public IntPtr GetDlcDownloadProgress;
        public IntPtr GetAppBuildId;
        public IntPtr RequestAllProofOfPurchaseKeys;
        public IntPtr GetFileDetails;
        public IntPtr GetLaunchCommandLine;
        public IntPtr IsSubscribedFromFamilySharing;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ISteamClient018 {
        public IntPtr CreateSteamPipe;
        public IntPtr ReleaseSteamPipe;
        public IntPtr ConnectToGlobalUser;
        public IntPtr CreateLocalUser;
        public IntPtr ReleaseUser;
        public IntPtr GetISteamUser;
        public IntPtr GetISteamGameServer;
        public IntPtr SetLocalIPBinding;
        public IntPtr GetISteamFriends;
        public IntPtr GetISteamUtils;
        public IntPtr GetISteamMatchmaking;
        public IntPtr GetISteamMatchmakingServers;
        public IntPtr GetISteamGenericInterface;
        public IntPtr GetISteamUserStats;
        public IntPtr GetISteamGameServerStats;
        public IntPtr GetISteamApps;
        public IntPtr GetISteamNetworking;
        public IntPtr GetISteamRemoteStorage;
        public IntPtr GetISteamScreenshots;
        public IntPtr GetISteamGameSearch;
        public IntPtr RunFrame;
        public IntPtr GetIPCCallCount;
        public IntPtr SetWarningMessageHook;
        public IntPtr ShutdownIfAllPipesClosed;
        public IntPtr GetISteamHTTP;
        public IntPtr DEPRECATED_GetISteamUnifiedMessages;
        public IntPtr GetISteamController;
        public IntPtr GetISteamUGC;
        public IntPtr GetISteamAppList;
        public IntPtr GetISteamMusic;
        public IntPtr GetISteamMusicRemote;
        public IntPtr GetISteamHTMLSurface;
        public IntPtr DEPRECATED_Set_SteamAPI_CPostAPIResultInProcess;
        public IntPtr DEPRECATED_Remove_SteamAPI_CPostAPIResultInProcess;
        public IntPtr Set_SteamAPI_CCheckCallbackRegisteredInProcess;
        public IntPtr GetISteamInventory;
        public IntPtr GetISteamVideo;
        public IntPtr GetISteamParentalSettings;
        public IntPtr GetISteamInput;
        public IntPtr GetISteamParties;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ISteamUser012 {
        public IntPtr GetHSteamUser;
        public IntPtr LoggedOn;
        public IntPtr GetSteamID;
        public IntPtr InitiateGameConnection;
        public IntPtr TerminateGameConnection;
        public IntPtr TrackAppUsageEvent;
        public IntPtr GetUserDataFolder;
        public IntPtr StartVoiceRecording;
        public IntPtr StopVoiceRecording;
        public IntPtr GetCompressedVoice;
        public IntPtr DecompressVoice;
        public IntPtr GetAuthSessionTicket;
        public IntPtr BeginAuthSession;
        public IntPtr EndAuthSession;
        public IntPtr CancelAuthTicket;
        public IntPtr UserHasLicenseForApp;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ISteamUserStats013 {
        public IntPtr GetStatFloat;
        public IntPtr GetStatInteger;
        public IntPtr SetStatFloat;
        public IntPtr SetStatInteger;
        public IntPtr UpdateAvgRateStat;
        public IntPtr GetAchievement;
        public IntPtr SetAchievement;
        public IntPtr ClearAchievement;
        public IntPtr GetAchievementAndUnlockTime;
        public IntPtr StoreStats;
        public IntPtr GetAchievementIcon;
        public IntPtr GetAchievementDisplayAttribute;
        public IntPtr IndicateAchievementProgress;
        public IntPtr GetNumAchievements;
        public IntPtr GetAchievementName;
        public IntPtr RequestUserStats;
        public IntPtr GetUserStatFloat;
        public IntPtr GetUserStatInt;
        public IntPtr GetUserAchievement;
        public IntPtr GetUserAchievementAndUnlockTime;
        public IntPtr ResetAllStats;
        public IntPtr FindOrCreateLeaderboard;
        public IntPtr FindLeaderboard;
        public IntPtr GetLeaderboardName;
        public IntPtr GetLeaderboardEntryCount;
        public IntPtr GetLeaderboardSortMethod;
        public IntPtr GetLeaderboardDisplayType;
        public IntPtr DownloadLeaderboardEntries;
        public IntPtr DownloadLeaderboardEntriesForUsers;
        public IntPtr GetDownloadedLeaderboardEntry;
        public IntPtr UploadLeaderboardScore;
        public IntPtr AttachLeaderboardUGC;
        public IntPtr GetNumberOfCurrentPlayers;
        public IntPtr RequestGlobalAchievementPercentages;
        public IntPtr GetMostAchievedAchievementInfo;
        public IntPtr GetNextMostAchievedAchievementInfo;
        public IntPtr GetAchievementAchievedPercent;
        public IntPtr RequestGlobalStats;
        public IntPtr GetGlobalStatFloat;
        public IntPtr GetGlobalStatInteger;
        public IntPtr GetGlobalStatHistoryFloat;
        public IntPtr GetGlobalStatHistoryInteger;
        public IntPtr GetAchievementProgressLimitsFloat;
        public IntPtr GetAchievementProgressLimitsInteger;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ISteamUtils005 {
        public IntPtr GetSecondsSinceAppActive;
        public IntPtr GetSecondsSinceComputerActive;
        public IntPtr GetConnectedUniverse;
        public IntPtr GetServerRealTime;
        public IntPtr GetIPCountry;
        public IntPtr GetImageSize;
        public IntPtr GetImageRGBA;
        public IntPtr GetCSERIPPort;
        public IntPtr GetCurrentBatteryPower;
        public IntPtr GetAppID;
        public IntPtr SetOverlayNotificationPosition;
        public IntPtr IsAPICallCompleted;
        public IntPtr GetAPICallFailureReason;
        public IntPtr GetAPICallResult;
        public IntPtr RunFrame;
        public IntPtr GetIPCCallCount;
        public IntPtr SetWarningMessageHook;
        public IntPtr IsOverlayEnabled;
        public IntPtr OverlayNeedsPresent;
    }
}
