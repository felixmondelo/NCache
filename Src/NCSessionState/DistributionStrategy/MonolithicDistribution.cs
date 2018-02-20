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
// limitations under the License


using System;
using System.Collections;
using System.Web.SessionState;
using Alachisoft.NCache.Serialization.Formatters;
using Alachisoft.NCache.Runtime;
using Alachisoft.NCache.Web.Util;
using Alachisoft.NCache.Web.Caching;
using Alachisoft.NCache.Runtime.Exceptions;
using Alachisoft.NCache.Web.SessionStateManagement;


namespace Alachisoft.NCache.Web.SessionState.DistributionStrategy
{
	/// <summary>
    /// Summary description for SingleValueDistribution.
    /// </summary>
    internal class MonolithicDistribution : IDistributionStrategy
    {
        private byte[] _table = null;
        private bool _isNewSession = false;
      

        internal MonolithicDistribution()
        {

        }

        #region	/                 --- IDistributionStrategy Members ---           /

        /// <summary>
        /// Fills the system ASP.NET session from NCache.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="cache"></param>
        /// <param name="strict"></param>
        void IDistributionStrategy.FillSessionFromCache(ISessionCache cache, HttpSessionState session, NSessionStateModule module, bool strict)
        {

            string sessionId = session.SessionID;

            SessionKey key = new SessionKey(sessionId, module.ApplicationId);
            if (strict)
                session.Clear();

            /// save the binary form of data, for comparision on FillCacheFromAspNet()
            _table = (byte[])cache.Get(key.ToString());

            if (_table == null)
            {
                _isNewSession = true;
                return;
            }

            Hashtable ht = (Hashtable)CompactBinaryFormatter.FromByteBuffer(_table, module.CacheID);
            if (ht == null)
            {
                return;
            }

            IDictionaryEnumerator i = ht.GetEnumerator();
            while (i.MoveNext())
            {
                session[i.Key.ToString()] = i.Value;
            }
        }

        /// <summary>
        /// Fills NCache from the system ASP.NET session.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="session"></param>
        /// <param name="strict"></param>
        /// <param name="async"></param>
        void IDistributionStrategy.FillCacheFromSession(ISessionCache cache, HttpSessionState session, NSessionStateModule module, bool strict, bool isAbandoned)
        {
            string sessionId = session.SessionID;
            SessionKey key = new SessionKey(sessionId, module.ApplicationId);

            try
            {

                if (session.IsReadOnly && cache.Contains(sessionId, key.ToString()) && !isAbandoned)
                    return;

                if (isAbandoned)// Session is not removed from store if it is cleared
                {
                    cache.Remove(sessionId, key.ToString(), false);
                    if (module.DetailedLogsEnabled) NSessionStateModule.NCacheLog.Debug(sessionId + " :session removed from cache");
                    return;
                }

                if (session.Count == 0 && _isNewSession) //We need not to keep any new empty session in the cache. 
                    return;

                IDictionary ht = new Hashtable();
                foreach (string skey in session.Keys)
                {
                    ht[skey] = session[skey];
                }

                byte[] _stable = CompactBinaryFormatter.ToByteBuffer(ht, module.CacheID);

                if (_table != null)
                {
                    if (BinaryComparer(_stable, _table))
                        return;
                }

                CacheItem sessionItem = new CacheItem(_stable);
                sessionItem.Priority = CacheItemPriority.NotRemovable;

                sessionItem.SlidingExpiration = TimeSpan.FromMinutes(session.Timeout);
                cache.Insert(sessionId, key.ToString(), sessionItem, false);
                
              
            }
            finally
            {
                if (session != null && strict) session.Clear();
            }
        }

        #endregion

        private bool BinaryComparer(byte[] src, byte[] cmp)
        {
            int srclen = src.Length;
            int cmplen = cmp.Length;

            if (srclen != cmplen)
                return false;

            for (int i = 0; i < srclen; i++)
            {
                if (src[i] != cmp[i])
                    return false;
            }
            return true;
        }
    }
}
