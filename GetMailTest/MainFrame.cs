using Eonae.Terminal;

namespace AttachementsDownloader
{
    public class MainFrame : Frame
    {
        public MainFrame() : base(true)
        {
            Settings.Messages.Greetings = "Welcome to email attachment downloader";
            AddCommand(new MailRuCommand());
            AddCommand(new GmailCommand());
            AddCommand(new YandexCommand());
            AddAlias("Gmail.com", "gmail");
            AddAlias("Mail.ru", "mail");
            AddAlias("Yandex.ru", "yandex");
        }
    }
}
