using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Consumers
{
    /// <summary>
    /// Marker interface để đánh dấu các consumer trong hệ thống messaging.
    /// Được sử dụng cho việc đăng ký tự động các consumer với MassTransit.
    /// </summary>
    public interface IBaseConsumer
    {
    }
}
