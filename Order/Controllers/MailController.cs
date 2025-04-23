using System;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;

public class MailController
{
    public async Task GetEmailsAsync()
    {
        // Данные для подключения 
        string imapHost = "imap.example.com";  // Адрес IMAP сервера
        int imapPort = 993;  // Порт для IMAP
        string username = "your-email@example.com";  
        string password = "your-password";  

        // Подключение к IMAP серверу
        using (var client = new ImapClient())
        {
            await client.ConnectAsync(imapHost, imapPort, SecureSocketOptions.SslOnConnect);

            // Аутентификация
            await client.AuthenticateAsync(username, password);

            // Открытие папки "Входящие"
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            // Получение сообщений (последние 10)
            int messageCount = inbox.Count;
            for (int i = messageCount - 1; i >= Math.Max(0, messageCount - 10); i--)
            {
                var message = await inbox.GetMessageAsync(i);
                Console.WriteLine($"Subject: {message.Subject}");
                Console.WriteLine($"From: {message.From}");
                Console.WriteLine($"Date: {message.Date}");
                Console.WriteLine($"Body: {message.TextBody}");  // Текстовое содержимое письма
                Console.WriteLine("---------------------------------------------------");
            }

            // Отключение от сервера
            await client.DisconnectAsync(true);
        }
    }
}
