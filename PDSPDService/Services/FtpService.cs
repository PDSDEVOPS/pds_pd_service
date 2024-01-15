using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Web;

using System.Text.RegularExpressions;
using log4net;

using ComponentPro;
using ComponentPro.Net;
using ComponentPro.IO;



namespace ProductionWebService.Services
{
    public enum FTPType
    {
        FTP,
        FTPS,
        SFTP
    }
    public class FtpService
    {
        private ILog log = log4net.LogManager.GetLogger(typeof(FtpService));
        private string username = string.Empty;
        private string password = string.Empty;
        private string server = string.Empty;
        private bool isSecure = false;

        const string FTP = "FTP";
        const string FTPS = "FTPS";
        const string SFTP = "SFTP";
        private FTPType FTPType;

        public FtpService(string host, string un, string pw, bool secure, FTPType FTPType)
        //public FtpService(string host, string un, string pw, bool secure)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(un) || string.IsNullOrEmpty(pw))
                throw new Exception("Host, Username and Password required.");

            server = host;
            username = un;
            password = pw;
            isSecure = secure;
            this.FTPType = FTPType;
        }

        private void Init(Sftp SFTPClient)
        {
            SFTPClient.HostKeyVerifying += Client_HostKeyVerifying;
            SFTPClient.Config.KeyExchangeAlgorithms = SecureShellKeyExchangeAlgorithm.DiffieHellmanGroup14SHA1;
            SFTPClient.Connect(server);
            SFTPClient.Authenticate(username, password);
        }

        static FtpService()
        {
            
        }

        private void Init(IFtpClient ftpClient)
        {
            ftpClient.Host = server;
            ftpClient.Credentials = new NetworkCredential(username, password);

            // these are in miliseconds, doubling from defaults.
            ftpClient.ConnectTimeout = 30000;
            ftpClient.DataConnectionConnectTimeout = 30000;
            ftpClient.DataConnectionReadTimeout = 30000;
            ftpClient.ReadTimeout = 30000;
            ftpClient.SocketKeepAlive = true;

            if (isSecure)
            {
                ftpClient.EncryptionMode = FtpEncryptionMode.Implicit;
                ftpClient.DataConnectionEncryption = true;
            }

            ftpClient.ValidateCertificate += (control, e) => { e.Accept = true; };

            /// Gets or sets a value indicating whether a test should be performed to ///
            //     see if there is stale (unrequested data) sitting on the socket. In some /// cases
            //     the control connection may time out but before the server closes /// the connection
            //     it might send a 4xx response that was unexpected and /// can cause synchronization
            //     errors with transactions. To avoid this /// problem the Execute() method checks
            //     to see if there is any data /// available on the socket before executing a command.
            //     On Azure hosting /// platforms this check can cause an exception to be thrown.
            //     In order /// to work around the exception you can set this property to false
            //     /// which will skip the test entirely however doing so eliminates the /// best
            //     effort attempt of detecting such scenarios. See this thread /// for more details
            //     about the Azure problem: /// https://netftp.codeplex.com/discussions/535879 ///
            //ftpClient.StaleDataCheck = false;

            ftpClient.Connect();
        }

        public byte[] DownloadFile(string server, string path, bool isSecure)
        {
            using (var ftpClient = new FtpClient())
            {
                Init(ftpClient);

                //if (!ftpClient.FileExists(path, FtpListOption.ForceList | FtpListOption.AllFiles))
                //    throw new Exception("The directory specified in the call does not exist: " + path);
                using (Stream s = ftpClient.OpenRead(path))
                {
                    try
                    {
                        byte[] b = new byte[8192];

                        using (MemoryStream output = new MemoryStream())
                        {
                            int read = 0;
                            do
                            {
                                read = s.Read(b, 0, b.Length);
                                if (read > 0)
                                {
                                    output.Write(b, 0, read);
                                }
                            } while (read > 0);

                            return output.ToArray();
                        }

                    }
                    finally
                    {
                        s.Close();
                    }
                }
            }
        }

        public IDictionary<string, byte[]> DownloadAllFilesWithExt(string path, string fileMatchRegEx)
        {
            IDictionary<string, byte[]> Files = null;

            if (!IsUsingSFTP)
            {
                Files = DownloadAllFilesWithExtFTP(path, fileMatchRegEx);
            }
            else
            {
                //Files = DownloadAllFilesWithExtSFTP(path, fileMatchRegEx);
                throw new NotImplementedException();
            }

            return Files;
        }

