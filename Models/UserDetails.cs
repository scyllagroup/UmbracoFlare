using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UmbracoFlare.Models
{
    public class UserDetails : BasicCloudflareResponse
    {
        [JsonProperty(PropertyName="result")]
        public UserDetailResult Result { get; set; }
    }

    public class UserDetailResult
    {
        [JsonProperty(PropertyName="email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName="first_name")]
        public string FirstName{ get; set; }

        [JsonProperty(PropertyName="last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName="username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName="telephone")]
        public string Telephone { get; set; }

        [JsonProperty(PropertyName="country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName="zipcode")]
        public string Zipcode { get; set; }

        [JsonProperty(PropertyName="created_on")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty(PropertyName="modified_on")]
        public DateTime ModifiedOn { get; set; }

        [JsonProperty(PropertyName="api_key")]
        public string ApiKey { get; set; }

        [JsonProperty(PropertyName="two_factor_authentication_enabled")]
        public bool TwoFactorAuthenticationEnabled { get; set; }
    }
}
