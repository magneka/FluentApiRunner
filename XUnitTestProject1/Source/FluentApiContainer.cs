using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Source
{
    public class FluentApiContainer
    {
        public HttpClient HttpClient { get; set; }
        public HttpRequestMessage HttpRequestMessage { get; set; }

        public String Server { get; set; }
        public String Uri { get; set; }
        public String Method { get; set; }
        public String Contents { get; set; }
        public StringContent JsonContent { get; set; }
        public String AccessToken { get; set; }
        public String ExpiresIn { get; set; }
        public String TokenType { get; set; }
        public string RefreshToken { get; set; }
        public string AntiForgeryToken { get; set; }

        public string CookiesAsString { get; set; }

        public HttpResponseMessage Response { get; set; }

        public string Role { get; set; }

        public List<KeyValuePair<string, string>> AuthenticationHeaders { get; set; }

        public List<KeyValuePair<string, string>> Headers { get; set; }

        public List<KeyValuePair<string, string>> Cookies { get; set; }

        public List<KeyValuePair<string, string>> FormFieldsIn { get; set; }

        public FluentApiContainer()
        {
            HttpClient = new HttpClient();
            Server = "";
            Uri = "";
            Contents = "";
            AuthenticationHeaders = new List<KeyValuePair<string, string>>();
            Headers = new List<KeyValuePair<string, string>>();
            FormFieldsIn = new List<KeyValuePair<string, string>>();
            Cookies = new List<KeyValuePair<string, string>>();
        }       
    }
}
