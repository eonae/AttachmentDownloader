using Eonae.Terminal;
using System;
using System.Linq;
using Eonae.DataStructures.Trees;

namespace AttachementsDownloader
{
    public static class Sequence
    {
        public static (bool Success, Tree<FolderWithCount> Folders, int Count) AnalizeMailbox(MailFetcher fetcher)
        {
            try
            {
                Console.WriteLine("Press any key to fetch Google Mailbox folder structure.");
                Console.ReadKey();
                Console.Write("\nConnecting... ");
                var tree = fetcher.GetFoldersTree();
                Console.Clear();
                tree.Display();
                var count = tree.Nodes.Skip(1).Select(n => n.Value.Count).Sum();
                Console.WriteLine("Completed!");
                Console.WriteLine($"Total message count: {count}");
                return (true, tree, count);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return (false, null, -1);
            }
        }

        public static bool DownloadAttachments(MailFetcher fetcher, int count)
        {
            try
            {
                var input = new ValueInput("Do you want to download all attachments? (y/n): ", new YNParser()).Read();
                if (input.Abort)
                    return false;
                else
                {
                    if (!(bool)input.Result)
                        return false;
                    else
                    {
                        var path_input = new ValueInput("Please specify path to save files: ", new PathParser()).Read();
                        if (path_input.Abort)
                            return false;
                        else
                        {
                            Console.WriteLine("\nConnecting...");
                            fetcher.GetAttachments(count, (string)path_input.Result);
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return false;
            }
        }
    }
}
