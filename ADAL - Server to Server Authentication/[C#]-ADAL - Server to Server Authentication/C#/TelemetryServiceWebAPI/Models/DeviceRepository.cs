//----------------------------------------------------------------------------------------------
//    Copyright 2013 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//----------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Adal.TelemetryServiceWebApi.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class DeviceRepository : IDeviceRepository
    {
        private readonly ConcurrentBag<Device> devices = new ConcurrentBag<Device>();
        private int nextId = 1;

        public DeviceRepository()
        {
            
        }

        // The TelemetryServiceWebAPI can expose several "read" actions as HTTP GET methods, each action would correspond to a method in the DeviceController class. 
        // For the example we have chosen to expose a single "read" action that reads status of all client devices.
        public IEnumerable<Device> GetAll()
        {
            return this.devices;
        }

        public Device Add(Device item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            item.Id = this.nextId++;
            this.devices.Add(item);
            return item;
        }
    }
}