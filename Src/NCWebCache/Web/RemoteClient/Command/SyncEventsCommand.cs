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

using Alachisoft.NCache.Caching;
using System.Collections.Generic;

namespace Alachisoft.NCache.Web.Command
{
    internal sealed class SyncEventsCommand : CommandBase
    {
        private Alachisoft.NCache.Common.Protobuf.SyncEventsCommand _syncEventCommand;

        private Alachisoft.NCache.Common.Protobuf.EventIdCommand _eventIdCommand;

        public SyncEventsCommand(List<EventId> eventIds)
        {
            base.name = "SyncEventsCommand";

            _syncEventCommand = new Alachisoft.NCache.Common.Protobuf.SyncEventsCommand();
            _syncEventCommand.requestId = base.RequestId;


            foreach (EventId eventId in eventIds)
            {
                _eventIdCommand = new Alachisoft.NCache.Common.Protobuf.EventIdCommand();
                _eventIdCommand.eventType = (int) eventId.EventType;
                _eventIdCommand.eventUniqueId = eventId.EventUniqueID;
                _eventIdCommand.eventCounter = eventId.EventCounter;
                _eventIdCommand.operationCounter = eventId.OperationCounter;

                _eventIdCommand.queryChangeType = (int) eventId.QueryChangeType;

                _eventIdCommand.queryId = eventId.QueryId;

                _syncEventCommand.eventIds.Add(_eventIdCommand);
            }
        }

        internal override CommandType CommandType
        {
            get { return CommandType.SYNC_EVENTS; }
        }

        internal override RequestType CommandRequestType
        {
            get { return RequestType.InternalCommand; }
        }

        internal override bool IsKeyBased
        {
            get { return false; }
        }


        protected override void CreateCommand()
        {
            base._command = new Alachisoft.NCache.Common.Protobuf.Command();
            base._command.requestID = base.RequestId;
            base._command.syncEventsCommand = _syncEventCommand;
            base._command.type = Alachisoft.NCache.Common.Protobuf.Command.Type.SYNC_EVENTS;
        }
    }
}