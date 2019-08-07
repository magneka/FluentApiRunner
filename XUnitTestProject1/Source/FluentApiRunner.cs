using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Source
{    
    public class FluentApiRunner
    {
        public FluentApiContainer ApiContainer { get; set; }

        public string AccessToken { get => ApiContainer.AccessToken; set => ApiContainer.AccessToken = value; }
        public string ExpiresIn { get => ApiContainer.ExpiresIn; set => ApiContainer.ExpiresIn = value; }        
        public string TokenType { get => ApiContainer.TokenType; set => ApiContainer.TokenType = value; }         
        public string RefreshToken { get => ApiContainer.RefreshToken; set => ApiContainer.RefreshToken = value; }
        public string Role { get => ApiContainer.Role; set => ApiContainer.Role = value; }
        public HttpResponseMessage Response { get => ApiContainer.Response; set => ApiContainer.Response = value; }
        public string Contents { get => ApiContainer.Contents; set => ApiContainer.Contents = value; }
        public StringContent JsonContent { get => ApiContainer.JsonContent; set => ApiContainer.JsonContent = value; }
        public string AntiForgeryToken { get => ApiContainer.AntiForgeryToken; set => ApiContainer.AntiForgeryToken = value; }       
        public string CookiesAsString { get => ApiContainer.CookiesAsString; set => ApiContainer.CookiesAsString = value; }        
        

        public FluentApiRunner()
        {
            ApiContainer = new FluentApiContainer();
        }

        public FluentApiRunner(string Uri)
        {
            ApiContainer = new FluentApiContainer();
            SetServerPathAndParams(Uri);
        }
        public FluentApiRunner SetServer(string server)
        {
            ApiContainer.Server = server;
            return this;
        }

        public FluentApiRunner SetLocalpath(string uri)
        {
            ApiContainer.Uri = uri;
            return this;
        }

        public FluentApiRunner SetServerPathAndParams(string uriAsString)
        {
            if (!String.IsNullOrEmpty(uriAsString))
            {
                var uri = new Uri(uriAsString);

                ApiContainer.Server = "";
                if (!String.IsNullOrEmpty(uri.Scheme))
                    ApiContainer.Server = uri.Scheme + "://" + uri.IdnHost;
                else
                    ApiContainer.Server = uri.IdnHost;

                ApiContainer.Server = ApiContainer.Server + ":" + uri.Port.ToString();
                ApiContainer.Uri = uri.LocalPath;

                var query = HttpUtility.ParseQueryString(uri.Query);

                foreach (var parm in query.AllKeys)
                {
                    AddParam(parm, query.Get(parm));
                }
            }

            return this;
        }

        public FluentApiRunner AddJson(string value)
        {
            var _content = new StringContent(value, Encoding.UTF8, "application/json");
            ApiContainer.JsonContent = _content;
            return this;
        }


        // Formvariables
        public FluentApiRunner AddParam(string name, string value)
        {
            ApiContainer.FormFieldsIn.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }

        public FluentApiRunner ClearParams()
        {
            ApiContainer.FormFieldsIn.Clear();
            return this;
        }

        public FluentApiRunner AddAutenticationHeader(string name, string value)
        {
            ApiContainer.HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(name, value);

            ApiContainer.AuthenticationHeaders.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }

        public FluentApiRunner AddHeader(string name, string value)
        {
            ApiContainer.Headers.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }

        public FluentApiRunner Post()
        {
            return RunQuery(HttpMethod.Post);           
        }

        public FluentApiRunner Get()
        {
            return RunQuery(HttpMethod.Get);
        }

        public FluentApiRunner Put()
        {
            return RunQuery(HttpMethod.Put);
        }

        public FluentApiRunner Delete()
        {
            return RunQuery(HttpMethod.Delete);
        }

        private FluentApiRunner RunQuery(HttpMethod httpMethod)
        {
            ApiContainer.HttpClient.BaseAddress = new Uri(ApiContainer.Server);

            if (httpMethod == HttpMethod.Get)
            {                
                var parampart = "?";
                var delimiter = "";

                if (ApiContainer.Uri.Contains("?"))
                    parampart = "";

                if (ApiContainer.Uri.Contains("&"))
                    delimiter = "&";

                foreach (KeyValuePair<string, string> parm in ApiContainer.FormFieldsIn)
                {
                    parampart = parampart + delimiter + parm.Key + "=" + parm.Value;
                    delimiter = "&";
                }
                ApiContainer.Uri = ApiContainer.Uri + parampart;

                ApiContainer.FormFieldsIn.Clear();                
            }

            ApiContainer.HttpRequestMessage = new HttpRequestMessage(httpMethod, ApiContainer.Uri);
            if (!String.IsNullOrEmpty(ApiContainer.CookiesAsString))
                ApiContainer.HttpRequestMessage.Headers.Add("Cookie", ApiContainer.CookiesAsString);
            ApiContainer.Method = httpMethod.Method.ToString();

            
            if (JsonContent == null)
            {
                if (httpMethod != HttpMethod.Get)
                    ApiContainer.HttpRequestMessage.Content = new FormUrlEncodedContent(ApiContainer.FormFieldsIn);
            }
            else
            {
                ApiContainer.HttpRequestMessage.Content = JsonContent;
            }
            ApiContainer.Response = ApiContainer.HttpClient.SendAsync(ApiContainer.HttpRequestMessage).Result;
            ApiContainer.Contents = ApiContainer.Response.Content.ReadAsStringAsync().Result;

            GetCookies();
            GetAntiForgeryToken();

            return this;
        }        

        public FluentApiRunner ProcessIdentityserverResults()
        {
            ApiContainer.AccessToken = "";
            ApiContainer.ExpiresIn = "";
            ApiContainer.TokenType = "";
            ApiContainer.RefreshToken = "";
            ApiContainer.Role = "";

            try
            {
                dynamic obj =
                    JsonConvert.DeserializeObject<ExpandoObject>(ApiContainer.Contents, new ExpandoObjectConverter());

                ApiContainer.AccessToken = obj.access_token.ToString();
                ApiContainer.ExpiresIn = obj.expires_in.ToString();
                ApiContainer.TokenType = obj.token_type.ToString();
                ApiContainer.RefreshToken = obj.refresh_token.ToString();                
            }
            catch (Exception)
            {
                ApiContainer.AccessToken = "";
                ApiContainer.ExpiresIn = "";
                ApiContainer.TokenType = "";
                ApiContainer.RefreshToken = "";
            }

            try
            {
                var splittedToken = ApiContainer.AccessToken.Split('.');
                var part2String = Base64Decode(splittedToken[1] + "=");

                dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(part2String, new ExpandoObjectConverter());
                ApiContainer.Role = obj2.role.ToString();
            }
            catch (Exception )
            {
                ApiContainer.Role = "";
            }

            return this;
        }

        private void GetCookies()
        {
            // Get cookies from response 2                
            IEnumerable<string> values;
            if (ApiContainer.Response.Headers.TryGetValues("Set-Cookie", out values))
            {
                var cookies = new List<string>();
                foreach (var value in values)
                {
                    var nameValue = value.Split(';')[0];
                    var parts = nameValue.Split('=');
                    if (string.IsNullOrWhiteSpace(parts[1])) continue;
                    cookies.Add(nameValue);
                    ApiContainer.Cookies.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
                }
                ApiContainer.CookiesAsString = string.Join("; ", cookies.ToArray());
            }
        }

        public FluentApiRunner SetCookies(string cookiesAsString)
        {            
            ApiContainer.CookiesAsString = cookiesAsString;
            return this;
        }


        private void GetAntiForgeryToken()
        {
            ApiContainer.AntiForgeryToken = Between(ApiContainer.Contents,
                "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"",
                "\" />");
        }

        // Håndter headers
        public string Base64Decode(string base64EncodedData)
        {
            char[] data = base64EncodedData.ToCharArray();
            Base64Decoder myDecoder = new Base64Decoder(data);
            StringBuilder sb = new StringBuilder();

            byte[] temp = myDecoder.GetDecoded();
            sb.Append(Encoding.UTF8.GetChars(temp));

            string result = sb.ToString();

            return result;
        }

        public static string CreateRandomPassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        static string Between(String source, string left, string right)
        {
            return Regex.Match(
                    source,
                    string.Format("{0}(.*){1}", left, right))
                .Groups[1].Value;
        }

        public string GetResetPasswordCode()
        {
            try
            {
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(ApiContainer.Contents, new ExpandoObjectConverter());
                string resetUrl = obj.returnUrl.ToString();

                Uri myUri = new Uri(resetUrl);
                string resetCode = HttpUtility.ParseQueryString(myUri.Query).Get("code");
                return resetCode;
            }
            catch (Exception)
            {
                return "";
            }            
        }

        public string GetResetPasswordUrl()
        {
            try
            {
                dynamic obj = JsonConvert.DeserializeObject<dynamic>(ApiContainer.Contents, new ExpandoObjectConverter());
                string resetUrl = obj.returnUrl.ToString();
                return resetUrl;
            }
            catch (Exception)
            {
                return "";
            }
        }

    }
}
