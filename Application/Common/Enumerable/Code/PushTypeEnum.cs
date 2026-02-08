using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Enumerable.Code
{
    public enum PushTypeEnum
    {
        PushSignUpUser = 1,
        PushSignInUser = 2,
        PushSignUpAdmin = 3,
        PushRegisterOrderUser = 4,
        PushProccessOrderUser = 5,
        PushSentOrderUser = 6,
        PushRegisterOrderStore = 7,
        PushRegisterOrderAdmin = 8,
        PushSentOrderAdmin = 9,
        PushRegisterReserveUser = 10,
        PushCompleteReserveUser = 11,
        PushCancelReserveUser = 12,
        PushRegisterReserveCompanion = 13,
        PushCancelReserveCompanion = 14,
        PushRegisterReserveAdmin = 15,
        PushCompleteReserveAdmin = 16,
        PushCancelReserveAdmin = 17,
        PushRegisterPansionUser = 18,
        PushCompletePansionUser = 19,
        PushRegisterPansionCompanion = 20,
        PushRegisterPansionAdmin = 21,
        PushCompletePansionAdmin = 22
    }
}
