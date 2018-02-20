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
// limitations under the License.
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;


namespace Alachisoft.NCache.Config.Dom
{

    public static class ConfigConverter

    {
        public static Hashtable ToHashtable(CacheServerConfig config)
        {
            return DomToHashtable.GetConfig(config);
        }

        public static Hashtable ToHashtable(CacheServerConfig[] configs)
        {
            return DomToHashtable.GetConfig(configs);
        }

        public static CacheServerConfig[] ToDom(Hashtable config)
        {
            return HashtableToDom.GetConfig(config);
        }


        static class HashtableToDom
        {
            public static CacheServerConfig[] GetConfig(Hashtable config)
            {
                CacheServerConfig[] caches = new CacheServerConfig[config.Count];
                int i = 0;
                foreach (Hashtable cache in config.Values)
                    caches[i++] = GetCacheConfiguration(CollectionsUtil.CreateCaseInsensitiveHashtable(cache));
                return caches;
            }

            private static CacheServerConfig GetCacheConfiguration(Hashtable settings)
            {
                CacheServerConfig cache = new CacheServerConfig();
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                try
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    cache.Name = settings["id"].ToString();
                    if (settings.ContainsKey("web-cache"))
                        GetWebCache(cache, (Hashtable)settings["web-cache"]);
                    if (settings.ContainsKey("cache"))
                        GetCache(cache, (Hashtable)settings["cache"]);

                }
                finally
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                }
                return cache;
            }

            private static void GetCache(CacheServerConfig cache, Hashtable settings)
            {
                if (settings.ContainsKey("data-format"))
                    cache.DataFormat = settings["data-format"].ToString();
                if (settings.ContainsKey("backing-source"))
                    cache.BackingSource = GetBackingSource((Hashtable)settings["backing-source"]);
                if (settings.ContainsKey("client-death-detection"))
                    cache.ClientDeathDetection = GetClientDeathDetection((Hashtable)settings["client-death-detection"]);

                if (settings.ContainsKey("log"))
                    cache.Log = GetLog((Hashtable)settings["log"]);
                if (settings.ContainsKey("cache-classes"))
                    GetCacheClasses(cache, (Hashtable)settings["cache-classes"]);
                if (settings.ContainsKey("perf-counters"))
                    cache.PerfCounters = GetPerfCounters(settings);




                if (settings.ContainsKey("replication-strategy"))
                {
                    cache.ReplicationStrategy = GetReplicationStrategy((Hashtable)settings["replication-strategy"]);
                }

                if (settings.ContainsKey("sql-dependency"))
                {
                    cache.SQLDependencyConfig = GetSQLDependency((Hashtable)settings["sql-dependency"]);
                }

                if (settings.ContainsKey("synchronization"))
                {
                    cache.SynchronizationStrategy = GetSynchronizationStrategy((Hashtable)settings["synchronizaton"]);
                }

                if (settings.ContainsKey("tasks-config"))
                {
                    cache.TaskConfiguration = GetTaskConfiguration(settings["tasks-config"] as Hashtable);
                }


                if (settings.ContainsKey("client-activity-notification"))
                {
                    cache.ClientActivityNotification =
                        GetClientActivityNotification(settings["client-activity-notification"] as Hashtable);
                }
            }

            public static ClientActivityNotification GetClientActivityNotification(Hashtable settings)
            {
                ClientActivityNotification config = new ClientActivityNotification();
                config.Enabled = bool.Parse(settings["enabled"].ToString());
                config.Retention = int.Parse(settings["retention-period"].ToString());
                return config;
            }



            private static PerfCounters GetPerfCounters(Hashtable settings)
            {
                PerfCounters perCounters = new PerfCounters();
                perCounters.Enabled = Convert.ToBoolean(settings["perf-counters"]);
                return perCounters;
            }



            private static ReplicationStrategy GetReplicationStrategy(Hashtable settings)
            {
                ReplicationStrategy replicationStrategy = new ReplicationStrategy();
                if (settings.ContainsKey("synchronous"))
                    replicationStrategy.ReplicateSynchronous = Convert.ToBoolean(settings["synchronous"]);
                return replicationStrategy;
            }


            private static SQLDependencyConfig GetSQLDependency(Hashtable settings)
            {
                SQLDependencyConfig sqlDependencyConfig = new SQLDependencyConfig();
                if (settings.ContainsKey("use-default"))
                    sqlDependencyConfig.UseDefault = Convert.ToBoolean(settings["use-default"]);
                return sqlDependencyConfig;
            }


            private static SynchronizationStrategy GetSynchronizationStrategy(Hashtable settings)
            {
                SynchronizationStrategy synchronizationStrategy = new SynchronizationStrategy();
                if (settings.ContainsKey("strategy"))
                    switch (Convert.ToString(settings["strategy"]))
                    {
                        case "polling":
                            synchronizationStrategy.Strategy = "polling";
                            break;
                        case "notification":
                            synchronizationStrategy.Strategy = "notification";
                            break;
                    }
                if (settings.ContainsKey("polling-interval"))
                    synchronizationStrategy.Interval = Convert.ToInt32(settings["polling-interval"]);
                return synchronizationStrategy;
            }

