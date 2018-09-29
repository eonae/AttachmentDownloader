using Eonae.Terminal;
using MailAccessLib;

namespace AttachementsDownloader
{
    public class MailRuCommand : Command
    {
        public MailRuCommand() :
            base(name: "Mail.ru",
                 action: (args) =>
                 {
                     var mailFetcher = new MailFetcher
                     {
                         HostName = "imap.mail.ru",
                         Port = 993,
                         UseSsl = true,
                         UserName = "aslanov@chess-iq.ru",
                         Password = "AlphaBeth1"
                     };
                     var analized = Sequence.AnalizeMailbox(mailFetcher);
                     if (analized.Success)
                         return Sequence.DownloadAttachments(mailFetcher, analized.Count);
                     else
                         return false;
                 },
                 validation: (args) => { return ArgsCount() == 0; },
                 commandinfo: "Downloads all attachments from mail.ru to specified folder")
        { }
    }
}
