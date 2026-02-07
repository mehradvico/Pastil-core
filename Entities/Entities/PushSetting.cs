using Entities.Entities.CommonField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Entities
{
    public class PushSetting : Id_Field
    {
        public long PushPatternId { get; set; }
        public bool IsEnabled { get; set; } = true;

        public PushPattern PushPattern { get; set; }
    }

}