            private static ServerMapping GetServerMapping(Hashtable settings)
            {
                ServerMapping serverMapping = new ServerMapping();
                if (settings.ContainsKey("server-end-point"))
                    serverMapping.MappingServers = GetMapping((Hashtable)settings["server-end-point"]);

                return serverMapping;
            }

            private static Mapping[] GetMapping(Hashtable settings)
            {
                Mapping[] mapping = null;
                if (settings.ContainsKey("end-point"))
                    mapping = settings["end-point"] as Mapping[];

                return mapping;
            }


            private static NotificationRecipient[] GetRecipients(Hashtable settings)
            {
                NotificationRecipient[] recipients = null;

                if (settings.Count != 0)
                {
                    recipients = new NotificationRecipient[settings.Count];
                    int index = 0;
                    IDictionaryEnumerator ide = settings.GetEnumerator();
                    while (ide.MoveNext())
                    {
                        recipients[index] = new NotificationRecipient();
                        recipients[index].ID = ide.Key.ToString();
                        index++;
                    }
                }
                return recipients;
            }

            private static void GetCacheClasses(CacheServerConfig cache, Hashtable settings)
            {
                if (settings.ContainsKey(cache.Name))
                    GetClassifiedCache(cache, (Hashtable)settings[cache.Name]);
            }

            private static void GetClassifiedCache(CacheServerConfig cache, Hashtable settings)
            {
                if (settings.ContainsKey("data-load-balancing"))
                    cache.AutoLoadBalancing = GetAutoLoadBalancing((Hashtable)settings["data-load-balancing"]);
                if (settings.ContainsKey("cluster"))
                    cache.Cluster = GetCluster(settings);
                if (settings.ContainsKey("internal-cache"))
                    GetInternalCache(cache, (Hashtable)settings["internal-cache"]);
                else
                    GetInternalCache(cache, settings);


                if (settings.ContainsKey("notifications"))
                    cache.Notifications = GetNotifications((Hashtable)settings["notifications"]);

            }

            private static AutoLoadBalancing GetAutoLoadBalancing(Hashtable settings)
            {
                AutoLoadBalancing autoLoadBalancing = new AutoLoadBalancing();
                if (settings.ContainsKey("enabled"))
                    autoLoadBalancing.Enabled = Convert.ToBoolean(settings["enabled"]);
                if (settings.ContainsKey("auto-balancing-threshold"))
                    autoLoadBalancing.Threshold = Convert.ToInt32(settings["auto-balancing-threshold"]);
                if (settings.ContainsKey("auto-balancing-interval"))
                    autoLoadBalancing.Interval = Convert.ToInt32(settings["auto-balancing-interval"]);
                return autoLoadBalancing;
            }


            private static Notifications GetNotifications(Hashtable settings)
            {
                Notifications notifications = new Notifications();
                if (settings.ContainsKey("item-remove"))
                    notifications.ItemRemove = Convert.ToBoolean(settings["item-remove"]);
                if (settings.ContainsKey("item-add"))
                    notifications.ItemAdd = Convert.ToBoolean(settings["item-add"]);
                if (settings.ContainsKey("item-update"))
                    notifications.ItemUpdate = Convert.ToBoolean(settings["item-update"]);
                if (settings.ContainsKey("cache-clear"))
                    notifications.CacheClear = Convert.ToBoolean(settings["cache-clear"]);
                return notifications;
            }

            private static void GetInternalCache(CacheServerConfig cache, Hashtable settings)
            {

                if (settings.ContainsKey("indexes"))
                    cache.QueryIndices = GetIndexes((Hashtable)settings["indexes"]);

                if (settings.ContainsKey("storage"))
                    cache.Storage = GetStorage((Hashtable)settings["storage"]);
                if (settings.ContainsKey("scavenging-policy"))
                    cache.EvictionPolicy = GetEvictionPolicy((Hashtable)settings["scavenging-policy"]);
                if (settings.ContainsKey("clean-interval"))
                    cache.Cleanup = GetCleanup(settings);
            }

            private static Cleanup GetCleanup(Hashtable settings)
            {
                Cleanup cleanup = new Cleanup();
                cleanup.Interval = Convert.ToInt32(settings["clean-interval"]);
                return cleanup;
            }

            private static EvictionPolicy GetEvictionPolicy(Hashtable settings)
            {
                EvictionPolicy evictionPolicy = new EvictionPolicy();

                if (settings.ContainsKey("priority"))
                    evictionPolicy.DefaultPriority = ((Hashtable)settings["priority"])["default-value"].ToString();
                if (settings.ContainsKey("evict-ratio"))
                    evictionPolicy.EvictionRatio = ((Convert.ToDecimal(settings["evict-ratio"])) < 5) ? 5 : (Convert.ToDecimal(settings["evict-ratio"]));


                return evictionPolicy;
            }

            private static QueryIndex GetIndexes(Hashtable settings)
            {
                QueryIndex indexes = new QueryIndex();
                if (settings.ContainsKey("index-classes"))
                    indexes.Classes = GetIndexClasses((Hashtable)settings["index-classes"]);
                return indexes;
            }

