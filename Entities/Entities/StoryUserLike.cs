using Entities.Entities.CommonField;
using Entities.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class StoryUserLike : Id_Field
    { 
        public long StoryItemId { get; set; }
        public long UserId { get; set; }

        public StoryItem StoryItem { get; set; }
        public User User { get; set; }
    }
}
