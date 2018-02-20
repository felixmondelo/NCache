// Copyright (c) 2018 Alachisoft
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
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using Alachisoft.NCache.Common;
using Alachisoft.NCache.Common.Logger;

namespace Alachisoft.NCache.Caching.Util
{
    internal class OleDbConnectionPool : ResourcePool
    {
        internal class SqlDbResourceInfo : IDisposable
        {
            private OleDbConnection _conn;
            private IDictionary _syncData;

            public SqlDbResourceInfo(OleDbConnection conn)
            {
                _conn = conn;
            }

            #region	/                 --- IDisposable ---           /

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or 
            /// resetting unmanaged resources.
            /// </summary>
            void IDisposable.Dispose()
            {
                if(_conn != null)
                {
                    lock (_conn)
                    {
                        _conn.Close();
                        _conn.Dispose();
                        _conn = null;
                    }
                }
            }

            #endregion

            public OleDbConnection Connection 
            { 
                get { return _conn; } 
                set { _conn = value; } 
            }

            public IDictionary DbSyncInfo
            {
                get {return _syncData;} 
                set {_syncData = value;}
            }
        }

        private ILogger _ncacheLog;

        ILogger NCacheLog
        {
            get { return _ncacheLog; }
        }

        public OleDbConnectionPool(ILogger NCacheLog)
        {
            this._ncacheLog = NCacheLog;
        }

        /// <summary>
        /// Adds a connection to the _connectionTable if already not present.
        /// Otherwise, increments the referrence count for it.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public OleDbConnection PoolConnection(string connString, OleDbConnection connection)
        {
            lock (this)
            {
                string connKey = connString.ToLower();
                SqlDbResourceInfo connInfo = (SqlDbResourceInfo)GetResource(connKey);
                if (connInfo == null)
                {
                    //OleDbConnection conn= new OleDbConnection(connString);
                    //connection.ConnectionString = connection.ConnectionString;
                    connection.Open();
                    connInfo = new SqlDbResourceInfo(connection);
                    AddResource(connKey, connInfo);
                }
                else
                {
                    AddResource(connKey, null); // To increase the reference count
                }
                return connInfo.Connection;
            }
        }


        /// <summary>
        /// When connection is no more required, it is closed and removed from the 
        /// _connectionTable.
        /// </summary>
        /// <param name="connString"></param>
        public void RemoveConnection(string connString)
        {
            lock (this)
            {
                RemoveResource(connString); //temporarily not removing connection instance. SarahMui:
            }
        }


        /// <summary>
        /// Wrapper for ResourcePool.GetResource(string key).
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        public OleDbConnection GetConnection(string connString)
        {
            lock (this)
            {
                SqlDbResourceInfo connInfo = (SqlDbResourceInfo) GetResource(connString);
                if(connInfo != null)
                    return connInfo.Connection;
                return null;
            }
        }


        /// <summary>
        /// Wrapper for ResourcePool.GetResource(string key).
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        public IDictionary GetResourceSyncInfo(string connString)
        {
            lock (this)
            {
                SqlDbResourceInfo connInfo = (SqlDbResourceInfo) GetResource(connString);
                if(connInfo != null)
                    return connInfo.DbSyncInfo;
                return null;
            }
        }
		

        /// <summary>
        /// Acquire the modified records in ncache_db_sync table
        /// </summary>
        /// <param name="syncTable"></param>
        /// <param name="cacheName"></param>
        public void AcquireSyncData(string syncTable, string cacheName)
        {
            lock (this)
            {
                IEnumerator em = Keys.GetEnumerator();
                while (em.MoveNext())
                {
                    SqlDbResourceInfo connInfo = (SqlDbResourceInfo) GetResource((string)em.Current); 
					
                    IDictionary dbSyncInfo = LoadTableData(syncTable, cacheName, connInfo.Connection);
                    connInfo.DbSyncInfo = dbSyncInfo;
                }
            }
        }

        /// <summary>
        /// Removes the modified records in ncache_db_sync table
        /// </summary>
        /// <param name="syncTable"></param>
        /// <param name="cacheName"></param>
        public void RemoveSyncData(string syncTable, string cacheName)
        {
            lock (this)
            {
                IEnumerator em = Keys.GetEnumerator();
                while (em.MoveNext())
                {
                    SqlDbResourceInfo connInfo = (SqlDbResourceInfo)GetResource((string)em.Current);

                    RemoveTableData(syncTable, cacheName, connInfo.Connection);
                    connInfo.DbSyncInfo = null;
                }
            }
        }