            private static Class[] GetIndexClasses(Hashtable settings)
            {
                Class[] classes = new Class[settings.Count];
                int i = 0;
                foreach (Hashtable cls in settings.Values)
                    classes[i++] = GetIndexClass(cls);
                return classes;
            }

            private static Class GetIndexClass(Hashtable settings)
            {
                Class cls = new Class();
                if (settings.ContainsKey("id"))
                    cls.ID = settings["id"].ToString();
                if (settings.ContainsKey("name"))
                    cls.Name = settings["name"].ToString();
                if (settings.ContainsKey("attributes"))
                    cls.Attributes = GetIndexAttributes((Hashtable)settings["attributes"]);
                return cls;
            }

            private static Attrib[] GetIndexAttributes(Hashtable settings)
            {
                Attrib[] attributes = new Attrib[settings.Count];
                int i = 0;
                foreach (Hashtable attrib in settings.Values)
                    attributes[i++] = GetIndexAttribute(attrib);
                return attributes;
            }

            private static Attrib GetIndexAttribute(Hashtable settings)
            {
                Attrib attrib = new Attrib();
                if (settings.ContainsKey("id"))
                    attrib.ID = settings["id"].ToString();
                if (settings.ContainsKey("data-type"))
                    attrib.Type = settings["data-type"].ToString();
                if (settings.ContainsKey("name"))
                    attrib.Name = settings["name"].ToString();
                return attrib;
            }

            private static Storage GetStorage(Hashtable settings)
            {
                Storage storage = new Storage();

                if (settings.ContainsKey("class"))
                    storage.Type = settings["class"].ToString();

                if (settings.ContainsKey("heap"))
                    storage.Size = Convert.ToInt64(((Hashtable)settings["heap"])["max-size"]);
                return storage;
            }






            private static Cluster GetCluster(Hashtable settings)
            {
                Cluster cluster = new Cluster();
                if (settings.ContainsKey("type"))
                {

                    cluster.Topology = settings["type"].ToString();
                }
                if (settings.ContainsKey("stats-repl-interval"))
                    cluster.StatsRepInterval = Convert.ToInt32(settings["stats-repl-interval"]);
                if (settings.ContainsKey("op-timeout"))
                    cluster.OpTimeout = Convert.ToInt32(settings["op-timeout"]);
                if (settings.ContainsKey("use-heart-beat"))
                    cluster.UseHeartbeat = Convert.ToBoolean(settings["use-heart-beat"]);

                settings = (Hashtable)settings["cluster"];
                if (settings.ContainsKey("channel"))
                    cluster.Channel = GetChannel((Hashtable)settings["channel"], 1);

                return cluster;
            }

            private static Channel GetChannel(Hashtable settings, int defaultPortRange)
            {
                Channel channel = new Channel(defaultPortRange);
                if (settings.ContainsKey("tcp"))
                    GetTcp(channel, (Hashtable)settings["tcp"]);
                if (settings.ContainsKey("tcpping"))
                    GetTcpPing(channel, (Hashtable)settings["tcpping"]);

                if (settings.ContainsKey("pbcast.gms"))
                    GetGMS(channel, (Hashtable)settings["pbcast.gms"]);

                return channel;
            }




            private static void GetTcpPing(Channel channel, Hashtable settings)
            {
                if (settings.ContainsKey("initial_hosts"))
                    channel.InitialHosts = settings["initial_hosts"].ToString();
                if (settings.ContainsKey("num_initial_members"))
                    channel.NumInitHosts = Convert.ToInt32(settings["num_initial_members"]);
                if (settings.ContainsKey("port_range"))
                    channel.PortRange = Convert.ToInt32(settings["port_range"]);
            }

            private static void GetTcp(Channel channel, Hashtable settings)
            {
                if (settings.ContainsKey("start_port"))
                    channel.TcpPort = Convert.ToInt32(settings["start_port"]);
                if (settings.ContainsKey("port_range"))
                    channel.PortRange = Convert.ToInt32(settings["port_range"]);
                if (settings.ContainsKey("connection_retries"))
                    channel.ConnectionRetries = Convert.ToInt32(settings["connection_retries"]);
                if (settings.ContainsKey("connection_retry_interval"))
                    channel.ConnectionRetryInterval = Convert.ToInt32(settings["connection_retry_interval"]);

            }

            private static void GetGMS(Channel channel, Hashtable settings)
            {
                if (settings.ContainsKey("join_retry_count"))
                    channel.JoinRetries = Convert.ToInt32(settings["join_retry_count"].ToString());
                if (settings.ContainsKey("join_retry_timeout"))
                    channel.JoinRetryInterval = Convert.ToInt32(settings["join_retry_timeout"]);
            }


            private static Log GetLog(Hashtable settings)
            {
                Log log = new Log();
                if (settings.ContainsKey("enabled"))
                    log.Enabled = Convert.ToBoolean(settings["enabled"]);
                if (settings.ContainsKey("trace-errors"))
                    log.TraceErrors = Convert.ToBoolean(settings["trace-errors"]);
                if (settings.ContainsKey("trace-debug"))
                    log.TraceDebug = Convert.ToBoolean(settings["trace-debug"]);
                if (settings.ContainsKey("log-path"))
                    log.LogPath = Convert.ToString(settings["log-path"]);
                return log;
            }



