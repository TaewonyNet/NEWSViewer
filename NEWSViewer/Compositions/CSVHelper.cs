using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaewonyNet.Common.Compositions;

namespace NEWSViewer.Compositions
{
    public class CSVHelper
    {
        public string FileName { get; set; }

        public CSVHelper()
        {
            
        }

        public bool? SaveFileDialog()
        {
            bool? result = null;
            FileName = null;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "CSV File|*.csv|All Files|*.*";
            dialog.RestoreDirectory = true;
            dialog.AddExtension = string.IsNullOrEmpty(dialog.Filter) == false;
            result = dialog.ShowDialog();
            if (result == true)
            {
                FileName = dialog.FileName;
            }
            return result;
        }

        public bool? OpenFileDialog()
        {
            bool? result = null;
            FileName = null;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV File|*.csv|All Files|*.*";
            dialog.RestoreDirectory = true;
            dialog.AddExtension = string.IsNullOrEmpty(dialog.Filter) == false;
            result = dialog.ShowDialog();
            if (result == true)
            {
                FileName = dialog.FileName;
            }
            return result;
        }

        public bool WriteFile(List<object[]> rows)
        {
            bool result = false;
            if (string.IsNullOrEmpty(FileName) == false)
            {
                string filename = FileName;// string.Join(" ", FileName.Split("\\/:*?<>|".ToCharArray()));
                TextWriter tw = null;
                try
                {
                    if (File.Exists(filename) == true)
                    {
                        File.Delete(filename);
                    }

                    StringBuilder sb = new StringBuilder();
                    tw = new StreamWriter(filename, true, System.Text.Encoding.UTF8);

                    // 목록 저장
                    if (rows != null)
                    {
                        foreach (object[] aobjData in rows)
                        {
                            for (int i = 0; i < aobjData.Length; i++)
                            {
                                sb.Append(GetString(aobjData[i]) + ",");
                            }
                            if (rows.Count > 0)
                            {
                                sb.AppendLine();
                            }
                        }
                    }
                    tw.Write(sb);
                    tw.Flush();
                    tw.Close();
                    //File.Move(filename, FileName);
                    result = true;
                }
                catch (Exception e)
                {
                    Log.Error("WriteFile {0}", e.ToString());
                    if (tw != null)
                    {
                        tw.Close();
                    }
                }
            }
            return result;
        }

        public bool ReadFile(List<string[]> rows)
        {
            bool result = false;
            string filename = FileName;//string.Join(" ", FileName.Split("\\/:*?<>|".ToCharArray()));
            if (string.IsNullOrEmpty(FileName) == false && File.Exists(filename) == true)
            {
                try
                {
                    byte[] buff = File.ReadAllBytes(filename);
                    var enc = TaewonyNet.Common.Compositions.Utilitys.GetEncoding(buff);
                    string[] lines = enc.GetString(buff).Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        rows.Add(SplitString(lines[i]));
                    }
                    result = true;
                }
                catch (Exception e)
                {
                    Log.Error("ReadFile {0}", e.ToString());
                }
            }
            return result;
        }

        // 들어온 값에 ','나 '"'가 있을 경우 항목이 잘못 나올 경우가 있으므로 " " 로 묶음
        private static string GetString(object obj)
        {
            string result = string.Empty;
            if (obj != null)
            {
                string s = obj != null ? obj.ToString() : string.Empty;
                if ((s.Contains(",") == true)
                    || (s.Contains("\"") == true))
                {
                    result = string.Format("\"{0}\"", s.ToString().Replace("\"", "\"\""));
                }
                else
                {
                    result = s;
                }
            }
            return result;
        }

        private static string[] SplitString(string row)
        {
            List<string> result = new List<string>();
            string[] sp = row.Split(',');
            for (int i = 0; i < sp.Length; i++)
            {
                var s = sp[i];
                if (s.StartsWith("\"") == true)
                {
                    s = s.Remove(0, 1);
                    i++;
                    for (; i < sp.Length; i++)
                    {
                        if (sp[i].EndsWith("\"") == true)
                        {
                            s += "," + sp[i].Remove(sp[i].Length - 1, 1);
                            break;
                        }
                        s += "," + sp[i];
                    }
                }
                result.Add(s.Replace("\"\"", "\""));
            }
            return result.ToArray();
        }

    }
}
