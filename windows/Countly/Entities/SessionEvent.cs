using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CountlySDK.Entities
{
    [DataContractAttribute]
    [KnownType(typeof(BeginSession))]
    [KnownType(typeof(UpdateSession))]
    [KnownType(typeof(EndSession))]
    internal abstract class SessionEvent
    {
        [DataMemberAttribute]
        public Dictionary<string, string> Content { get; set; } = new Dictionary<string, string>();
    }
}
