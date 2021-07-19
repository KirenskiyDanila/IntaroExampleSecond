using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Types;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Configuration;

namespace IntaroExampleSecond
{
    class GithubConnection
    {
        private static string token = ConfigurationManager.AppSettings.Get("GithubApiToken");


        public static void Unsubscribe(string URL, string ChatId) // метод, который отписывает от репозитория
        {

            string pattern = "^https:\\/\\/github.com\\/[^\\/]+\\/[^\\/]+$";

            if (!Regex.Match(URL, pattern).Success)
            {
                TelegramBot.SendURLError(ChatId);
                return;
            }

            string fixedURL = URL.Remove(0, URL.IndexOf('m') + 2);

            string name = fixedURL.Remove(0, fixedURL.IndexOf('/') + 1);

            string owner = fixedURL.Remove(fixedURL.IndexOf('/'));

            if (!DatabaseConnection.DeleteSubscription(ChatId, name, owner))
            {
                TelegramBot.SendUnsubscribeError(ChatId);
                return;
            }

            else
            {
                TelegramBot.SendUnsubscribeSuccess(ChatId);
                return;
            }

        }

        public static async void Subscribe(string URL, string ChatId) // метод, который подписывает на репозиторий
        {

            string pattern = "^https:\\/\\/github.com\\/[^\\/]+\\/[^\\/]+$";

            if (!Regex.Match(URL, pattern).Success)
            {
                TelegramBot.SendURLError(ChatId);
                return;
            }

            string fixedURL = URL.Remove(0, URL.IndexOf('m') + 2);

            string name = fixedURL.Remove(0, fixedURL.IndexOf('/') + 1);

            string owner = fixedURL.Remove(fixedURL.IndexOf('/'));

            var queryObject = new
            {
                query = @"{
   repository (name:" + "\u0022" + name + "\u0022 " + "owner: " + "\u0022" + owner + "\u0022" + @") {
     updatedAt
        }
    }",
                variables = new { }
            };

            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com/graphql")
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", "MyConsoleApp");

            string basicValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"KirenskiyDanila:{token}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicValue);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(queryObject), Encoding.UTF8, "application/json")
            };

            dynamic responseObj;

            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
            }

            if (responseObj.errors != null)
            {
                TelegramBot.SendRepoError(ChatId);
                return;
            }

            if (!DatabaseConnection.AddSubscription(ChatId, name, owner))
            {
                TelegramBot.SendSubscribeError(ChatId);
                return;
            }

            else
            {
                TelegramBot.SendSubscribeSuccess(ChatId);
                return;
            }

        }


        public static async void recommendations(string ChatId) // метод, который отсылает рекомендации пользователю
        {
            List<string> list = new List<string>();

            string MessageText = "Рекомендованные вам репозитории:\n\n";

            list = DatabaseConnection.ListOfSubscriptions(ChatId);

            bool empty = true;

            foreach (var URL in list)
            {

                string fixedURL = URL.Remove(0, URL.IndexOf('m') + 2);

                string owner = fixedURL.Remove(fixedURL.IndexOf('/'));

                

                var queryObject = new
                {
                    query = @"{ 
                            organization(login: " + "\u0022" + owner +"\u0022" +@" ) {
                                 name
                                 url
                                 repositories(first: 1, orderBy: { field: STARGAZERS, direction: DESC}) {
                                    edges {
                                        node {
                                             name
                                             url
                                             description
                        }
                    }
                }
            }
        }",
                    variables = new { }
                };

                HttpClient httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.github.com/graphql")
                };

                httpClient.DefaultRequestHeaders.Add("User-Agent", "MyConsoleApp");

                string basicValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"KirenskiyDanila:{token}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicValue);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(queryObject), Encoding.UTF8, "application/json")
                };

                dynamic responseObj;

                using (var response = await httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                }

                if (responseObj.errors == null)
                {
                    string name = responseObj.data.organization.repositories.edges[0].node.name;
                    if (!DatabaseConnection.Check(ChatId, name, owner))
                    {
                        MessageText += "Организация: " + owner + "\nНазвание репозитория: " + name +
                            "\nОписание: " + responseObj.data.organization.repositories.edges[0].node.description + 
                            "\nСсылка: " + responseObj.data.organization.repositories.edges[0].node.url + "\n\n\n";
                        empty = false;
                    }
                }

            }

            if (empty) MessageText = "К сожалению, для вас нет рекомендаций.\nДобавьте больше репозиториев.";

            TelegramBot.SendMessage(ChatId, MessageText);
        }


        public static async void news(string ChatId) // метод, который отсылает новости о коммитах пользователю
        {
            List<string> list = new List<string>();

            string MessageText = "Обновления в репозиториях:\n";

            list = DatabaseConnection.ListOfSubscriptions(ChatId);

            bool empty = true;

            foreach (var URL in list)
            {

                string fixedURL = URL.Remove(0, URL.IndexOf('m') + 2);

                string name = fixedURL.Remove(0, fixedURL.IndexOf('/') + 1);

                string owner = fixedURL.Remove(fixedURL.IndexOf('/'));
                
                var queryObject = new
                {
                    query = @"{
   repository (name:" + "\u0022" + name + "\u0022 " + "owner: " + "\u0022" + owner + "\u0022" + @") {
    defaultBranchRef {
                target  {
                    ... on Commit  {
                        history {
                            edges {
                                node {
                                    commitUrl           
                                    committedDate
                                }
                            }
                        }
                    }
                }
            }
        }
    }",
                    variables = new { }
                };

                HttpClient httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://api.github.com/graphql")
                };

                httpClient.DefaultRequestHeaders.Add("User-Agent", "MyConsoleApp");

                string basicValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"KirenskiyDanila:{token}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicValue);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(queryObject), Encoding.UTF8, "application/json")
                };

                dynamic responseObj;

                using (var response = await httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    responseObj = JsonConvert.DeserializeObject<dynamic>(responseString);
                }

                dynamic commits = responseObj.data.repository.defaultBranchRef.target.history.edges;

                var commitURL = commits[0].node.commitUrl.ToString();

                string commitDate = commits[0].node.committedDate.ToString();

                string TodayDate = DateTime.Today.ToString().Remove(10); // актуальная дата

                string YesterdayDate = DateTime.Today.AddDays(-1).ToString().Remove(10); // вчерашняя дата

                if ((commitDate.Remove(10) == TodayDate) || (commitDate.Remove(10) == YesterdayDate)) // если последний коммит произошел сегодня или вчера
                {
                    MessageText += "\n" + "Репозиторий:" + URL + "\n" + "Был обновлен:" + commitDate + "\n" + "Ссылка на коммит репозитория:" + commitURL + "\n\n\n";
                    empty = false;
                }


            }
            if (empty) MessageText = "Коммитов в репозитории за последние два дня не наблюдается.";
            TelegramBot.SendMessage(ChatId, MessageText);
        }

    }
}
