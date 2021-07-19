using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Configuration;
using System.Collections.Specialized;

namespace IntaroExampleSecond
{
    class TelegramBot
    {
        string token = ConfigurationManager.AppSettings.Get("TelegramApiToken");
        private static TelegramBotClient client;
        public TelegramBot()
        {
            client = new TelegramBotClient(token);
            List<BotCommand> commands = new List<BotCommand>();
            commands.Add(
                new BotCommand()
                {
                    Command = "/info",
                    Description = "Информация о боте"
                }
            );
            commands.Add(
                new BotCommand()
                {
                    Command = "/subscribe",
                    Description = "Подписаться на репозиторий"
                }
            );
            commands.Add(
                new BotCommand()
                {
                    Command = "/unsubscribe",
                    Description = "Отписаться от репозитория"
                }
            );
            commands.Add(
                new BotCommand()
                {
                    Command = "/news",
                    Description = "Новости о избранных репозиториях"
                }
            );
            commands.Add(
                new BotCommand()
                {
                    Command = "/recommendations",
                    Description = "Рекомендованные репозитории"
                }
            );
            commands.Add(
                new BotCommand()
                {
                    Command = "/list",
                    Description = "Ваши репозитории"
                }
            );
            client = new TelegramBotClient(token);
            client.SetMyCommandsAsync(commands);
            client.OnMessage += BotOnMessageReceived;
            client.OnMessageEdited += BotOnMessageReceived;
        }


        public void Start()
        {
           client.StartReceiving();
        }

        public void Stop()
        {
            client.StopReceiving();
        }


        public static async void SendURLError(string ChatId) // отправляет сообщение об ошибке - ошибка в ссылке
        {
            await client.SendTextMessageAsync(ChatId, "Ошибка в URL!");
        }

        public static async void SendRepoError(string ChatId) // отправляет сообщение об ошибке - несуществующий репозиторий 
        {
            await client.SendTextMessageAsync(ChatId, "Такого репозитория не существует!"); 
        }

        public static async void SendSubscribeError(string ChatId) // отправляет сообщение об ошибке - пользователь уже подписан на репозиторий
        {
            await client.SendTextMessageAsync(ChatId, "Вы уже подписаны на данный репозиторий!"); 
        }

        public static async void SendSubscribeSuccess(string ChatId) // отправляет сообщение об успехе - пользователь подписался на репозиторий
        {
            await client.SendTextMessageAsync(ChatId, "Вы подписались на данный репозиторий!");
        }

        public static async void SendUnsubscribeError(string ChatId) // отправляет сообщение об ошибке - пользователь НЕ подписан на репозиторий
        {
            await client.SendTextMessageAsync(ChatId, "Вы не подписаны на данный репозиторий!");
        }

        public static async void SendUnsubscribeSuccess(string ChatId) // отправляет сообщение об успехе - пользователь отписался от репозитория
        {
            await client.SendTextMessageAsync(ChatId, "Вы отписались от данного репозитория!");
        }


        public static async void SendMessage(string ChatId, string text) // позволяет отправить произвольное сообщение
        {
            await client.SendTextMessageAsync(ChatId, text);
        }

        async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs) // метод, обрабатывающий получаемые сообщения
        {
            var message = messageEventArgs.Message;
            if (message?.Type == MessageType.Text)
            {
                string request = message.Text;
                string text = "";
                if (message.Text.Contains(' ')) // если сообщение состоит больше чем из одного слова
                {
                    request = message.Text.Remove(message.Text.IndexOf(' '));
                    text = message.Text.Remove(0, message.Text.IndexOf(' ') + 1);
                }
                switch (request)
                {
                    case "/info":
                        await client.SendTextMessageAsync(message.Chat.Id, @"Привет!
Я бот для работы с репозиториями с Github.
Список команд:
/subscribe Ссылка.на.репозиторий - Подписаться на данный репозиторий.
/unsubscribe Ссылка.на.репозиторий - Отписаться от данного репозитория.
/news - Новости из ваших репозиториев.
/list - Список ваших репозиториев.
/recommendations - Рекомендованные вам репозитории.");
                        break;
                    case "/subscribe":
                        GithubConnection.Subscribe(text, message.Chat.Id.ToString());
                        break;
                    case "/unsubscribe":
                        GithubConnection.Unsubscribe(text, message.Chat.Id.ToString());
                        break;
                    case "/list":
                        List<string> list = new List<string>();
                        list = DatabaseConnection.ListOfSubscriptions(message.Chat.Id.ToString());
                        string MessageText = "Ваши репозитории:";
                        foreach (var item in list)
                        {
                            MessageText += "\n" + item;
                        }
                        await client.SendTextMessageAsync(message.Chat.Id, MessageText);
                        break;
                    case "/news":
                        GithubConnection.news(message.Chat.Id.ToString());
                        break;
                    case "/recommendations":
                        await client.SendTextMessageAsync(message.Chat.Id, "Подождите, это может занять немного времени...");
                        GithubConnection.recommendations(message.Chat.Id.ToString());
                        break;
                    default:
                        await client.SendTextMessageAsync(message.Chat.Id, "Бот ничего не понял...");
                        break;
                }
            }
        }

    }
}