            private static TaskConfiguration GetTaskConfiguration(Hashtable settings)
            {
                TaskConfiguration taskConfig = new TaskConfiguration();
                if (settings.ContainsKey("max-tasks"))
                {
                    taskConfig.MaxTasks = Convert.ToInt32(settings["max-tasks"]);
                }
                if (settings.ContainsKey("chunk-size"))
                {
                    taskConfig.ChunkSize = Convert.ToInt32(settings["chunk-size"]);
                }
                if (settings.ContainsKey("communicate-stats"))
                {
                    taskConfig.CommunicateStats = Convert.ToBoolean(settings["communicate-stats"]);
                }
                if (settings.ContainsKey("queue-size"))
                {
                    taskConfig.QueueSize = Convert.ToInt32(settings["queue-size"]);
                }
                if (settings.ContainsKey("max-avoidable-exceptions"))
                {
                    taskConfig.MaxExceptions = Convert.ToInt32(settings["max-avoidable-exceptions"]);
                }
                return taskConfig;
            }

            private static ClientDeathDetection GetClientDeathDetection(Hashtable settings)
            {
                ClientDeathDetection deathDetection = new ClientDeathDetection();
                if (settings.ContainsKey("enable"))
                    deathDetection.Enabled = Convert.ToBoolean(settings["enable"]);
                if (settings.ContainsKey("grace-interval"))
                    deathDetection.GraceInterval = Convert.ToInt32(settings["grace-interval"]);
                return deathDetection;
            }

            private static BackingSource GetBackingSource(Hashtable settings)
            {
                BackingSource backingSource = new BackingSource();
                if (settings.ContainsKey("read-thru"))
                    backingSource.Readthru = GetReadThru((Hashtable)settings["read-thru"]);
                if (settings.ContainsKey("write-thru"))
                    backingSource.Writethru = GetWriteThru((Hashtable)settings["write-thru"]);
                return backingSource;
            }



            private static Writethru GetWriteThru(Hashtable settings)
            {
                Writethru writeThru = new Writethru();
                if (settings.ContainsKey("write-thru-providers"))
                    writeThru.Providers = GetProviders(settings["write-thru-providers"] as Hashtable);
                if (settings.ContainsKey("write-behind"))
                    writeThru.WriteBehind = GetWriteBehind(settings["write-behind"] as Hashtable);
                return writeThru;
            }

            private static Readthru GetReadThru(Hashtable settings)
            {
                Readthru readThru = new Readthru();
                if (settings.ContainsKey("read-thru-providers"))
                    readThru.Providers = GetProviders(settings["read-thru-providers"] as Hashtable);
                return readThru;
            }

            private static WriteBehind GetWriteBehind(Hashtable settings)
            {
                if (settings == null) return null;

                WriteBehind writeBehind = new WriteBehind();

                if (settings.ContainsKey("mode"))
                    writeBehind.Mode = settings["mode"].ToString();
                if (settings.ContainsKey("throttling-rate-per-sec"))
                    writeBehind.Throttling = settings["throttling-rate-per-sec"].ToString();
                if (settings.ContainsKey("failed-operations-queue-limit"))
                    writeBehind.RequeueLimit = settings["failed-operations-queue-limit"].ToString();
                if (settings.ContainsKey("failed-operations-eviction-ratio"))
                    writeBehind.Eviction = settings["failed-operations-eviction-ratio"].ToString();
                if (settings.ContainsKey("batch-mode-config"))
                    writeBehind.BatchConfig = GetBatchConfig(settings["batch-mode-config"] as Hashtable);

                return writeBehind;
            }

            private static BatchConfig GetBatchConfig(Hashtable settings)
            {
                if (settings == null) return null;

                BatchConfig batchConfig = new BatchConfig();
                if (settings.ContainsKey("batch-interval"))
                    batchConfig.BatchInterval = settings["batch-interval"].ToString();
                if (settings.ContainsKey("operation-delay"))
                    batchConfig.OperationDelay = settings["operation-delay"].ToString();

                return batchConfig;
            }

            private static Provider[] GetProviders(Hashtable settings)
            {
                if (settings == null) return null;
                Provider[] providers = new Provider[settings.Count];
                int i = 0;
                IDictionaryEnumerator ide = settings.GetEnumerator();
                while (ide.MoveNext())
                {
                    if (ide.Value is Hashtable)
                    {
                        Provider provider = new Provider();
                        Hashtable properties = (Hashtable)ide.Value;
                        IDictionaryEnumerator de = properties.GetEnumerator();
                        while (de.MoveNext())
                        {
                            if (de.Key.Equals("assembly-name"))
                            {
                                provider.AssemblyName = (string)de.Value;
                            }
                            if (de.Key.Equals("class-name"))
                            {
                                provider.ClassName = (string)de.Value;
                            }
                            if (de.Key.Equals("provider-name"))
                            {
                                provider.ProviderName = (string)de.Value;
                            }

                            if (de.Key.Equals("full-name"))
                            {
                                provider.FullProviderName = (string)de.Value;
                            }

                            if (de.Key.Equals("default-provider"))
                            {
                                provider.IsDefaultProvider = Convert.ToBoolean(de.Value);
                            }
                            if (de.Key.Equals("parameters"))
                            {
                                provider.Parameters = GetParameters(de.Value as Hashtable);
                            }
                        }
                        providers[i] = provider;
                        i++;
                    }
                }
                return providers;
            }

