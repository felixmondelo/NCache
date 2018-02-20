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
using System.Collections.Generic;
using System.Collections;
using Alachisoft.NCache.Persistence;
using Alachisoft.NCache.Caching;

namespace Alachisoft.NCache.SocketServer.Command.ResponseBuilders
{
    class SyncEventResponseBuilder : ResponseBuilderBase
    {
        public static IList BuildResponse(List<Event> events, string requestId, IList serializedResponse, string clientId, int commandID, Caching.Cache cache)
        {
            long requestID = Convert.ToInt64(requestId);
           
            Alachisoft.NCache.Common.Protobuf.Response response = new Alachisoft.NCache.Common.Protobuf.Response();
            Alachisoft.NCache.Common.Protobuf.SyncEventsResponse syncEventResponse = new Alachisoft.NCache.Common.Protobuf.SyncEventsResponse();
            response.syncEventsResponse = syncEventResponse;
            response.requestId = requestID;
            response.commandID = commandID;
            response.responseType = Alachisoft.NCache.Common.Protobuf.Response.Type.SYNC_EVENTS;
            
            foreach (Event evt in events)
            {
                Alachisoft.NCache.Common.Protobuf.EventInfo evtInfo = new Alachisoft.NCache.Common.Protobuf.EventInfo();
                evtInfo.eventId = new Common.Protobuf.EventId();
                evtInfo.eventId.eventUniqueId = evt.PersistedEventId.EventUniqueID;
                evtInfo.eventId.eventCounter = evt.PersistedEventId.EventCounter;
                evtInfo.eventId.operationCounter = evt.PersistedEventId.OperationCounter;
                
                switch (evt.PersistedEventId.EventType)
                {
                    case EventType.CACHE_CLEARED_EVENT:
                        evtInfo.eventType = Alachisoft.NCache.Common.Protobuf.EventInfo.EventType.CACHE_CLEARED_EVENT;
                        break;


                    case EventType.CQ_CALLBACK:
                        evtInfo.key = evt.PersistedEventInfo.Key;
                        evtInfo.queryId = evt.PersistedEventId.QueryId;
                        evtInfo.changeType = (int)evt.PersistedEventId.QueryChangeType;
                        evtInfo.eventType = Alachisoft.NCache.Common.Protobuf.EventInfo.EventType.CQ_CALLBACK;
                        break; 


                    case EventType.ITEM_ADDED_EVENT:
                        evtInfo.key = evt.PersistedEventInfo.Key;
                        evtInfo.eventType = Alachisoft.NCache.Common.Protobuf.EventInfo.EventType.ITEM_ADDED_EVENT;
                        break;


                    case EventType.ITEM_REMOVED_CALLBACK:
                        evtInfo.key = evt.PersistedEventInfo.Key;
                        evtInfo.flag = evt.PersistedEventInfo.Flag.Data;
                        foreach (CallbackInfo cbInfo in evt.PersistedEventInfo.CallBackInfoList)
                        {
                            if (cbInfo.Client.Equals(clientId))
                            {
                                evtInfo.callbackId = (short)cbInfo.Callback;
                            }
                        }
                        evtInfo.value.AddRange((List<byte[]>)evt.PersistedEventInfo.Value);
                        evtInfo.itemRemoveReason = (int)evt.PersistedEventInfo.Reason;
                        evtInfo.eventType = Alachisoft.NCache.Common.Protobuf.EventInfo.EventType.ITEM_REMOVED_CALLBACK;
                        break;


                    case EventType.ITEM_REMOVED_EVENT:
                        evtInfo.key = evt.PersistedEventInfo.Key;
                        evtInfo.eventType = Alachisoft.NCache.Common.Protobuf.EventInfo.EventType.ITEM_REMOVED_EVENT;
                        break;


                    case EventType.ITEM_UPDATED_CALLBACK:
                        evtInfo.key = evt.PersistedEventInfo.Key;
                        foreach (CallbackInfo cbInfo in evt.PersistedEventInfo.CallBackInfoList)
                        {
                            if (cbInfo.Client.Equals(clientId))
                            {
                                evtInfo.callbackId = (short)cbInfo.Callback;
                            }
                        }
                        evtInfo.eventType = Alachisoft.NCache.Common.Protobuf.EventInfo.EventType.ITEM_UPDATED_CALLBACK;
                        break;


                    case EventType.ITEM_UPDATED_EVENT:
                        evtInfo.key = evt.PersistedEventInfo.Key;
                        evtInfo.eventType = Alachisoft.NCache.Common.Protobuf.EventInfo.EventType.ITEM_UPDATED_EVENT;
                        break;
                }

                response.syncEventsResponse.eventInfo.Add(evtInfo);
            }
            serializedResponse.Add(Alachisoft.NCache.Common.Util.ResponseHelper.SerializeResponse(response));

            return serializedResponse;
        }
    }
}