        /// <summary>
        /// Remove all the stored sync information
        /// </summary>
        public void FlushSyncData()
        {
            lock (this)
            {
                IEnumerator em = Keys.GetEnumerator();
                while (em.MoveNext())
                {
                    SqlDbResourceInfo connInfo = (SqlDbResourceInfo) GetResource((string)em.Current);
                    connInfo.DbSyncInfo = null;
                }
            }		
        }

        /// <summary>
        /// Load the modified records for the given cache and set these flags to false
        /// </summary>
        /// <returns></returns>
        private Hashtable LoadTableData(string syncTable, string cacheName, OleDbConnection connection)
        {
            object[] tableInfo = new object[] { syncTable, cacheName };
            Hashtable tableData = new Hashtable();

            OleDbDataReader reader = null;
            OleDbCommand command = null;

            string cacheKey = "";
            bool modified = false;
            bool hasRows = false;

            lock (connection)
            {
                //if (nTrace.IsErrorEnabled) nTrace.error("ConnectionPool.LoadTableData", "Begining transaction");
                var transaction = connection.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    command = connection.CreateCommand();
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET WORK_IN_PROGRESS = 1 WHERE CACHE_ID = '{1}' AND MODIFIED = 1", tableInfo);
                    command.CommandType = CommandType.Text;
                    command.Transaction = transaction;

                    reader = command.ExecuteReader();
                }
                catch (Exception ex)
                {
                    NCacheLog.Error(cacheName, ex.ToString());
                    transaction.Rollback();
                    return null;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();

                        reader.Dispose();

                        reader = null;
                    }
                    if (command != null)
                    {
                        command.Dispose();
                        command = null;
                    }
                }

                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    command = connection.CreateCommand();
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT CACHE_KEY, MODIFIED FROM {0} WHERE CACHE_ID = '{1}' AND WORK_IN_PROGRESS = 1", tableInfo);
                    command.CommandType = CommandType.Text;
                    command.Transaction = transaction;

                    reader = command.ExecuteReader();
                    hasRows = reader.HasRows;

                    while (reader.Read())
                    {
                        cacheKey = Convert.ToString(reader.GetValue(0));
                        modified = Convert.ToBoolean(reader.GetValue(1));
                        tableData.Add(cacheKey, modified);
                    }
                }
                catch (Exception ex)
                {
                    NCacheLog.Error(cacheName, ex.ToString());
                    transaction.Rollback();
                    return null;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();

                        reader.Dispose();

                        reader = null;
                    }
                    if (command != null)                    
                    {
                        command.Dispose();
                        command = null;
                    }
                }

                transaction.Commit();
            }

            return tableData;
        }

        /// <summary>
        /// Remove the modified records from ncache_db_sync
        /// </summary>
        /// <param name="syncTable"></param>
        /// <param name="cacheName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        private bool RemoveTableData(string syncTable, string cacheName, OleDbConnection connection)
        {
            object[] tableInfo = new object[] { syncTable, cacheName };

            OleDbCommand command = null;

            lock (connection)
            {
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }
                    command = connection.CreateCommand();
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE CACHE_ID = '{1}' AND WORK_IN_PROGRESS = 1", tableInfo);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    NCacheLog.Error(cacheName, ex.ToString());
                    return false;
                }
                finally
                {
                    if (command != null)
                    {
                        command.Dispose();
                        command = null;
                    }
                }
            } //end Lock()
        }

        /// <summary>
        /// Gets the keys which have been modified in the database.
        /// call this method after acquiring the latest database state.
        /// </summary>
        /// <returns> array list of all the modified keys. </returns>
        internal ArrayList GetExpiredKeys()
        {
            ArrayList keys = new ArrayList();
            lock (this)
            {
                IEnumerator em = Keys.GetEnumerator();
                while (em.MoveNext())
                {
                    SqlDbResourceInfo connInfo = (SqlDbResourceInfo)GetResource((string)em.Current);
                    if (connInfo != null && connInfo.DbSyncInfo != null)
                    {
                        keys.AddRange(connInfo.DbSyncInfo.Keys);
                        connInfo.DbSyncInfo = null;
                    }
                }
            }
            return keys;
        }
    }
}