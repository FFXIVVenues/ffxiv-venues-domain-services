#pragma warning disable CS8618

namespace FFXIVVenues.ApiGateway.Security.V1._0
{
    public class AuthorizationKey
    {

        public string Key { get; set; }
        public string Scope { get; set; }
        public bool Create { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool ReadUnapproved { get; set; }
        public bool Approve { get; set; }

    }
}