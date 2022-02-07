using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DaybreakGames.Census.Operators
{
    public sealed class CensusQuery : CensusOperator
    {
        private readonly ICensusClient _censusClient;

        public string ServiceName { get; private set; }

        public string ServiceId { get; private set; }
        public string ServiceNamespace { get; private set; }

        private List<CensusArgument> Terms { get; set; }

        [UriQueryProperty]
        public bool ExactMatchFirst { get; set; } = false;

        [UriQueryProperty]
        private bool Timing { get; set; } = false;

        [UriQueryProperty]
        private bool IncludeNull { get; set; } = false;

        [DefaultValue(true)]
        [UriQueryProperty]
        private bool Case { get; set; } = true;

        [DefaultValue(true)]
        [UriQueryProperty]
        private bool Retry { get; set; } = true;

        [UriQueryProperty]
        public int? Limit { get; private set; } = null;

        [UriQueryProperty("limitPerDB")]
        private int? LimitPerDB { get; set; } = null;

        [UriQueryProperty]
        public int? Start { get; private set; }

        [UriQueryProperty]
        private List<string> Show { get; set; }

        [UriQueryProperty]
        private List<string> Hide { get; set; }

        [UriQueryProperty]
        private List<string> Sort { get; set; }

        [UriQueryProperty]
        private List<string> Has { get; set; }

        [UriQueryProperty]
        private List<string> Resolve { get; set; }

        [UriQueryProperty]
        private List<CensusJoin> Join { get; set; }

        [UriQueryProperty]
        private List<CensusTree> Tree { get; set; }

        [UriQueryProperty]
        private string Distinct { get; set; }

        [UriQueryProperty("lang")]
        public string Language { get; private set; }

        public CensusQuery(ICensusClient censusClient, string serviceName)
        {
            _censusClient = censusClient;

            ServiceName = serviceName;
        }

        public CensusQuery SetServiceId(string serviceId)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                throw new ArgumentNullException(nameof(serviceId));
            }

            ServiceId = serviceId;

            return this;
        }

        public CensusQuery SetServiceNamespace(string serviceNamespace)
        {
            if (string.IsNullOrWhiteSpace(serviceNamespace))
            {
                throw new ArgumentNullException(nameof(serviceNamespace));
            }

            ServiceNamespace = serviceNamespace;

            return this;
        }

        public CensusJoin JoinService(string service)
        {
            var newJoin = new CensusJoin(service);

            if (Join == null)
            {
                Join = new List<CensusJoin>();
            }

            Join.Add(newJoin);
            return newJoin;
        }

        public CensusTree TreeField(string field)
        {
            var newTree = new CensusTree(field);

            if (Tree == null)
            {
                Tree = new List<CensusTree>();
            }

            Tree.Add(newTree);
            return newTree;
        }

        public CensusOperand Where(string field)
        {
            var newArg = new CensusArgument(field);

            if (Terms == null)
            {
                Terms = new List<CensusArgument>();
            }

            Terms.Add(newArg);
            return newArg.Operand;
        }

        public CensusQuery ShowFields(params string[] fields)
        {
            if (Show == null)
            {
                Show = new List<string>();
            }

            Show.AddRange(fields);

            return this;
        }

        public CensusQuery HideFields(params string[] fields)
        {
            if (Hide == null)
            {
                Hide = new List<string>();
            }

            Hide.AddRange(fields);

            return this;
        }

        public CensusQuery SetLimit(int limit)
        {
            Limit = limit;

            return this;
        }

        public CensusQuery SetStart(int start)
        {
            Start = start;

            return this;
        }

        public CensusQuery AddResolve(params string[] resolves)
        {
            if (Resolve == null)
            {
                Resolve = new List<string>();
            }

            Resolve.AddRange(resolves);

            return this;
        }

        public CensusQuery SetLanguage(CensusLanguage language)
        {
            switch(language)
            {
                case CensusLanguage.English:
                    SetLanguage("en");
                    break;
                case CensusLanguage.German:
                    SetLanguage("de");
                    break;
                case CensusLanguage.Spanish:
                    SetLanguage("es");
                    break;
                case CensusLanguage.French:
                    SetLanguage("fr");
                    break;
                case CensusLanguage.Italian:
                    SetLanguage("it");
                    break;
                case CensusLanguage.Turkish:
                    SetLanguage("tr");
                    break;
            }

            return this;
        }

        public CensusQuery SetLanguage(string language)
        {
            Language = language;

            return this;
        }

        public Task<JToken> GetAsync()
        {
            return GetAsync<JToken>();
        }

        public Task<IEnumerable<JToken>> GetListAsync()
        {
            return GetListAsync<JToken>();
        }

        public Task<IEnumerable<JToken>> GetBatchAsync()
        {
            return GetBatchAsync<JToken>();
        }

        public async Task<T> GetAsync<T>()
        {
            var result = await GetListAsync<T>();
            return result == null ? default(T) : result.FirstOrDefault();
        }

        public Task<IEnumerable<T>> GetListAsync<T>()
        {
            return _censusClient.ExecuteQuery<IEnumerable<T>>(this);
        }

        public Task<IEnumerable<T>> GetBatchAsync<T>()
        {
            return _censusClient.ExecuteQueryBatch<T>(this);
        }

        public Uri GetUri()
        {
            return _censusClient.CreateRequestUri(this);
        }

        public override string ToString()
        {
            var baseString = base.ToString();

            var terms = Terms?.Select(t => t.ToString()).ToList() ?? new List<string>();
            var stringTerms = string.Join(GetPropertySpacer(), terms);


            if (!string.IsNullOrEmpty(baseString))
            {
                baseString = $"?{baseString}";

                if (!string.IsNullOrEmpty(stringTerms))
                {
                    stringTerms = $"&{stringTerms}";
                }
            }
            else if (!string.IsNullOrEmpty(stringTerms))
            {
                stringTerms = $"?{stringTerms}";
            }

            return $"{ServiceName}/{baseString}{stringTerms}";
        }

        public override string GetKeyValueStringFormat()
        {
            return "c:{0}={1}";
        }

        public override string GetPropertySpacer()
        {
            return "&";
        }

        public override string GetTermSpacer()
        {
            return ",";
        }
    }
}
