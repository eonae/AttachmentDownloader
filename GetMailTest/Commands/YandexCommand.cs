using Eonae.Terminal;
using MailAccessLib;


namespace AttachementsDownloader
{
    public class YandexCommand : Command
    {
        public YandexCommand() :
            base(name: "Yandex.ru",
                 action: (args) =>
                 {
                     var mailFetcher = new MailFetcher
                     {
                         HostName = "imap.yandex.ru",
                         Port = 993,
                         UseSsl = true,
                         UserName = "chessiq@yandex.ru",
                         Password = "Sarkisian2018"
                     };
                     var analized = Sequence.AnalizeMailbox(mailFetcher);
                     if (analized.Success)
                         return Sequence.DownloadAttachments(mailFetcher, analized.Count);
                     else
                         return false;
                 },
                 validation: (args) => { return ArgsCount() == 0; },
                 commandinfo: "Downloads all attachments from yandex.ru to specified folder")
        { }
    }
}
