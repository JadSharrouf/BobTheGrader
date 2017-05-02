using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Threading;
using System.Text;

namespace BobTheGrader.Models
{
    /// <summary>
    /// This program serves as an intermediary between the grader and the client-side as it unzips the submitted file, corrects and identifies the first inconsistent line if found.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// This method interacts with the database and calls on the correction method.
        /// </summary>
        /// <param name="subId">The submission identifier.</param>
        /// <param name="path">The path of the file to be graded.</param>
        /// <returns>An integer representing the location of mistake.</returns>
        public static int run(int subId, string path)
        {
            SqlConnection cn = new SqlConnection(@"Server=tcp:grader.database.windows.net,1433;Initial Catalog=grader;Persist Security Info=False;User ID=projectgrader;Password=Lollol111;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            cn.Open();
            SqlCommand cm = new SqlCommand("SELECT * FROM vwGrader WHERE SubmissionID = " + subId, cn);
            SqlDataReader reader = cm.ExecuteReader();

            reader.Read();

            string qtype = (string)reader["title"];
            string output = (string)reader["output"];

            return Run(path, qtype, output);
        }

        /// <summary>
        /// This method unzips and compiles the file uploaded, and creates an instance of the appropriate grader.
        /// </summary>
        /// <param name="path">File location.</param>
        /// <param name="qtype">Type of the question.</param>
        /// <param name="output">Expected output.</param>
        /// <returns>An integer representing the location of mistake.</returns>
        public static int Run(string path, string qtype, string output)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string workingD = Path.GetDirectoryName(path) + "\\";
            String zipped = workingD + fileName + ".zip";

            ZipFile.ExtractToDirectory(zipped, workingD);
            FileInfo zip = new FileInfo(workingD + fileName + ".zip");
            zip.Delete();
            //initializing process
            Process ps = new Process();
            ps.StartInfo = new ProcessStartInfo();
            ps.StartInfo.FileName = @"D:\Program Files\Java\jdk1.7.0_51\bin\javac";
            ps.StartInfo.WorkingDirectory = workingD;
            ps.StartInfo.CreateNoWindow = true;
            ps.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            ps.StartInfo.UseShellExecute = false;

            if (Directory.Exists(workingD))
            {
                DirectoryInfo di = new DirectoryInfo(workingD);
                FileInfo[] files = di.GetFiles();

                //handling several java files (
                if (files.Length > 1)
                {
                    return -10003;
                }
                else if (files.Length == 1)
                {
                    if (files[0].Extension.Equals(".java"))
                    {
                        String fileN = Path.GetFileNameWithoutExtension(files[0].Name);
                        ps.StartInfo.Arguments = fileN + ".java";
                        ps.Start();
                        ps.WaitForExit();

                        var compiled = false;
                        DirectoryInfo newd = new DirectoryInfo(workingD);
                        foreach(var fil in newd.GetFiles())
                        {
                            if (Path.GetExtension(fil.FullName).Equals(".class")) compiled = true;
                        }
                        if (!compiled) return -10002;

                        ps.StartInfo.FileName = @"D:\Program Files\Java\jdk1.7.0_51\bin\java";
                        ps.StartInfo.Arguments = fileN;
                        ps.StartInfo.RedirectStandardOutput = true;
                        ps.StartInfo.UseShellExecute = false;
                        ps.Start();        

                        StringBuilder q = new StringBuilder();
                        while (!ps.HasExited)
                        {
                            q.Append(ps.StandardOutput.ReadToEnd()).Append("\n");
                        }
                        string result = q.ToString();

                        System.IO.File.WriteAllText(workingD + fileN + ".txt", result);
                        Grader grader = Grader.createGrader(qtype);
                        int line = grader.correct(workingD + fileN + ".txt", output);

                        return line;
                    }
                    else return -10004;
                }
                else return -10006;
            }
            else return -10005;
        }
    }
}
