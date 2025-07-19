using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Settings
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = default!;
        public string DatabaseName { get; set; } = "StreamCartDb"; 
        public string LivestreamChatCollectionName { get; set; } = "LivestreamChats";
        public string ChatRoomCollectionName { get; set; } = "ChatRooms";
        public string ChatMessageCollectionName { get; set; } = "ChatMessages";
    }
}
