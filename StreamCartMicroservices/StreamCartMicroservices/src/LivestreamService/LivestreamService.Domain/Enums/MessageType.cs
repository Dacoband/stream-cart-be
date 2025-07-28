using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Domain.Enums
{
    /// <summary>
    /// Represents the type of message in chat and livestream
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Regular text message
        /// </summary>
        Text = 0,

        /// <summary>
        /// Image message
        /// </summary>
        Image = 1,

        /// <summary>
        /// Product message (e.g., sharing product in livestream)
        /// </summary>
        Product = 2,

        /// <summary>
        /// Gift message (e.g., virtual gifts in livestream)
        /// </summary>
        Gift = 3,

        /// <summary>
        /// System message (e.g., user joined, left, etc.)
        /// </summary>
        System = 4,

        /// <summary>
        /// File attachment message
        /// </summary>
        File = 5
    }
}
