using System.Collections.Generic;
using System.Linq;
using System;
using MimeKit;
using MailKit.Net.Imap;
using MailKit;
using System.IO;
using Eonae.DataStructures.Trees;

namespace MailAccessLib
{
    public class FolderWithCount
    {
        public IMailFolder Folder { get; set; }
        public int Count { get; set; }
        public override string ToString()
        {
            return $"{Folder.ToString()} - {Count}";
        }
        public FolderWithCount(IMailFolder folder)
        {
            Folder = folder;
        }
    }

    public class MailFetcher
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        private int _processed = 0;
        private int _downloaded = 0;
        private long _size = 0;
        private string last_downloaded = string.Empty;

        private string Normalize(string fileName)
        {
            string temp = fileName;
            char[] invalid_chars = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            foreach (var ch in invalid_chars)
                temp = temp.Replace(ch, '#');
            return temp;
        }
        private string CopyCount(int i)
        {
            if (i == 0) return string.Empty;
            else return $" Copy {i}";
        }
        private string SizeToString(long size)
        {
            if (size < 1024 * 1024)
                return $"{ _size / 1024} kb";
            else
                return $"{_size / 1024 / 1024} MB";
        }
        private string GetFileName(MimeEntity attachement, string path)
        {
            string att_fullName = Normalize(attachement.ContentDisposition?.FileName ?? attachement.ContentType.Name);
            var arr = att_fullName.Split('.');
            string ext = arr[arr.Length - 1];
            string original_name = string.Join(".", arr.Take(arr.Length - 1));
            int copy_number = 0;
            string name = original_name;
            while (File.Exists(Path.Combine(path, $"{name}.{ext}")))
            {
                copy_number++;
                name = original_name + CopyCount(copy_number);
            }
            return $"{path}\\{name}.{ext}";
        }

        public Tree<FolderWithCount> GetFoldersTree()
        {
            var _tree = new Tree<FolderWithCount>(); // Создаётся корень дерева

            void GetChildren(IMailFolder parent_folder)
            {
                foreach (var f in parent_folder.GetSubfolders())
                {
                    _tree.AddNode(new FolderWithCount(f), _tree.Nodes.Skip(1).Where(n => n.Value.Folder == parent_folder).Single());
                    GetChildren(f);
                }
            }

            using (ImapClient client = new ImapClient())
            {
                // Вход на сервер.

                client.Connect(HostName, Port, UseSsl);
                client.Authenticate(UserName, Password);

                // Получение списка папок.

                Console.Write("\nAnalysing mailbox folders... ");
                var main_folders = client.GetFolder(client.PersonalNamespaces[0]).GetSubfolders();
                foreach (var mf in main_folders)
                {
                    _tree.AddNode(new FolderWithCount(mf), _tree.Root);
                    GetChildren(mf);
                }
                Console.Write("\nCalculating messages... ");
                var counts = GetCount(_tree.Nodes.Skip(1).Select(n => n.Value.Folder));
                foreach (var node in _tree.Nodes.Skip(1))
                    node.Value.Count = counts.PerFolder[node.Value.Folder];
            }
            return _tree;
        }

        public (int Total, Dictionary<IMailFolder, int> PerFolder) GetCount(IEnumerable<IMailFolder> folders)
        {
            var perFolder = new Dictionary<IMailFolder, int>();
            int total = 0;

            foreach (var f in folders)
            {
                try
                {
                    f.Open(FolderAccess.ReadOnly);
                    perFolder.Add(f, f.Count);
                    total += f.Count;
                }
                catch { perFolder.Add(f, -1); }
            }

            return (total, perFolder);
        }

        public void SaveAttachments(int total, string path)
        {
            void GetChildren(IMailFolder parent_folder, List<IMailFolder> list)
            {
                foreach (var f in parent_folder.GetSubfolders())
                {
                    list.Add(f);
                    GetChildren(f, list);
                }
            }

            using (ImapClient client = new ImapClient())
            {
                // Вход на сервер.

                client.Connect(HostName, Port, UseSsl);
                client.Authenticate(UserName, Password);

                var list = new List<IMailFolder>();


                var main_folders = client.GetFolder(client.PersonalNamespaces[0]).GetSubfolders();
                foreach (var mf in main_folders)
                {
                    list.Add(mf);
                    GetChildren(mf, list);
                }

                // Скачивание вложений.

                foreach (var f in list)
                {
                    void Display()
                    {
                        Console.Clear();
                        Console.WriteLine($"Downloaded {_downloaded} files of total size {SizeToString(_size)}");
                        Console.WriteLine($"Processed {_processed}/{total} {(int)((double)_processed / total * 100)}% messages");
                        Console.WriteLine($"Downloading {last_downloaded}");
                    }

                    f.Open(FolderAccess.ReadOnly);
                    for (int i = 0; i <= f.Count-1; i++)
                    {
                        if (_processed == 1721 || _processed == 0) Console.WriteLine("Debug");
                        _processed++;
                        MimeMessage msg = f.GetMessage(i);
                        if (msg.Attachments.Count() != 0)
                        {
                            foreach (MimeEntity attachement in msg.Attachments)
                            {
                                string fileName = GetFileName(attachement, path);

                                using (var stream = File.Create(fileName))
                                {
                                    if (attachement is MessagePart)
                                    {
                                        var rfc822 = (MessagePart)attachement;
                                        rfc822.WriteTo(stream);
                                    }
                                    else
                                    {
                                        var part = (MimePart)attachement;
                                        part.Content.DecodeTo(stream);
                                    }
                                    _downloaded++;
                                    _size += stream.Length;
                                    last_downloaded = fileName;
                                }
                                Display();
                            }
                        }
                        Display();
                    }

                }
                Console.WriteLine("\nCompleted! ");
            }
        }
    }
}
