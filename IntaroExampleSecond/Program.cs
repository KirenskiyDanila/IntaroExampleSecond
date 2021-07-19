using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    class Program
    {
       

        static void Main(string[] args)
        {

            TelegramBot bot = new TelegramBot();
            bot.Start();
            Console.ReadLine();
            bot.Stop();
        }

    }
}
