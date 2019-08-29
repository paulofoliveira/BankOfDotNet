using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BankOfDotNet.ConsoleClient
{
    class Program
    {
        private readonly static string[] _authorizedOptions = new string[] { "1", "2", "s" };
        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            Console.Write("Entre com o tipo de Flows ..:\n1 - Client Credentials\n2 - Resource Owner Client\n\n ");
            Console.Write("Resposta ..: ");
            var option = Console.ReadLine();

            if (!_authorizedOptions.Contains(option.ToLower())) return;

            var tokenResponse = await GetTokenResponseByOptionAsync(option);

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // Consumir a API:

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var customerInfo = new StringContent(JsonConvert.SerializeObject(new { Id = 10, FirstName = "Paulo", LastName = "Silva" }), Encoding.UTF8, "application/json");

            var createCustomerResponse = await client.PostAsync("http://localhost:61807/api/customers", customerInfo);

            if (!createCustomerResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(createCustomerResponse.StatusCode);
            }
            else
            {
                var getCustomerResponse = await client.GetAsync("http://localhost:61807/api/customers");

                if (!getCustomerResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine(getCustomerResponse.StatusCode);
                }
                else
                {
                    var content = await getCustomerResponse.Content.ReadAsStringAsync();
                    Console.WriteLine(JArray.Parse(content));
                }
            }

            Console.ReadKey();
        }

        private static async Task<TokenResponse> GetTokenResponseByOptionAsync(string option)
        {
            // Descobrir os endpoints usando metadados do Identity Server:

            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return null;
            }

            // Pegar o token:

            var client = GetClientBySelectedOption(option);
            var tokenClient = new TokenClient(disco.TokenEndpoint, client, "secret");
            var scope = "bankOfDotNetApi";

            if (option.Equals("1"))
            {
                Console.WriteLine("Executando o flow Client Credentials...");
                return await tokenClient.RequestClientCredentialsAsync(scope);
            }
            else if (option.Equals("2"))
            {
                Console.Write("Entre com o userName ..: ");
                var userName = Console.ReadLine();

                Console.Write("Entre com o password ..: ");
                var password = Console.ReadLine();
                Console.WriteLine("Executando o flow Request Resource Owner Password...");
                return await tokenClient.RequestResourceOwnerPasswordAsync(userName, password, scope);
            }

            return null;
        }

        private static string GetClientBySelectedOption(string option)
        {
            if (option == "1") return "client";
            if (option == "2") return "ro.client";

            throw new Exception("Option not found.");
        }
    }
}