        public IDictionary<string, byte[]> MoveAllFilesWithExtToFileShare(string SourcePath, string FileMatchRegEx, string DestinationPath)
        {
            IDictionary<string, byte[]> Files = null;

            if (!IsUsingSFTP)
            {
                //Files = DownloadAllFilesWithExtFTP(path, fileMatchRegEx, DestinationPath);
                throw new NotImplementedException();
            }
            else
            {
                Files = MoveAllFilesWithExtToFileShareSFTP(SourcePath, FileMatchRegEx, DestinationPath);
            }

            return Files;
        }

        //public IDictionary<string, byte[]> DownloadLatestFileWithExtFTP(string path, string fileMatchRegEx)
            public byte[] DownloadLatestFileWithExtFTP(string path, string fileMatchRegEx)
        {
            byte[] returnFiles = null;
            using (var ftpClient = new FtpClient())
            {
                Init(ftpClient);

                FtpListItem[] files = ftpClient.GetListing(path);
                var FilteredFiles = new List<FtpListItem>();

                foreach (var FileToCheck in files)
                {
                    Regex regex = new Regex(fileMatchRegEx);
                    Match match = regex.Match(FileToCheck.Name);

                    if (match.Success)
                    {
                        FilteredFiles.Add(FileToCheck);
                    }
                }
                var FileToReturn = FilteredFiles.OrderByDescending(x => x.Modified).FirstOrDefault();

                try
                {
                    using (Stream s = ftpClient.OpenRead(FileToReturn.FullName))
                    {
                        try
                        {
                            byte[] b = new byte[8192];

                            using (MemoryStream output = new MemoryStream())
                            {
                                int read = 0;
                                do
                                {
                                    read = s.Read(b, 0, b.Length);
                                    if (read > 0)
                                    {
                                        output.Write(b, 0, read);
                                    }
                                } while (read > 0);

                                returnFiles = output.ToArray();

                            }

                        }
                        finally
                        {
                            s.Close();
                        }
                    }
                }
                catch (System.Net.Sockets.SocketException se)
                {
                    log.Error(String.Format("Error occurred in DownloadAllFilesWithExt. Path = {0}", path), se);
                    //continue;
                }
                catch (System.IO.IOException io)
                {
                    log.Error(String.Format("Error occurred in DownloadAllFilesWithExt. Path = {0}", path), io);
                    //continue;
                }

                //foreach (FtpListItem file in files)
                //{
                //    if (file.Type == FtpFileSystemObjectType.File)
                //    {
                //        System.Diagnostics.Debug.WriteLine($"{file.Name} - {file.Modified}");
                //        //Regex regex = new Regex(fileMatchRegEx);
                //        //Match match = regex.Match(file.Name);

                //        //if (match.Success)
                //        //{
                //        //    try
                //        //    {
                //        //        using (Stream s = ftpClient.OpenRead(file.FullName))
                //        //        {
                //        //            try
                //        //            {
                //        //                byte[] b = new byte[8192];

                //        //                using (MemoryStream output = new MemoryStream())
                //        //                {
                //        //                    int read = 0;
                //        //                    do
                //        //                    {
                //        //                        read = s.Read(b, 0, b.Length);
                //        //                        if (read > 0)
                //        //                        {
                //        //                            output.Write(b, 0, read);
                //        //                        }
                //        //                    } while (read > 0);

                //        //                    returnFiles.Add(new KeyValuePair<string, byte[]>(file.FullName, output.ToArray()));

                //        //                }

                //        //            }
                //        //            finally
                //        //            {
                //        //                s.Close();
                //        //            }
                //        //        }
                //        //    }
                //        //    catch (System.Net.Sockets.SocketException se)
                //        //    {
                //        //        log.Error(String.Format("Error occurred in DownloadAllFilesWithExt. Path = {0}", path), se);
                //        //        continue;
                //        //    }
                //        //    catch (System.IO.IOException io)
                //        //    {
                //        //        log.Error(String.Format("Error occurred in DownloadAllFilesWithExt. Path = {0}", path), io);
                //        //        continue;
                //        //    }

                //        //}
                //    }

                //}
            }
            return returnFiles;
        }

