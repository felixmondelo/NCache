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
using Alachisoft.NCache.Common.DataStructures;
using Alachisoft.NCache.Common.Monitoring;
using Alachisoft.NCache.SocketServer.RequestLogging;

namespace Alachisoft.NCache.SocketServer.Command
{
    class InquiryRequestCommand : CommandBase
    {
        private struct CommandInfo
        {
            public string RequestId;
        }

        private Bookie _bookie;

        public InquiryRequestCommand(Bookie bookie)
        {
            _bookie = bookie;
        }

        //PROTOBUF
        public override void ExecuteCommand(ClientManager clientManager, Common.Protobuf.Command command)
        {
            CommandInfo cmdInfo;
            NCache nCache = clientManager.CmdExecuter as NCache;
            byte[] data = null;

            try
            {
                if (ServerMonitor.MonitorActivity) ServerMonitor.LogClientActivity("ContCmd.Exec", "cmd parsed");
            }
            catch (Exception exc)
            {
                if (!base.immatureId.Equals("-2"))
                    //PROTOBUF:RESPONSE
                    _serializedResponsePackets.Add(Common.Util.ResponseHelper.SerializeExceptionResponse(exc, command.requestID, command.commandID));
                return;
            }

            try
            {
                RequestStatus requestStatus;

                if (command.inquiryRequestCommand.serverIP.Equals(ConnectionManager.ServerIpAddress))
                {
                    requestStatus = _bookie.GetRequestStatus(clientManager.ClientID,
                        command.inquiryRequestCommand.inquiryRequestId, command.inquiryRequestCommand.inquiryCommandId);
                }
                else
                {
                    requestStatus = nCache.Cache.GetClientRequestStatus(clientManager.ClientID,
                        command.inquiryRequestCommand.inquiryRequestId, command.inquiryRequestCommand.inquiryCommandId,
                        command.inquiryRequestCommand.serverIP);
                    if (requestStatus == null)
                    {
                        requestStatus = new RequestStatus(Common.Enum.RequestStatus.NODE_DOWN);
                    }
                }

                Common.Protobuf.Response response = new Common.Protobuf.Response();
                Common.Protobuf.InquiryRequestResponse inquiryResponse = new Common.Protobuf.InquiryRequestResponse();
                response.requestId = Convert.ToInt64(command.requestID);
                response.commandID = command.commandID;
                inquiryResponse.status = requestStatus.Status;

                if (inquiryResponse.status == Common.Enum.RequestStatus.RECEIVED_AND_EXECUTED)
                {
                    IList existingResponse = requestStatus.RequestResult;
                    foreach (byte[] extRes in existingResponse)
                    {
                        inquiryResponse.value.Add(extRes);
                    }
                }

                inquiryResponse.expirationInterval = _bookie.CleanInterval;
                response.responseType = Common.Protobuf.Response.Type.INQUIRY_REQUEST_RESPONSE;
                response.inquiryRequestResponse = inquiryResponse;
                _serializedResponsePackets.Add(Common.Util.ResponseHelper.SerializeResponse(response));
            }
            catch (Exception exc)
            {
                _serializedResponsePackets.Add(Common.Util.ResponseHelper.SerializeExceptionResponse(exc,
                    command.requestID, command.commandID));
            }
            if (ServerMonitor.MonitorActivity) ServerMonitor.LogClientActivity("ContCmd.Exec", "cmd executed on cache");
        }

        private CommandInfo ParseCommand(Alachisoft.NCache.Common.Protobuf.Command command, ClientManager clientManager)
        {
            CommandInfo cmdInfo = new CommandInfo();

            Alachisoft.NCache.Common.Protobuf.InquiryRequestCommand inquiryCommand = command.inquiryRequestCommand;

            cmdInfo.RequestId = command.requestID.ToString();

            return cmdInfo;
        }
    }
}