            private static Parameter[] GetParameters(Hashtable settings)
            {
                if (settings == null) return null;
                Parameter[] parameters = new Parameter[settings.Count];

                int i = 0;
                IDictionaryEnumerator ide = settings.GetEnumerator();
                while (ide.MoveNext())
                {
                    Parameter parameter = new Parameter();
                    parameter.Name = ide.Key as string;
                    parameter.ParamValue = ide.Value as string;
                    parameters[i] = parameter;
                    i++;
                }
                return parameters;
            }
            private static void GetWebCache(CacheServerConfig cache, Hashtable settings)
            {
                if (settings.ContainsKey("shared"))
                    cache.InProc = !Convert.ToBoolean(settings["shared"]);
            }
        }


        static class DomToHashtable
        {
            public static Hashtable GetCacheConfiguration(CacheServerConfig cache)
            {
                Hashtable config = new Hashtable();
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                try
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture =
           new System.Globalization.CultureInfo("en-US");
                    config.Add("type", "cache-configuration");
                    config.Add("id", cache.Name);
                    config.Add("cache", GetCache(cache));
                    config.Add("web-cache", GetWebCache(cache));
                }
                finally
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                }

                return config;
            }

            private static Hashtable GetCompactType(Type type)
            {
                Hashtable settings = new Hashtable();
                settings.Add("name", type.Name);
                settings.Add("portable", type.Portable);
                settings.Add("id", type.ID);
                if (type.PortableClasses != null)
                {
                    settings.Add("known-classes", GetCompactPortableClasses(type.PortableClasses));
                }
                return settings;
            }



            private static Hashtable GetCompactAttributeListUnion(AttributeListUnion attributeList)
            {
                Hashtable settings = new Hashtable();
                if (attributeList != null && attributeList.PortableAttributes != null)
                    settings.Add("attribute", GetCompactPortableAttributes(attributeList.PortableAttributes));
                return settings;

            }


            private static Hashtable GetCompactPortableClasses(PortableClass[] classes)
            {
                Hashtable settings = new Hashtable();
                foreach (PortableClass clas in classes)
                    settings.Add(clas.Name, GetCompactPortableClass(clas));
                return settings;
            }


            private static Hashtable GetCompactPortableClass(PortableClass clas)
            {
                Hashtable settings = new Hashtable();
                settings.Add("name", clas.Name);
                if (clas.ID != null)
                {
                    settings.Add("handle-id", clas.ID);
                }
                settings.Add("assembly", clas.Assembly);
                settings.Add("type", clas.Type);
                if (clas.PortableAttributes != null)
                    settings.Add("attribute", GetCompactPortableAttributes(clas.PortableAttributes));
                return settings;
            }

            private static Hashtable GetCompactPortableAttributes(PortableAttribute[] attributes)
            {
                Hashtable settings = new Hashtable();
                foreach (PortableAttribute attrib in attributes)
                    settings.Add(attrib.Name + ":" + attrib.Type, GetCompactPortableAttribute(attrib));
                return settings;
            }

            private static Hashtable GetCompactPortableAttribute(PortableAttribute attrib)
            {
                Hashtable settings = new Hashtable();
                settings.Add("name", attrib.Name);
                settings.Add("type", attrib.Type);
                settings.Add("order", attrib.Order);
                return settings;
            }

            public static Hashtable GetWebCache(CacheServerConfig cache)
            {
                Hashtable settings = new Hashtable();
                settings.Add("shared", (!cache.InProc).ToString().ToLower());
                settings.Add("cache-id", cache.Name);
                return settings;
            }

            public static Hashtable GetCache(CacheServerConfig cache)
            {
                Hashtable settings = new Hashtable();
                settings.Add("name", cache.Name);
                if (cache.Log != null)
                    settings.Add("log", GetLog(cache.Log));

                if (cache.DataFormat != null)
                    settings.Add("data-format", cache.DataFormat);
                settings.Add("cache-classes", GetCacheClasses(cache));
                settings.Add("class", cache.Name);

                if (cache.PerfCounters != null)
                    settings.Add("perf-counters", cache.PerfCounters.Enabled);


                if (cache.ReplicationStrategy != null)
                {
                    settings.Add("replication-strategy", GetReplicationStrategy(cache.ReplicationStrategy));
                }

                if (cache.SQLDependencyConfig != null)
                {
                    settings.Add("sql-dependency", GetSQLDependency(cache.SQLDependencyConfig));
                }

                if (cache.ClientDeathDetection != null)
                    settings.Add("client-death-detection", GetClientDeathDetection(cache.ClientDeathDetection));

                if (cache.BackingSource != null)
                    settings.Add("backing-source", GetBackingSource(cache.BackingSource));

                if (cache.TaskConfiguration != null)
                    settings.Add("tasks-config", GetTaskConfiguration(cache.TaskConfiguration));
                if (cache.SynchronizationStrategy != null)
                {
                    settings.Add("synchronization", GetSynchronizationStrategy(cache.SynchronizationStrategy));
                }

                if (cache.ClientActivityNotification != null)
                    settings.Add("client-activity-notification",
                        GetClientActivityNotification(cache.ClientActivityNotification));
                return settings;
            }

            public static Hashtable GetClientActivityNotification(ClientActivityNotification config)
            {
                Hashtable settings = new Hashtable();
                if (config.Enabled)
                {
                    settings.Add("enabled", config.Enabled);
                    settings.Add("retention-period", config.Retention);
                }
                return settings;
            }

            private static Hashtable GetBackingSource(BackingSource backingSource)
            {
                Hashtable settings = new Hashtable();
                if (backingSource.Readthru != null)
                    settings.Add("read-thru", GetReadThru(backingSource.Readthru));
                if (backingSource.Writethru != null)
                    settings.Add("write-thru", GetWriteThru(backingSource.Writethru));
                return settings;
            }



            private static Hashtable GetSynchronizationStrategy(SynchronizationStrategy syncStrategy)
            {
                Hashtable settings = new Hashtable();
                if (syncStrategy != null)
                {
                    settings.Add("strategy", syncStrategy.Strategy);
                    settings.Add("polling-interval", syncStrategy.Interval);
                }
                return settings;

            }





            private static Hashtable GetWriteThru(Writethru writethru)
            {
                Hashtable settings = new Hashtable();
                settings["enabled"] = writethru.Enabled.ToString();

                if (writethru.Providers != null)
                    settings.Add("write-thru-providers", GetProviders(writethru.Providers));

                if (writethru.WriteBehind != null)
                    settings.Add("write-behind", GetWriteBehind(writethru.WriteBehind));

                return settings;
            }

            private static Hashtable GetReadThru(Readthru readthru)
            {
                Hashtable settings = new Hashtable();
                settings["enabled"] = readthru.Enabled.ToString();

                if (readthru.Providers != null)
                    settings.Add("read-thru-providers", GetProviders(readthru.Providers));

                return settings;
            }

            private static Hashtable GetWriteBehind(WriteBehind writeBehind)
            {
                Hashtable settings = new Hashtable();

                if (writeBehind != null)
                {
                    settings["mode"] = writeBehind.Mode;
                    settings["throttling-rate-per-sec"] = writeBehind.Throttling;
                    settings["failed-operations-queue-limit"] = writeBehind.RequeueLimit;
                    settings["failed-operations-eviction-ratio"] = writeBehind.Eviction;
                    if (writeBehind.BatchConfig != null)
                        settings.Add("batch-mode-config", GetBatchConfig(writeBehind.BatchConfig));
                }

                return settings;
            }

            private static Hashtable GetBatchConfig(BatchConfig batchConfig)
            {
                Hashtable settings = new Hashtable();
                if (batchConfig != null)
                {
                    settings["batch-interval"] = batchConfig.BatchInterval;
                    settings["operation-delay"] = batchConfig.OperationDelay;
                }
                return settings;
            }

            private static Hashtable GetProviders(Provider[] providers)
            {
                Hashtable settings = new Hashtable();

                if (providers != null && providers.Length > 0)
                {
                    for (int i = 0; i < providers.Length; i++)
                    {
                        settings[providers[i].ProviderName] = GetProvider(providers[i]);
                    }
                }

                return settings;
            }

            private static Hashtable GetProvider(Provider provider)
            {
                Hashtable settings = new Hashtable();

                if (provider != null)
                {
                    settings["provider-name"] = provider.ProviderName;
                    settings["assembly-name"] = provider.AssemblyName;
                    settings["class-name"] = provider.ClassName;
                    settings["full-name"] = provider.FullProviderName;
                    settings["default-provider"] = provider.IsDefaultProvider.ToString();
                    Hashtable paramss = GetParameters(provider.Parameters);
                    if (paramss != null)
                        settings["parameters"] = paramss;
                }

                return settings;
            }

            private static Hashtable GetParameters(Parameter[] parameters)
            {
                if (parameters == null)
                    return null;

                Hashtable settings = new Hashtable();
                for (int i = 0; i < parameters.Length; i++)
                    settings[parameters[i].Name] = System.Web.HttpUtility.HtmlDecode(parameters[i].ParamValue);


                return settings;
            }



            private static Hashtable GetTaskConfiguration(TaskConfiguration taskConfig)
            {
                Hashtable settings = new Hashtable();
                settings.Add("max-tasks", taskConfig.MaxTasks);
                settings.Add("chunk-size", taskConfig.ChunkSize);
                settings.Add("communicate-stats", taskConfig.CommunicateStats);
                settings.Add("queue-size", taskConfig.QueueSize);
                settings.Add("max-avoidable-exceptions", taskConfig.MaxExceptions);
                return settings;
            }
            private static Hashtable GetClientDeathDetection(ClientDeathDetection deathDetection)
            {
                Hashtable settings = new Hashtable();
                settings.Add("enable", deathDetection.Enabled.ToString().ToLower());
                settings.Add("grace-interval", deathDetection.GraceInterval.ToString());
                return settings;
            }

            private static Hashtable GetCacheClasses(CacheServerConfig cache)
            {
                Hashtable settings = new Hashtable();
                settings.Add(cache.Name, GetClassifiedCache(cache));
                return settings;
            }

            private static Hashtable GetClassifiedCache(CacheServerConfig cache)
            {
                Hashtable settings = new Hashtable();
                settings.Add("id", cache.Name);
                if (cache.AutoLoadBalancing != null)
                {
                    settings.Add("data-load-balancing", GetAutoLoadBalancing(cache.AutoLoadBalancing));
                }

                if (cache.Cluster == null)
                {
                    settings.Add("type", "local-cache");
                    GetInternalCache(settings, cache, true);
                }
                else
                    GetCluster(settings, cache);

                if (cache.Notifications != null)
                    settings.Add("notifications", GetNotifications(cache.Notifications));

                return settings;
            }

            private static Hashtable GetAutoLoadBalancing(AutoLoadBalancing autoLoadBalancing)
            {
                Hashtable settings = new Hashtable();
                settings.Add("enabled", autoLoadBalancing.Enabled);
                settings.Add("auto-balancing-threshold", autoLoadBalancing.Threshold);
                settings.Add("auto-balancing-interval", autoLoadBalancing.Interval);
                return settings;
            }


            private static void GetInternalCache(Hashtable source, CacheServerConfig cache, bool localCache)
            {

                if (cache.QueryIndices != null)
                    source.Add("indexes", GetIndexes(cache.QueryIndices));

                if (cache.Storage != null)
                    source.Add("storage", GetStorage(cache.Storage));
                if (!localCache)
                {
                    source.Add("type", "local-cache");
                    source.Add("id", "internal-cache");
                }
                if (cache.EvictionPolicy != null)
                    source.Add("scavenging-policy", GetEvictionPolicy(cache.EvictionPolicy));
                if (cache.Cleanup != null)
                    source.Add("clean-interval", cache.Cleanup.Interval.ToString());
                if (cache.TaskConfiguration != null)
                    source.Add("tasks-config", GetTaskConfiguration(cache.TaskConfiguration));
            }

            private static Hashtable GetEvictionPolicy(EvictionPolicy evictionPolicy)
            {
                Hashtable settings = new Hashtable();

                settings.Add("class", "priority");
                settings.Add("eviction-enabled", true);
                settings.Add("priority", GetEvictionPriority(evictionPolicy));
                settings.Add("evict-ratio", evictionPolicy.EvictionRatio.ToString());
                return settings;
            }

            private static Hashtable GetEvictionPriority(EvictionPolicy evictionPolicy)
            {
                Hashtable settings = new Hashtable();
                settings.Add("default-value", evictionPolicy.DefaultPriority);
                return settings;
            }

            private static Hashtable GetStorage(Storage storage)
            {
                Hashtable settings = new Hashtable();

                settings.Add("class", storage.Type);
                if (storage.Type == "heap")
                    settings.Add("heap", GetHeap(storage));

                return settings;
            }






            private static Hashtable GetHeap(Storage storage)
            {
                Hashtable settings = new Hashtable();
                settings.Add("max-size", storage.Size.ToString());
                return settings;
            }

            private static Hashtable GetIndexes(QueryIndex indexes)
            {
                Hashtable settings = new Hashtable();
                if (indexes.Classes != null)
                    settings.Add("index-classes", GetIndexClasses(indexes.Classes));
                return settings;
            }

            private static Hashtable GetIndexClasses(Class[] classes)
            {
                Hashtable settings = new Hashtable();
                foreach (Class cls in classes)
                    settings.Add(cls.ID, GetIndexClass(cls));
                return settings;
            }

            private static Hashtable GetIndexClass(Class cls)
            {
                Hashtable settings = new Hashtable();
                settings.Add("name", cls.Name);
                settings.Add("type", "class");
                settings.Add("id", cls.ID);
                if (cls.Attributes != null)
                    settings.Add("attributes", GetIndexAttributes(cls.Attributes));
                return settings;
            }

            private static Hashtable GetIndexAttributes(Attrib[] attributes)
            {
                Hashtable settings = new Hashtable();
                foreach (Attrib attrib in attributes)
                    settings.Add(attrib.ID, GetIndexAttribute(attrib));
                return settings;
            }

            private static Hashtable GetIndexAttribute(Attrib attrib)
            {
                Hashtable settings = new Hashtable();
                settings.Add("name", attrib.Name);
                settings.Add("data-type", attrib.Type);
                settings.Add("type", "attrib");
                settings.Add("id", attrib.ID);
                return settings;
            }

            private static void GetCluster(Hashtable settings, CacheServerConfig cache)
            {
                settings.Add("type", cache.Cluster.Topology);
                settings.Add("stats-repl-interval", cache.Cluster.StatsRepInterval.ToString());
                settings.Add("op-timeout", cache.Cluster.OpTimeout.ToString());
                settings.Add("use-heart-beat", cache.Cluster.UseHeartbeat.ToString().ToLower());

                if (cache.Cleanup != null)
                    settings.Add("clean-interval", cache.Cleanup.Interval.ToString());
                settings.Add("internal-cache", new Hashtable());
                GetInternalCache((Hashtable)settings["internal-cache"], cache, false);

                Hashtable cluster = new Hashtable();
                cluster.Add("group-id", cache.Name);
                cluster.Add("class", "tcp");
                cluster.Add("channel", GetChannel(cache.Cluster.Channel, cache.Cluster.UseHeartbeat));

                settings.Add("cluster", cluster);
            }




            private static Hashtable GetReplicationStrategy(ReplicationStrategy replicationStrategy)
            {
                Hashtable settings = new Hashtable();
                settings.Add("synchronous", replicationStrategy.ReplicateSynchronous.ToString().ToLower());
                return settings;
            }

            private static Hashtable GetSQLDependency(SQLDependencyConfig sqlDependencyConfig)
            {
                Hashtable settings = new Hashtable();
                settings.Add("use-default", sqlDependencyConfig.UseDefault.ToString().ToLower());
                return settings;
            }

            private static Hashtable GetServerMapping(ServerMapping serverMapping)
            {
                Hashtable settings = new Hashtable();
                if (serverMapping.MappingServers != null && serverMapping.MappingServers.Length > 0)
                {
                    foreach (Mapping mapping in serverMapping.MappingServers)
                        settings.Add(mapping.PrivateIP, GetMapping(mapping));
                }

                return settings;
            }

            private static Hashtable GetMapping(Mapping mapping)
            {
                Hashtable settings = new Hashtable();
                if (mapping != null)
                {
                    settings.Add("private-ip", mapping.PrivateIP.ToString());
                    settings.Add("private-port", mapping.PrivatePort.ToString());
                    settings.Add("public-ip", mapping.PublicIP.ToString());
                    settings.Add("public-port", mapping.PublicPort.ToString());
                }
                return settings;
            }

            private static Hashtable GetMappings(Mapping[] mappings)
            {
                Hashtable settings = new Hashtable();


                return settings;
            }



            private static Hashtable GetRecipients(NotificationRecipient[] recipients)
            {
                Hashtable settings = new Hashtable();

                for (int i = 0; i < recipients.Length; i++)
                {
                    settings.Add(recipients[i].ID, recipients[i].ID);
                }

                return settings;
            }


            private static Hashtable GetChannel(Channel channel, bool useHeartBeat)
            {
                Hashtable settings = new Hashtable();
                settings.Add("tcp", GetTcp(channel, useHeartBeat));
                settings.Add("tcpping", GetTcpPing(channel));

                settings.Add("pbcast.gms", GetGMS(channel));

                return settings;
            }

            private static Hashtable GetTcpPing(Channel channel)
            {
                Hashtable settings = new Hashtable();
                settings.Add("initial_hosts", channel.InitialHosts.ToString());
                settings.Add("num_initial_members", channel.NumInitHosts.ToString());
                settings.Add("port_range", channel.PortRange.ToString());
                return settings;
            }

            private static Hashtable GetTcp(Channel channel, bool useHeartBeat)
            {
                Hashtable settings = new Hashtable();
                settings.Add("start_port", channel.TcpPort.ToString());
                settings.Add("port_range", channel.PortRange.ToString());
                settings.Add("connection_retries", channel.ConnectionRetries);
                settings.Add("connection_retry_interval", channel.ConnectionRetryInterval);
                settings.Add("use_heart_beat", useHeartBeat);
                return settings;
            }

            private static Hashtable GetGMS(Channel channel)
            {
                Hashtable settings = new Hashtable();
                settings.Add("join_retry_count", channel.JoinRetries.ToString());
                settings.Add("join_retry_timeout", channel.JoinRetryInterval.ToString());
                return settings;
            }

            private static Hashtable GetNotifications(Notifications notifications)
            {
                Hashtable settings = new Hashtable();
                settings.Add("item-add", notifications.ItemAdd.ToString().ToLower());
                settings.Add("item-remove", notifications.ItemRemove.ToString().ToLower());
                settings.Add("item-update", notifications.ItemUpdate.ToString().ToLower());
                settings.Add("cache-clear", notifications.CacheClear.ToString().ToLower());
                return settings;
            }

            private static Hashtable GetLog(Log log)
            {
                Hashtable settings = new Hashtable();
                settings.Add("enabled", log.Enabled.ToString().ToLower());
                settings.Add("trace-errors", log.TraceErrors.ToString().ToLower());
                settings.Add("trace-debug", log.TraceDebug.ToString().ToLower());
                settings.Add("log-path", log.LogPath.ToLower());
                return settings;
            }

            public static Hashtable GetConfig(CacheServerConfig cache)
            {
                return GetCacheConfiguration(cache);
            }

            public static Hashtable GetConfig(CacheServerConfig[] caches)
            {
                Hashtable settings = new Hashtable();
                foreach (CacheServerConfig cache in caches)
                    settings.Add(cache.Name, GetCacheConfiguration(cache));
                return settings;
            }
        }
    }
}