        private IDictionary<string, byte[]> DownloadAllFilesWithExtFTP(string path, string fileMatchRegEx)
        {
            IDictionary<string, byte[]> returnFiles = new Dictionary<string, byte[]>();
            using (var ftpClient = new FtpClient())
            {
                Init(ftpClient);

                FtpListItem[] files = ftpClient.GetListing(path);

                foreach (FtpListItem file in files)
                {

                    if (file.Type == FtpFileSystemObjectType.File)
                    {   
                        Regex regex = new Regex(fileMatchRegEx);
                        Match match = regex.Match(file.Name);

                        if (match.Success)
                        {
                            try
                            {
                                using (Stream s = ftpClient.OpenRead(file.FullName))
                                {
                                    try
                                    {
                                        byte[] b = new byte[8192];

                                        using (MemoryStream output = new MemoryStream())
                                        {
                                            int read = 0;
                                            do
                                            {
                                                read = s.Read(b, 0, b.Length);
                                                if (read > 0)
                                                {
                                                    output.Write(b, 0, read);
                                                }
                                            } while (read > 0);

                                            returnFiles.Add(new KeyValuePair<string, byte[]>(file.FullName, output.ToArray()));

                                        }

                                    }
                                    finally
                                    {
                                        s.Close();
                                    }
                                }
                            }
                            catch (System.Net.Sockets.SocketException se)
                            {
                                log.Error(String.Format("Error occurred in DownloadAllFilesWithExt. Path = {0}", path), se);
                                continue;
                            }
                            catch (System.IO.IOException io)
                            {
                                log.Error(String.Format("Error occurred in DownloadAllFilesWithExt. Path = {0}", path), io);
                                continue;
                            }

                        }
                    }

                }
            }
            return returnFiles;
        }

        private IDictionary<string, byte[]> MoveAllFilesWithExtToFileShareSFTP(string path, string fileMatchRegEx, string OutputPath)
        {
            IDictionary<string, byte[]> returnFiles = new Dictionary<string, byte[]>();

            using (Sftp SFTPClient = new Sftp())
            {
                Init(SFTPClient);

                SFTPClient.SetCurrentDirectory(path);

                FileInfoCollection FilesListing = SFTPClient.ListDirectory();

                List<string> FilesToDownload = new List<string>();

                foreach (var FileInfo in FilesListing)
                {
                    Regex regex = new Regex(fileMatchRegEx);

                    Match match = regex.Match(FileInfo.Name);

                    if (match.Success)
                    {
                        FilesToDownload.Add(FileInfo.Name);
                    }
                }

                if (FilesToDownload.Count > 0)
                {
                    var FileNames = FilesToDownload.ToArray();

                    if (!String.IsNullOrEmpty(OutputPath) && !OutputPath.EndsWith(@"\"))
                    {
                        // Append the trailing \
                        OutputPath = OutputPath + @"\";
                    }

                    if (!Directory.Exists(OutputPath))
                    {
                        Directory.CreateDirectory(OutputPath);
                    }

                    var TransferOptions = new TransferOptions()
                    {
                        MoveFiles = true
                    };

                    //SFTPClient.DownloadFiles(FileNames, OutputPath, new TransferOptions());
                    SFTPClient.DownloadFiles(FileNames, OutputPath, TransferOptions);

                    string DestinationFullFilePath;
                    byte[] FileContent = null;

                    foreach (var FileName in FilesToDownload)
                    {
                        DestinationFullFilePath = OutputPath + FileName;
                        FileContent = System.IO.File.ReadAllBytes(DestinationFullFilePath);

                        returnFiles.Add(DestinationFullFilePath, FileContent);
                    }
                }

                SFTPClient.Disconnect();
            }

            return returnFiles;
        }

        public IList<MoveFileReturnType> MoveFiles(string server, ICollection<ExportedFileToArchive> files, string newPath, bool isSecure)
        {
            var FinalFileNames = new List<MoveFileReturnType>();

            using (var ftpClient = new FtpClient())
            {
                Init(ftpClient);

                foreach (var File in files)
                {
                    var filePath = File.FileName;

                    string fileName = Path.GetFileName(filePath);

                    if (!String.IsNullOrEmpty(fileName))
                    {
                        string newFilePath = newPath + fileName;

                        if (ftpClient.FileExists(newFilePath))
                        {
                            newFilePath = newPath + Path.GetRandomFileName() + Path.GetExtension(fileName);
                        }

                        ftpClient.Rename(filePath, newFilePath);

                        FinalFileNames.Add(new MoveFileReturnType()
                        {
                            InputFileName = filePath,
                            OutputFileName = newFilePath,
                            Tickets = File.Tickets
                        });
                    }
                }
            }

            return FinalFileNames;
        }

        private void Client_HostKeyVerifying(object sender, HostKeyVerifyingEventArgs e)
        {
            e.Accept = true;
        }

        private bool IsUsingSFTP
        {
            get
            {
                bool UsingSFTP = false;

                if (this.FTPType != null && this.FTPType == FTPType.SFTP)
                {
                    UsingSFTP = true;
                }

                return UsingSFTP;
            }
        }

        public class MoveFileReturnType
        {
            public string InputFileName { get; set; }
            public string OutputFileName { get; set; }
            public List<Guid> Tickets { get; set; }
        }

        public class ExportedFileToArchive
        {
            public string FileName { get; set; }
            public byte[] File { get; set; }
            public List<Guid> Tickets { get; set; }
        }
    }
}