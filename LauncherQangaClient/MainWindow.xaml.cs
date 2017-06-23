using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using laucherQange;
using Newtonsoft.Json;
using System.IO.Compression;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace LauncherQangaClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        private const string ipAddresseServeur = "163.172.121.13";
        private System.ComponentModel.BackgroundWorker backgroundWorker1 = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
           
                      
          
        }
        int compteur = 0;
        private void miseAJour(string update,string version,string url)
        {
            //string update = @"C:\Users\adrie\OneDrive\Documents\DiffEngine_src\movie.json";

            string[] lines = System.IO.File.ReadAllLines(update);


            List<filupdate> deserializedProduct = (List<filupdate>)JsonConvert.DeserializeObject(lines[0], typeof(List<filupdate>));
            
            foreach(filupdate file in deserializedProduct)
            {

                if(file.Type == 0)
                {
                    ftp ftpClient = new ftp(@"ftp://"+ipAddresseServeur, "launcher", "123456");
                    ftpClient.download(url+"\\jeu\\normal"+file.Fichier , @"."+file.Fichier, null);

                }
                else if(file.Type == 1)
                {
                    try
                    {
                        string oldFile = "."+file.Fichier;
                        string newFile = "."+file.Fichier + ".new";
                        string patchFile = "."+file.Updateur;
                        using (FileStream input = new FileStream(oldFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (FileStream output = new FileStream(newFile, FileMode.Create))
                            BinaryPatchUtility.Apply(input, () => new FileStream(patchFile, FileMode.Open, FileAccess.Read, FileShare.Read), output);
                        File.Delete("."+file.Fichier);
                        File.Move(newFile, "."+file.Fichier);
                        

                    }
                    catch (FileNotFoundException ex)
                    {
                        Console.Error.WriteLine("Could not open '{0}'.", ex.FileName);
                    }
                }
                else if(file.Type == 2)
                {

                }
                else
                {
                    MessageBox.Show("Error 203");
                }
                
              

            }

            FileStream ver = new FileStream("version.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            ver.Write(System.Text.Encoding.UTF8.GetBytes(version), 0, System.Text.Encoding.UTF8.GetBytes(version).Length);
            ver.Close();
            Directory.Delete("./patch", true);
            File.Delete("update.zip");

        }


        private Boolean run;
        private string reponse = "";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           

            

                if (File.Exists("version.txt"))
                {
              
                    string[] lines = System.IO.File.ReadAllLines("version.txt");
                    ConnectServeur s = new ConnectServeur();
                    try {
                        reponse = s.connect(ipAddresseServeur, 6000, "L " + lines[0]);
                        //envoie ligne 0 launcher, ligne 1 jeu
                        if (reponse != "ok" )
                        {
                        backgroundWorker1 = new BackgroundWorker();
                        backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_workUpdate);
                            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompletedUpdate);
                            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_change);
                            backgroundWorker1.WorkerReportsProgress = true;

                            backgroundWorker1.RunWorkerAsync();
                          
                            
                        }
                    else
                    {
                        this.start();
                    }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("une erreur c'est produite erreur 422" + ex.Message);
                    }
                

                }
                else
                {
                    ConnectServeur s = new ConnectServeur();
                     reponse = s.connect(ipAddresseServeur, 6000, "L 0.0.0.0.0.0");
                    if (reponse != "-1")
                    {
                        // recuperer l'emplacement
                        backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_work);
                    backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
                    backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_change);
                    backgroundWorker1.WorkerReportsProgress = true;

                    backgroundWorker1.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show("Serveur Non dipsognible");
                    }
            }

        }

        private void backgroundWorker1_change(object sender,  ProgressChangedEventArgs e)
        {
            progressabar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_work(object sender, DoWorkEventArgs e)
        {
            ftp ftpClient = new ftp(@"ftp://"+ipAddresseServeur, "launcher", "123456");


            ftpClient.download(reponse.Split(' ')[0] + "\\jeu\\zip\\jeu.zip", "Jeu.zip", backgroundWorker1);
            
        }
        private void backgroundWorker1_RunWorkerCompleted(
           object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
          
            else
            {
                // Finally, handle the case where the operation 
                // succeeded.
                ZipFile.ExtractToDirectory(@"Jeu.zip", @".\");
                File.Delete(@"Jeu.zip");
                string version = reponse.Split(' ')[1];
                FileStream ver = new FileStream("version.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                ver.Write(System.Text.Encoding.UTF8.GetBytes(version), 0, System.Text.Encoding.UTF8.GetBytes(version).Length);
                ver.Close();
                start();

            }

            // Enable the UpDown control.
          
        }
        private void backgroundWorker1_workUpdate(object sender, DoWorkEventArgs e)
        {
            ftp ftpClient = new ftp(@"ftp://"+ipAddresseServeur, "launcher", "123456");
            ftpClient.download(reponse.Split(' ')[0] + "\\update.zip", @".\update.zip", backgroundWorker1);
           
           
           

        }
        private void backgroundWorker1_RunWorkerCompletedUpdate(object sender, RunWorkerCompletedEventArgs e)
        {
            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }

            else
            {
                if (reponse != "ok" && reponse !="-1")
                {
                    string version = reponse.Split(' ')[1];
                    ZipFile.ExtractToDirectory(@"update.zip", @".\patch");

                    miseAJour("patch\\updateLauncherJeu.json", version, reponse.Split(' ')[0]);

                    // 
                    //ecrire la nouvelle version

                    File.Delete("updateJeu.json");
                    FileStream ver = new FileStream("version.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    ver.Write(System.Text.Encoding.UTF8.GetBytes(version), 0, System.Text.Encoding.UTF8.GetBytes(version).Length);
                    ver.Close();


                    while (backgroundWorker1.IsBusy) { Thread.Sleep(1000); }
                    ConnectServeur s = new ConnectServeur();
                    try
                    {
                        string[] lines = System.IO.File.ReadAllLines("version.txt");
                        reponse = s.connect(ipAddresseServeur, 6000, "L " + version);
                        //envoie ligne 0 launcher, ligne 1 jeu
                        if (reponse != "ok")
                        {
                            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_workUpdate);
                            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompletedUpdate);
                            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_change);
                            backgroundWorker1.WorkerReportsProgress = true;

                            backgroundWorker1.RunWorkerAsync();

                        }

                        else
                        {
                            this.start();
                        }

                        }
                    catch (Exception ex)
                    {
                        MessageBox.Show("une erreur c'est produite erreur 42" + ex.Message);
                    }
                }
                else if (reponse == "-1")
                {
                    MessageBox.Show("problème de connexion!");
                }
            }

            // Enable the UpDown control.

        }


        private void start()
        {
            Process.Start("QangaJeu.exe");
            Process.GetCurrentProcess().Kill();
           // MessageBox.Show("reussi");
        }








    }
}
