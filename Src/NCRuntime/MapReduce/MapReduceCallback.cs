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

namespace Alachisoft.NCache.Runtime.MapReduce
{
    /// <summary>
    ///Async event of registered callback in case of complete execution, failure or cancellation of task. 
    /// </summary>
    /// <param name="response">Encapsulates task and result if completed</param>
    /// <remarks><b>Note:</b> If map reduce call back is registered than GetTaskResult(taskID) for specific taskID can not be called. </remarks>
    /// 
    public delegate void MapReduceCallback(ITaskResult response);
}
