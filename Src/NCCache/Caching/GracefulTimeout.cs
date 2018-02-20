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
using Alachisoft.NCache.Common.Util;

namespace Alachisoft.NCache.Caching
{
    public class GracefulTimeout
    {


        public static string GetGracefulShutDownTimeout(ref int shutdownTimeout, ref int blockTimeout)
        {
            shutdownTimeout = 180;
            blockTimeout = 3;
            string exceptionMsgST = null;
            string gracefullTimeout = "NCacheServer.GracefullShutdownTimeout";
            string blockingActivity = "NCacheServer.BlockingActivityTimeout";

            try
            {
                shutdownTimeout = ServiceConfiguration.GracefullTimeout;
            }
            catch (Exception ex)
            {
                exceptionMsgST = "Invalid value is assigned to " + gracefullTimeout + ". Reassigning it to default value.(180 seconds)";
                shutdownTimeout = 180;
            }

            try
            {
                blockTimeout = ServiceConfiguration.BlockingActivity;
            }
            catch (Exception ex)
            {
                //NCacheLog.Error("Cache.GracefulShutDown", "Invalid value is assigned to NCacheServer.BlockingActivityTimeout. Reassigning it to default value.(3 seconds)");
                exceptionMsgST = "Invalid value is assigned to " + blockingActivity + ". Reassigning it to default value.(3 seconds)";
                blockTimeout = 3;
            }

            if (shutdownTimeout <= 0)
            {
                // _context.NCacheLog.Error("Cache.Stop", "Invalid value is assigned to NCacheServer.GracefullShutdownTimeout. Reassigning it to default value.(180 seconds)");
                exceptionMsgST = "Invalid value is assigned to " + gracefullTimeout + ". Reassigning it to default value.(180 seconds)";
                shutdownTimeout = 180;
            }

            if (blockTimeout <= 0)
            {
                //NCacheLog.Error("Cache.GracefulShutDown", "0 or negative value is assigned to NCacheServer.BlockingActivityTimeout. Reassigning it to default value.(3 seconds)");
                exceptionMsgST = "0 or negative value is assigned to " + blockingActivity + ". Reassigning it to default value.(3 seconds)";
                blockTimeout = 3;
            }


            if (blockTimeout >= shutdownTimeout)
            {
                //NCacheLog.Error("Cache.GracefulShutDown", "NCacheServer.BlockingActivityTimeout is greater than NCacheServer.GracefullShutdownTimeout. Reassigning it to default value.(3 seconds)");
                exceptionMsgST = blockingActivity+" is greater than or equal to "+gracefullTimeout+". Reassigning both to default value.";
                blockTimeout = 3;
                shutdownTimeout = 180;
            }

            return exceptionMsgST;
        }

  

    }
}