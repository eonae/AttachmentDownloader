using Eonae.Terminal;
using MailAccessLib;

namespace AttachementsDownloader
{
    public class GmailCommand : Command
    {
        public GmailCommand() :
            base(name: "Gmail.com",
                 action: (args) =>
                 {
                     var mailFetcher = new MailFetcher
                     {
                         HostName = "imap.gmail.com",
                         Port = 993,
                         UseSsl = true,
                         UserName = "eonae.white@gmail.com",
                         Password = "mtlqsgburtdymcmh"
                     };
                     var analized = Sequence.AnalizeMailbox(mailFetcher);
                     if (analized.Success)
                         return Sequence.DownloadAttachments(mailFetcher, analized.Count);
                     else
                         return false;
                 },
                 validation: (args) => { return ArgsCount() == 0; },
                 commandinfo: "Downloads all attachments from Gmail to specified folder")
        { }
    }
}
