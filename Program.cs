using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MHiRepacker
{
    internal class Program
    {
        static string loc = Path.GetDirectoryName(AppContext.BaseDirectory) + "\\";
        static string Unpack_Input = loc + "Unpack_Input";
        static string Unpack_Output = loc + "Unpack_Output";
        static string Repack_Input = loc + "Repack_Input";
        static string Repack_Output = loc + "Repack_Output";
        static string exceldata = "exceldata.dat";

        static void Main(string[] args)
        {
            string title = $"MHiRepacker Ver.1.0 By 皓月云 axibug.com";
            Console.Title = title;
            Console.WriteLine(title);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Encoding shiftencode = Encoding.GetEncoding("Shift-JIS");


            while (true)
            {
                Console.WriteLine("进行的操作:[1]解包Unpack [2]打包Repack");
                if (int.TryParse(Console.ReadLine(), out int func))
                {
#if !DEBUG
                    try
#endif
                    {
                        switch (func)
                        {
                            case 1:
                                Unpack();
                                break;
                            case 2:
                                Repack();
                                break;
                        }
                    }

#if !DEBUG
                    catch (Exception ex)
                    {
                        Console.WriteLine("处理异常:" + ex.ToString());
                    }
#endif
                }
            }
        }

        static void Unpack()
        {
            if (!Directory.Exists(Unpack_Input))
            {
                Console.WriteLine("Unpack_Input文件不存在");
                return;
            }

            if (!Directory.Exists(Unpack_Output))
            {
                Console.WriteLine("Unpack_Output文件不存在");
                return;
            }
            if (!File.Exists(Unpack_Input + "//" + exceldata))
            {
                Console.WriteLine($"{Unpack_Input + "//" + exceldata}不存在");
                return;
            }
            Console.WriteLine($"是否开始处理{exceldata}");
            if (!FileHelper.LoadFile(Unpack_Input + "//" + exceldata, out byte[] data))
            {
                Console.WriteLine($"读取失败");
                return;
            }

            if (!DoUnpack(data))
            {
                Console.WriteLine($"处理失败");
                return;
            }
            return;
        }

        static bool DoUnpack(byte[] data)
        {
            int StartHead = 0x01;

            byte[] StartHeadEndArr = { 0x50, 0x4B, 0x03, 0x04, 0x14 };//PK

            List<string> tempLog = new List<string>();
            int pos = StartHead;
            bool CheckStartEnd(byte[] temp, int startIdx)
            {
                for (int i = 0; i < StartHeadEndArr.Length; i++)
                {
                    if (StartHeadEndArr[i] != temp[startIdx++])
                        return true;
                }
                return false;
            }

            List<int> SizeList = new List<int>();
            try
            {
                int idx = 0;
                int StartContentPtr = 0;
                string log;
                tempLog.Add("文件头读取：");
                while (CheckStartEnd(data, pos) && CheckStartEnd(data, pos + 1))
                {
                    int size = HexHelper.bytesToInt(data, 2, pos);
                    SizeList.Add(size);
                    log = $"lenght:[{idx++}]=>{size}({size.ToString("X")})| byte src: 0x{data[pos].ToString("X")} 0x{data[pos + 1].ToString("X")}";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                    pos += 2;
                }
                log = $"读取完毕共{SizeList.Count}个";
                Console.WriteLine(log);
                tempLog.Add(log);

                StartContentPtr = pos;
                for (int i = 0; i < SizeList.Count; i++)
                {
                    string filename = Unpack_Output + $"\\{i.ToString().PadLeft(3, '0')}.zip";
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        fs.Write(data, StartContentPtr, SizeList[i]);
                    }
                    StartContentPtr += SizeList[i];

                    log = $"写入{filename}";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                }


                log = $"Unpack完毕，共{SizeList.Count}个.zip";
                Console.WriteLine(log);
                tempLog.Add(log);
                if(File.Exists(Unpack_Output + $"//filelist.txt"))
                    File.Delete(Unpack_Output + $"//filelist.txt");

                File.WriteAllLines(Unpack_Output + $"//filelist.txt", tempLog);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        static void Repack()
        {
            if (!Directory.Exists(Repack_Input))
            {
                Console.WriteLine("Repack_Input文件不存在");
                return;
            }

            if (!Directory.Exists(Unpack_Output))
            {
                Console.WriteLine("Unpack_Output文件不存在");
                return;
            }


            if (!DoRepack())
            {
                Console.WriteLine($"处理失败");
                return;
            }
            return;
        }

        static bool DoRepack()
        {
            int StartHead = 0x01;
            string[] zipfiles = FileHelper.GetDirFile(Unpack_Output).Where(w => w.ToLower().EndsWith(".zip")).ToArray();
            List<byte[]> files = new List<byte[]>();

            Console.WriteLine($"-----------原数据读取完毕-----------");
            Console.WriteLine($"待处理文件:");
            for (int i = 0; i < zipfiles.Length; i++)
            {
                if (!FileHelper.LoadFile(zipfiles[i], out byte[] filedata))
                {
                    Console.WriteLine($"加载失败，文件:{zipfiles[i]}");
                    return false;
                }
                else
                {
                    files.Add(filedata);
                    Console.WriteLine($"加载，待处理文件:{zipfiles[i]}，Done");
                }
            }

            Console.WriteLine($"共{files.Count}个文件，是否处理? (y/n)");
            string yn = Console.ReadLine();
            if (yn.ToLower() != "y")
                return false;

            List<string> tempLog = new List<string>();
            string log;
            using (FileStream fs = new FileStream(Repack_Output + "//" + exceldata, FileMode.Create))
            {
                fs.WriteByte((byte)files.Count);
                log = $"写入文件个数{files.Count}=>0x{files.Count.ToString("X")}";
                Console.WriteLine(log);
                tempLog.Add(log);

                for (int i = 0; i < files.Count; i++)
                {
                    int lenght = files[i].Length;

                    byte[] lenghtdata = HexHelper.intToBytes(lenght);
                    int lenghtVal = Math.Min(2, lenghtdata.Length);
                    fs.Write(lenghtdata, 0, lenghtVal);
                    if (lenghtdata.Length == 1)
                        fs.WriteByte(0);

                    log = $"写入第[{i}]个文件大小{lenght}到文件头";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                }

                log = $"文件头写入完毕";


                for (int i = 0; i < files.Count; i++)
                {
                    fs.Write(files[i], 0, files[i].Length);
                    log = $"写入第[{i}]个文件";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                }

            }

            log = $"Repack完毕：{Repack_Output + "//" + exceldata}";
            Console.WriteLine(log);
            tempLog.Add(log);
            if (File.Exists(Repack_Output + $"//filelist.txt"))
                File.Delete(Repack_Output + $"//filelist.txt");

            File.WriteAllLines(Repack_Output + $"//filelist.txt", tempLog);
            return true;
        }
    }
}
