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
using Alachisoft.NCache.Common.Configuration;
using Alachisoft.NCache.Runtime.Serialization;

namespace Alachisoft.NCache.Config.Dom
{
	/// <summary>
	/// Dated: 20100413
	/// This Class is responsible for holding the recipient of notification
	/// ID attribute can be used to hold recipient of different type for example it can email, in future can be
	/// SMS etc
	/// </summary>
	[Serializable]
	public class NotificationRecipient : ICloneable,ICompactSerializable
	{
		private string _id;

        [ConfigurationAttribute("email-id")]//Changes for New Dom from id
		public string ID
		{
			get { return _id; }
			set { _id = value; }
		}

		#region ICloneable Members

		public object Clone()
		{
			NotificationRecipient recipient = new NotificationRecipient();
				recipient.ID = this.ID;
			return recipient;
		}

		#endregion

        #region ICompactSerializable Members

        public void Deserialize(Runtime.Serialization.IO.CompactReader reader)
        {
            _id = reader.ReadObject() as string;
        }

        public void Serialize(Runtime.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_id);
        }

        #endregion
    }
}
