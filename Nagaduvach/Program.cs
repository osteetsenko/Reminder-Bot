using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Nagaduvach
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = UTF8Encoding.UTF8;
            var botClient = new TelegramBotClient("5587486227:AAFWXMINIFJZpJ2zIi81DtspmRtfVnXOLjY");
            var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            /// <summary>
            /// Инициализация комманд бота
            /// </summary>
            List<BotCommand> commands = new List<BotCommand>
            {
                new BotCommand { Command = "info", Description = "Інформація про роботу з ботом" }
            };
            await botClient.SetMyCommandsAsync(commands);

            

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");

            while (true)
            {
                Console.WriteLine("Очікування повідомлень");
                Thread.Sleep(60000);
                Reminders reminders = new Reminders();
                reminders = reminders.DeSerializeReminders();
                foreach(Reminder reminder in reminders.Reminderlist)
                {
                    if ((reminder.Datetime < DateTime.Now) && (reminder.IsActive))
                    {
                        Console.WriteLine("Відправка нагадування");
                        string txt = $"{reminder.Datetime:dd.MM.yyyy HH:mm}:\n{reminder.Text}";
                        await botClient.SendTextMessageAsync(chatId: reminder.UserID, text: txt);
                        if (reminder.Repeat == "Без повторень")
                        {
                            reminder.OnOff();
                            reminders.SerializeReminders();
                        }
                        else if(reminder.Repeat == "Кожен рік")
                        {
                            DateTime datetime = new DateTime();
                            datetime = reminder.Datetime;
                            reminder.Datetime = datetime.AddYears(1);
                            reminders.SerializeReminders();
                        }
                        else if (reminder.Repeat == "Кожен місяць")
                        {
                            DateTime datetime = new DateTime();
                            datetime = reminder.Datetime;
                            reminder.Datetime = datetime.AddMonths(1);
                            reminders.SerializeReminders();
                        }
                    }
                }
            }
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string ErrorMessage = exception.Message;
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;
            // Only process text messages
            if (update.Message.Type != MessageType.Text)
                return;
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;
            var username = update.Message.Chat.Username;

            UserStates userstates = new UserStates();
            userstates = userstates.DeSerializeStates();
            long userstate = userstates.GetUserState(chatId);

            Reminders reminders = new Reminders();
            reminders = reminders.DeSerializeReminders();

            Notes notes = new Notes();
            notes = notes.DeSerializeNotes();

            List<KeyboardButton> keyboard0 = new List<KeyboardButton>()
            {
                new KeyboardButton("Нагадування"),
                new KeyboardButton("Нотатки"),
                new KeyboardButton("Інфо")
            };
            ReplyKeyboardMarkup ReplyKeyboard0 = new ReplyKeyboardMarkup(keyboard0)
            {
                ResizeKeyboard = true
            };

            List<KeyboardButton>[] keyboard10 = new List<KeyboardButton>[]
            {
                new List<KeyboardButton> { new KeyboardButton("Створити нове нагадування") },
                new List<KeyboardButton> { new KeyboardButton("Видалити нагадування") },
                new List<KeyboardButton> { new KeyboardButton("Вимкнути/Ввімкнути нагадування") },
                new List<KeyboardButton> { new KeyboardButton("Відкласти на 15 хвилин") },
                new List<KeyboardButton> { new KeyboardButton("Нотатки"), new KeyboardButton("Назад") }
            };
            ReplyKeyboardMarkup ReplyKeyboard10 = new ReplyKeyboardMarkup(keyboard10)
            {
                ResizeKeyboard = true
            };

            List<KeyboardButton> keyboard11 = new List<KeyboardButton>()
            {
                new KeyboardButton("Відміна")
            };
            ReplyKeyboardMarkup ReplyKeyboard11 = new ReplyKeyboardMarkup(keyboard11)
            {
                ResizeKeyboard = true
            };

            List<KeyboardButton> keyboard12 = new List<KeyboardButton>()
            {
                new KeyboardButton("Відміна"),
                new KeyboardButton("Назад")
            };
            ReplyKeyboardMarkup ReplyKeyboard12 = new ReplyKeyboardMarkup(keyboard12)
            {
                ResizeKeyboard = true
            };

            List<KeyboardButton>[] keyboard13 = new List<KeyboardButton>[]
            {
                new List<KeyboardButton> { new KeyboardButton("Кожен місяць") },
                new List<KeyboardButton> { new KeyboardButton("Кожен рік") },
                new List<KeyboardButton> { new KeyboardButton("Без повторень") },
                new List<KeyboardButton> { new KeyboardButton("Назад"), new KeyboardButton("Відміна") }
            };
            ReplyKeyboardMarkup ReplyKeyboard13 = new ReplyKeyboardMarkup(keyboard13)
            {
                ResizeKeyboard = true
            };

            List<KeyboardButton>[] keyboard20 = new List<KeyboardButton>[]
            {
                new List<KeyboardButton> { new KeyboardButton("Створити нову нотатку") },
                new List<KeyboardButton> { new KeyboardButton("Видалити нотатку") },
                new List<KeyboardButton> { new KeyboardButton("Нагадування"), new KeyboardButton("Назад") }
            };
            ReplyKeyboardMarkup ReplyKeyboard20 = new ReplyKeyboardMarkup(keyboard20)
            {
                ResizeKeyboard = true
            };

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId} from {username}");
            if (messageText == "/start")
            {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Привіт, я бот для нагадувань.\nДля отримання информації про мій функціонал відправте Інфо або /info", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard0);
                return;
            }
            if (messageText == "/info" || messageText == "Інфо")
            {
                string help = "Для створення, перегляду, та роботи з нагадуваннями відправте:\nНагадування або /reminders\n";
                help += "Для створення, перегляду, та роботи з нотатками відправте:\nНотатки або /notes\n";
                await botClient.SendTextMessageAsync(chatId: chatId, text: help, cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard0);
                return;
            }
            Reminder newreminder = new Reminder();
            
            //початкове меню
            if (userstate == 0) 
            {
                if (messageText == "Нагадування")
                {
                    userstates.SetUserState(chatId, 10);
                    userstates.SerializeStates();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                    int i = 0;
                    foreach (Reminder reminder in reminders.Reminderlist)
                    {
                        if (reminder.UserID == chatId)
                        {
                            string userreminders = $"Нагадування {++i}:\n"+$"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                            await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                        }
                    }
                    return;
                }
                if (messageText == "Нотатки")
                {
                    userstates.SetUserState(chatId, 20);
                    userstates.SerializeStates();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатки:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard20);
                    int i = 0;
                    foreach (Note note in notes.Notelist)
                    {
                        if (note.UserID == chatId)
                        {
                            string usernotes = $"Нотатка {++i}:\n{note.Text}";
                            await botClient.SendTextMessageAsync(chatId: chatId, text: usernotes, cancellationToken: cancellationToken);
                        }
                    }
                    return;
                }
            }
            //для створення нагадувань
            switch (userstate) 
            {
                case 10:
                    {
                        if (messageText == "Назад")
                        {
                            userstates.SetUserState(chatId, 0);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Головне меню:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard0);
                            return;
                        }
                        if(messageText == "Нотатки")
                        {
                            userstates.SetUserState(chatId, 20);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатки:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard20);
                            int i = 0;
                            foreach (Note note in notes.Notelist)
                            {
                                if (note.UserID == chatId)
                                {
                                    string usernotes = $"Нотатка {++i}:\n{note.Text}";
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: usernotes, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (messageText == "Створити нове нагадування")
                        {
                            userstates.SetUserState(chatId, 11);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть текст нагадування", cancellationToken: cancellationToken, replyMarkup:ReplyKeyboard11);
                            return;
                        }
                        if (messageText== "Видалити нагадування")
                        {
                            userstates.SetUserState(chatId, 110);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть яке з нагадувань видалити:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                            return;
                        }
                        if (messageText == "Вимкнути/Ввімкнути нагадування")
                        {
                            userstates.SetUserState(chatId, 120);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть яке з нагадувань вимкнути/ввімкнути:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                            return;
                        }
                        if (messageText == "Відкласти на 15 хвилин")
                        {
                            userstates.SetUserState(chatId, 130);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть яке з нагадувань відкласти:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                            return;
                        }
                        break;
                    }
                case 11:
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                        $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                        (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        newreminder.UserID = chatId;
                        newreminder.Text = messageText;
                        newreminder.SerializeReminder(chatId);
                        userstates.SetUserState(chatId, 12);
                        userstates.SerializeStates();
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть дату та час нагадування в форматі дд.мм.рррр гг.хх", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard12);
                        return;
                    }
                case 12:
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                        $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                        (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (messageText == "Назад")
                        {
                            userstates.SetUserState(chatId, 11);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть текст нагадування", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                            return;
                        }
                        Regex regex = new Regex(@"\d{2}\.\d{2}\.\d{4}\s\d{2}\.\d{2}");
                        if (regex.IsMatch(messageText))
                        {
                            newreminder = newreminder.DeSerializeReminder(chatId);
                            newreminder.Datetime = DateTime.ParseExact(messageText, "dd.MM.yyyy HH.mm", null);
                            newreminder.SerializeReminder(chatId);
                            userstates.SetUserState(chatId, 13);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Повторювання?", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard13);
                            return;
                        }
                        break;
                    }
                case 13:
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                        $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                        (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (messageText == "Назад")
                        {
                            userstates.SetUserState(chatId, 12);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть дату та час нагадування в форматі дд.мм.рррр гг.хх", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard12);
                            return;
                        }
                        if (messageText == "Кожен місяць")
                        {
                            newreminder = newreminder.DeSerializeReminder(chatId);
                            newreminder.Repeat = messageText;
                            reminders.Reminderlist.Add(newreminder);
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування додано", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminde in reminders.Reminderlist)
                            {
                                if (reminde.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminde.Text}\nКоли зробити:\n" +
                                        $"{reminde.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminde.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminde.Repeat) +
                                        (reminde.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            reminders.SerializeReminders();
                            return;
                        }
                        if (messageText == "Кожен рік")
                        {
                            newreminder = newreminder.DeSerializeReminder(chatId);
                            newreminder.Repeat = messageText;
                            reminders.Reminderlist.Add(newreminder);
                            
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування додано", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminde in reminders.Reminderlist)
                            {
                                if (reminde.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminde.Text}\nКоли зробити:\n" +
                                        $"{reminde.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminde.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminde.Repeat) +
                                        (reminde.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            reminders.SerializeReminders();
                            return;
                        }
                        if (messageText == "Без повторень")
                        {
                            newreminder = newreminder.DeSerializeReminder(chatId);
                            newreminder.Repeat = messageText;
                            reminders.Reminderlist.Add(newreminder);
                            
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування додано", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminde in reminders.Reminderlist)
                            {
                                if (reminde.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminde.Text}\nКоли зробити:\n" +
                                        $"{reminde.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminde.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminde.Repeat) +
                                        (reminde.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            reminders.SerializeReminders();
                            return;
                        }
                        break;
                    }
            }
            //для видалення, відключення або відкладування нагадувань
            switch (userstate)
            {
                case 110: //видалення
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                        $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                        (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (int.TryParse(messageText, out int num))
                        {
                            int count = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    count++;
                                    if (num == count)
                                    {
                                        reminders.Reminderlist.Remove(reminder);
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування видалено", cancellationToken: cancellationToken);
                                        userstates.SetUserState(chatId, 10);
                                        userstates.SerializeStates();
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                                        int i = 0;
                                        foreach (Reminder reminde in reminders.Reminderlist)
                                        {
                                            if (reminde.UserID == chatId)
                                            {
                                                string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminde.Text}\nКоли зробити:\n" +
                                                    $"{reminde.Datetime:dd.MM.yyyy HH:mm}\n" +
                                                    (reminde.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminde.Repeat) +
                                                    (reminde.IsActive ? "" : "\nНагадування вимкнено.");
                                                await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                            }
                                        }
                                        reminders.SerializeReminders();
                                        return;
                                    }
                                }
                            }
                            if (num > count)
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування не існує, введіть знову", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                                return;
                            }
                        }
                        break;
                    }
                case 120: //ввімкнення/вимкнення
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                        $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                        (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (int.TryParse(messageText, out int num))
                        {
                            int count = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    count++;
                                    if (num == count)
                                    {
                                        reminder.OnOff();
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Готово.", cancellationToken: cancellationToken);
                                        userstates.SetUserState(chatId, 10);
                                        userstates.SerializeStates();
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                                        int i = 0;
                                        foreach (Reminder reminde in reminders.Reminderlist)
                                        {
                                            if (reminde.UserID == chatId)
                                            {
                                                string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminde.Text}\nКоли зробити:\n" +
                                                    $"{reminde.Datetime:dd.MM.yyyy HH:mm}\n" +
                                                    (reminde.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminde.Repeat) +
                                                    (reminde.IsActive ? "" : "\nНагадування вимкнено.");
                                                await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                            }
                                        }
                                        reminders.SerializeReminders();
                                        return;
                                    }
                                }
                            }
                            if (num > count)
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування не існує, введіть знову", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                                return;
                            }
                            
                        }
                        break;
                    }
                case 130: //відкладення
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                        $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                        (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (int.TryParse(messageText, out int num))
                        {
                            int count = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    count++;
                                    if (num == count)
                                    {
                                        DateTime datetime = new DateTime();
                                        datetime = reminder.Datetime;
                                        reminder.Datetime = datetime.AddMinutes(15);
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Відкладено на 15 хвилин", cancellationToken: cancellationToken);
                                        userstates.SetUserState(chatId, 10);
                                        userstates.SerializeStates();
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                                        int i = 0;
                                        foreach (Reminder reminde in reminders.Reminderlist)
                                        {
                                            if (reminde.UserID == chatId)
                                            {
                                                string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminde.Text}\nКоли зробити:\n" +
                                                    $"{reminde.Datetime:dd.MM.yyyy HH:mm}\n" +
                                                    (reminde.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminde.Repeat) +
                                                    (reminde.IsActive ? "" : "\nНагадування вимкнено.");
                                                await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                            }
                                        }
                                        reminders.SerializeReminders();
                                        return;
                                    }
                                }
                            }
                            if (num > count)
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування не існує, введіть знову", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                                return;
                            }
                        }
                        break;
                    }
            }
            //для створення та видалення нотаток
            switch (userstate)
            {
                case 20:
                    {
                        if (messageText == "Назад")
                        {
                            userstates.SetUserState(chatId, 0);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Головне меню:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard0);
                            return;
                        }
                        if (messageText == "Нагадування")
                        {
                            userstates.SetUserState(chatId, 10);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нагадування:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard10);
                            int i = 0;
                            foreach (Reminder reminder in reminders.Reminderlist)
                            {
                                if (reminder.UserID == chatId)
                                {
                                    string userreminders = $"Нагадування {++i}:\n" + $"Що зробити:\n{reminder.Text}\nКоли зробити:\n" +
                                        $"{reminder.Datetime:dd.MM.yyyy HH:mm}\n" +
                                        (reminder.Repeat == "Без повторень" ? "Без повторень" : "Повторювати:\n" + reminder.Repeat) +
                                        (reminder.IsActive ? "" : "\nНагадування вимкнено.");
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: userreminders, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (messageText == "Створити нову нотатку")
                        {
                            userstates.SetUserState(chatId, 21);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть текст нотатки", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                            return;
                        }
                        if (messageText == "Видалити нотатку")
                        {
                            userstates.SetUserState(chatId, 22);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Введіть яку з нотаткок видалити:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                            return;
                        }
                        break;
                    }
                case 21:
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 20);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатки:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard20);
                            int k = 0;
                            foreach (Note note in notes.Notelist)
                            {
                                if (note.UserID == chatId)
                                {
                                    string usernotes = $"Нотатка {++k}:\n{note.Text}";
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: usernotes, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        notes.Notelist.Add(new Note(chatId, messageText));
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатку додано", cancellationToken: cancellationToken);
                        userstates.SetUserState(chatId, 20);
                        userstates.SerializeStates();
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатки:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard20);
                        int i = 0;
                        foreach (Note note in notes.Notelist)
                        {
                            if (note.UserID == chatId)
                            {
                                string usernotes = $"Нотатка {++i}:\n{note.Text}";
                                await botClient.SendTextMessageAsync(chatId: chatId, text: usernotes, cancellationToken: cancellationToken);
                            }
                        }
                        notes.SerializeNotes();
                        return;
                    }
                case 22:
                    {
                        if (messageText == "Відміна")
                        {
                            userstates.SetUserState(chatId, 20);
                            userstates.SerializeStates();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатки:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard20);
                            int k = 0;
                            foreach (Note note in notes.Notelist)
                            {
                                if (note.UserID == chatId)
                                {
                                    string usernotes = $"Нотатка {++k}:\n{note.Text}";
                                    await botClient.SendTextMessageAsync(chatId: chatId, text: usernotes, cancellationToken: cancellationToken);
                                }
                            }
                            return;
                        }
                        if (int.TryParse(messageText, out int num))
                        {
                            int count = 0;
                            foreach (Note note in notes.Notelist)
                            {
                                if (note.UserID == chatId)
                                {
                                    count++;
                                    if (num == count)
                                    {
                                        notes.Notelist.Remove(note);
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатку видалено", cancellationToken: cancellationToken);
                                        userstates.SetUserState(chatId, 20);
                                        userstates.SerializeStates();
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатки:", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard20);
                                        int k = 0;
                                        foreach (Note onenote in notes.Notelist)
                                        {
                                            if (onenote.UserID == chatId)
                                            {
                                                string usernotes = $"Нотатка {++k}:\n{onenote.Text}";
                                                await botClient.SendTextMessageAsync(chatId: chatId, text: usernotes, cancellationToken: cancellationToken);
                                            }
                                        }
                                        notes.SerializeNotes();
                                        return;
                                    }
                                }
                            }
                            if (num > count)
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId, text: "Нотатка не існує, введіть знову", cancellationToken: cancellationToken, replyMarkup: ReplyKeyboard11);
                                return;
                            }
                        }
                        break;
                    }
            }

            await botClient.SendTextMessageAsync(chatId: chatId, text: "Некоректне введення.\n", cancellationToken: cancellationToken);
            Console.WriteLine($"Sent a 'incorrect input' message in chat {chatId} to {username}");
        }
    }
}
