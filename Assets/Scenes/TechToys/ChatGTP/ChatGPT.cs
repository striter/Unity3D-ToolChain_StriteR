using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace TechToys.ChatGPT
{
    
    public class ChatGPT : MonoBehaviour
    {
        public string m_ApiKey;
        public FRequestBody m_RequestBody;
        [ExtendButton(nameof(SendChat),nameof(SendChat))]
        public string m_UserMessage;

        private ChatGPTConnection connection;
        private CancellationTokenSource token;
        
        public async void SendChat()
        {
            if (connection == null)
                connection = new ChatGPTConnection(m_ApiKey,m_RequestBody);
            FResponseBody result;
            try
            {
                token = new CancellationTokenSource(60 * 1000);
                result = await connection.CreateMessageAsync(m_UserMessage,token.Token);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
                
            Debug.Log(result.ResultMessage);
        }

        private void OnDestroy()
        {
            if (token == null)
                return;
            token.Cancel();
            token = null;
        }
    }

    public enum ERole : byte
    {
        System,
        Assistant,
        User,
    }
    
    [Serializable,JsonObject]
    public struct FRequestBody
    {
        public string model;
        public List<FMessage> messages;
        public float temperature;
        public float top_p;
        public int? n;
        public bool? stream;
        [CanBeNull] public string[] stop;
        public int? maxTokens;
        public float? presense_penalty;
        [CanBeNull] public Dictionary<int, int> logit_bias;
        [CanBeNull] public string user;
        public string Convert() => JsonConvert.SerializeObject(this,Formatting.Indented,new JsonSerializerSettings(){NullValueHandling = NullValueHandling.Ignore});
        public static FRequestBody Convert(string _json) => JsonConvert.DeserializeObject<FRequestBody>(_json);
    }
    
    
    [JsonObject]
    public struct FResponseBody
    {
        public string id;
        [JsonProperty("object")] public string obj;
        public uint created;
        public string model;
        public FUsage usage;
        public FChoice[] choices;
        public string ResultMessage => choices.Length > 0 ? choices[0].message.content : string.Empty;
        public string Convert() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static FResponseBody Convert(string _json) => JsonConvert.DeserializeObject<FResponseBody>(_json);
    }
    
    [JsonObject]
    public struct FErrorResponseBody
    {
        public FError error;
        public string Convert() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static FErrorResponseBody Convert(string _json) => JsonConvert.DeserializeObject<FErrorResponseBody>(_json);
    }
    
    [JsonObject]
    public struct FError
    {
        public string message;
        public string type;
        [CanBeNull] public string param;
        [CanBeNull] public string code;
    }
    
    [JsonObject]
    public struct FUsage
    {
        public uint prompt_tokens;
        public uint completion_tokens;
        public uint total_tokens;
    }
    
    [JsonObject]
    public struct FChoice
    {
        public FMessage message;
        public string finish_reason;
        public uint index;
    }
    
    [JsonObject,Serializable]
    public struct FMessage
    {
        public string role;
        public string content;
    
        public static readonly FMessage kDefault = new FMessage() { role = ERole.Assistant.ToString(), content = string.Empty };
    
        internal FMessage(ERole _role, string _content)
        {
            if (string.IsNullOrEmpty(_content))
                throw new ArgumentNullException();
            role = _role.ToString();
            content = _content;
        }
    }
    
    public class ChatGPTConnection
    {
        private readonly IReadOnlyDictionary<string, string> headers;
        private readonly List<FMessage> messages;
        private readonly FRequestBody requestBody;
        private static readonly HttpClient kHttpClient = new HttpClient();
        
        public ChatGPTConnection(string _apiKey, FRequestBody _requestBody)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new ArgumentNullException(nameof(_apiKey));
    
            this.headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {_apiKey}", };
            this.requestBody = _requestBody;
            this.messages = this.requestBody.messages;
        }
    
        static HttpRequestMessage CreateRequestMessage(IReadOnlyDictionary<string, string> headers, FRequestBody _body)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            foreach (var header in headers)
                requestMessage.Headers.Add(header.Key,header.Value);
    
            var requestContent = new StringContent(content: _body.Convert(),
                encoding: System.Text.Encoding.UTF8,
                mediaType: "application/json");
            requestMessage.Content = requestContent;
            return requestMessage;
        }
        public async Task<FResponseBody> CreateMessageAsync(string _content,CancellationToken _cancellationToken)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            messages.Add(new FMessage(ERole.User,_content));
    
            using var responseMessage = await kHttpClient.SendAsync(CreateRequestMessage(headers, requestBody), _cancellationToken);
            if (responseMessage == null)
                throw new Exception("[ChatGPT] Response is null");
    
            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseJson))
                throw new Exception("[ChatGPT] Response JSON is null or empty");
    
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseBody = FResponseBody.Convert(responseJson);
                if (responseBody.choices.Length == 0)
                    throw new Exception("[ChatGPT] Response Body is null");
    
                if (responseBody.choices.Length == 0)
                    throw new Exception($"[ChatGPT] Not found any choices in response {responseJson}");
                
                messages.Add(responseBody.choices[0].message);
                return responseBody;
            }
    
            switch ((int) responseMessage.StatusCode)
            {
                //Is API Error
                case >= 400 and <= 499:
                {
                    var errorResponse = FErrorResponseBody.Convert(responseJson);
                    throw new Exception($"[ChatGPT] Error Response {TDataConvert.Convert(errorResponse)}") ;
                }
                default:
                    responseMessage.EnsureSuccessStatusCode();
                    throw new Exception($"[ChatGPT] It should not be reached with status:{responseMessage.StatusCode}");
            }
        }
    }
}