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

namespace Alachisoft.NCache.Web.Command
{
    internal sealed class GetConnectedClientsCommand : CommandBase
    {
        Common.Protobuf.GetConnectedClientsCommand _getConnectedClients;

        public GetConnectedClientsCommand()
        {
            _getConnectedClients = new Common.Protobuf.GetConnectedClientsCommand();
            _getConnectedClients.requestId = RequestId;
        }

        internal override RequestType CommandRequestType
        {
            get { return RequestType.AtomicRead; }
        }

        internal override CommandType CommandType
        {
            get { return CommandType.GET_CONNECTED_CLIENTS; }
        }

        protected override void CreateCommand()
        {
            _command = new Common.Protobuf.Command();
            _command.requestID = RequestId;
            _command.getConnectedClientsCommand = _getConnectedClients;
            _command.type = Common.Protobuf.Command.Type.GET_CONNECTED_CLIENTS;
        }
    }
}