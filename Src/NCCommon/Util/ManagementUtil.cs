﻿// Copyright (c) 2018 Alachisoft
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License

using System;
using System.Collections.Generic;
using System.Text;

namespace Alachisoft.NCache.Common.Util
{
    public class ManagementUtil
    {
        public struct MethodName

        {
            #region [ Cache Server]
            public const string StopServer = "StopServer";
            public const string StartCache = "StartCache";
            public const string GetClusterIP = "GetClusterIP";
            public const string GetLocalCacheIP = "GetLocalCacheIP";
            public const string CopyAssemblies = "CopyAssemblies";
            public const string GetAssembly = "GetAssembly";
            public const string ClearCache = "ClearCache";
            public const string Authorize = "Authorize";
            public const string GetRunningCaches = "GetRunningCaches";
            public const string GetCacheProps = "GetCacheProps";
            public const string CacheProps = "CacheProps";
            public const string GetCacheConfiguration = "GetCacheConfiguration";
            public const string GetCacheInfo = "GetCacheInfo";
            public const string GetHostName = "GetHostName";
            public const string ReloadSrvcConfig = "ReloadSrvcConfig";
            public const string GetSocketServerPort = "GetSocketServerPort";
            public const string GetUpdatedCacheConfiguration = "GetUpdatedCacheConfiguration";
            public const string GetNodeInfo = "GetNodeInfo";
            public const string RemoveCacheServerFromClientConfig = "RemoveCacheServerFromClientConfig";
            public const string RemoveCacheFromClientConfig = "RemoveCacheFromClientConfig";
            public const string UpdateClientServersList = "UpdateClientServersList";
 
         
            public const string GetClientConfiguration = "GetClientConfiguration";
            public const string UpdateClientConfiguration = "UpdateClientConfiguration";
            public const string GetBindIP = "GetBindIP";
            public const string GetClientConfigId = "GetClientConfigId";
            public const string GetClientNodeStatus = "GetClientNodeStatus";
            public const string VerifyWindowsUser = "VerifyWindowsUser";
            public const string VerfyAdministrator = "VerfyAdministrator";
            public const string VerifyNodeAdministrator = "VerifyNodeAdministrator";
            public const string DisableLogging = "DisableLogging";
            public const string SynchronizeClientConfig = "SynchronizeClientConfig";
         
            public const string UnregisterCache = "UnregisterCache";
            public const string StartCachePhase2 = "StartCachePhase2";
            public const string StopCache = "StopCache";
            public const string DetectNICs = "DetectNICs";
            public const string BindToIP = "BindToIP";
            public const string BindedIp = "BindedIp";
            public const string GetMaxPort = "GetMaxPort";
            public const string IsClusteredCache = "IsClusteredCache";
            public const string IsPortAvailable = "IsPortAvailable";
            public const string NodeIsAllowed = "NodeIsAllowed";
            public const string Decrypt = "Decrypt";
            public const string GetCacheStatus = "GetCacheStatus";
            public const string StartMonitoringActivity = "StartMonitoringActivity";
            public const string StopMonitoringActivity = "StopMonitoringActivity";
            public const string PublishActivity = "PublishActivity";
            public const string GetCacheStatistics = "GetCacheStatistics";
            public const string GetCacheStatistics2 = "GetCacheStatistics2";
            public const string GetCacheServers = "GetCacheServers";
            public const string GetRunningCacheServers = "GetRunningCacheServers";
            public const string GetCacheClients = "GetCacheClients";
            public const string GetClientProcessStats = "GetClientProcessStats";
            public const string GetAllConfiguredCaches = "GetAllConfiguredCaches";
            public const string GetCacheConfigurationInfo = "GetCacheConfigurationInfo";
            public const string EnableLogging = "EnableLogging";
            public const string GetCaches = "GetCaches";
            public const string GetCacheRenderer = "GetCacheRenderer";
            public const string GetCacheInstanceIgnoreReplica = "GetCacheInstanceIgnoreReplica";
            public const string GetCacheInstance = "GetCacheInstance";
            public const string GetLicenseKey = "GetLicenseKey";
            public const string GetLicenses = "GetLicenses";
            public const string GetLicenseLogger = "GetLicenseLogger";
            public const string ClearCacheContent = "ClearCacheContent";
            public const string IsRunning = "IsRunning";
            public const string GetStatistics = "GetStatistics";
            public const string GetCacheCount = "GetCacheCount";
            public const string GetClusterNIC = "GetClusterNIC";
            public const string GetSocketServerNIC = "GetSocketServerNIC";
            public const string GetNICForIP = "GetNICForIP";
            public const string GetUpdatedCacheServers = "GetUpdatedCacheServers";
            public const string GetUpdatedRunningCacheServers = "GetUpdatedRunningCacheServers";
            public const string GetUpdatedCacheClients = "GetUpdatedCacheClients";
            public const string GetPercentageCPUUsage = "GetPercentageCPUUsage";
            public const string RegisterEventViewerEvents = "RegisterEventViewerEvents";
            public const string UnRegisterEventViewerEvents = "UnRegisterEventViewerEvents";
            public const string GetLatestEvents = "GetLatestEvents";
            public const string SetLocalCacheIP = "SetLocalCacheIP";
            public const string IsCacheRegistered = "IsCacheRegistered";
            public const string InitializeMonitor = "InitializeMonitor";
            public const string GetConfiguredPartitionedReplicaCaches = "GetConfiguredPartitionedReplicaCaches";
            public const string GetSnmpPorts = "GetSnmpPorts";
            public const string GetServerPlatform = "GetServerPlatform";
            public const string GetNewConfiguration = "GetNewConfiguration";
            public const string GetNewUpdatedCacheConfiguration = "GetNewUpdatedCacheConfiguration";
            public const string GetServerLicenseInfo = "GetServerLicenseInfo";
            public const string SetClientProductVersion = "SetClientProductVersion";
            public const string GetProductVersion = "GetProductVersion";
            public const string GetShutdownTimeout = "GetShutdownTimeout";
            public const string GetTayzGridServer = "GetTayzGridServer";
            public const string GetServerMappingForConfig = "GetServerMappingForConfig";
            public const string UpdateServerMappingConfig = "UpdateServerMappingConfig";

            public const string GarbageCollect = "GarbageCollect";

            public const string HotApplyConfiguration = "HotApplyConfiguration";
            public const string GetProcessId = "GetProcessId";
            public const string GetMaxManagementPort = "GetMaxManagementPort";
            public const string GetMaxSocketPort = "GetMaxSocketPort";
            public const string StopAllCaches = "StopAllCaches";
            public const string GetCache = "GetCache";
            public const string StopCacheInstance = "StopCacheInstance";
            public const string TransferConnection = "TransferConnection";
            public const string GetCacheName = "GetCacheName";
            public const string StopCachesOnNode = "StopCachesOnNode";
            public const string StopCacheOnHost = "StopCacheOnHost";
            public const string GetConfigurationVersion = "GetConfigurationVersion";
            public const string GetTopicStats = "GetTopicStats";
            public const string GetCacheProcessID = "GetCacheProcessID";
            public const string StartAPILogging = "StartAPILogging";
            public const string StopAPILogging = "StopAPILogging";
            public const string GetAPILogData = "GetAPILogData";



            #endregion


        }

        public struct ManagementObjectName
        {
            public const string CacheServer = "CacheServer";
        }
    }
}
