using System.Text;

namespace MHiRepacker
{
    internal class Program
    {
        static string inputFile;
        static bool bUnpack;
        static string exceldata = "exceldata.dat";

        static int mode;

        static string helperStr = @"
————————————————————————
params:
[func] [inputpath] [outputpath]
func: unpack/repack
        unpack: [inputpath] is input File path ,[outputpath] is output Directory path
        repack: [inputpath] is input Directory path ,[outputpath] is output File path

eg.

unpack G:\exceldata.dat G:\exceldata_unpacked
repack G:\exceldata_unpacked G:\exceldata.dat

unpack G:\syokibugu.dat G:\syokibugu_unpacked
repack G:\syokibugu_unpacked G:\syokibugu
————————————————————————
";

        static void PrintHelperStr()
        {
            Console.WriteLine(helperStr);
            Console.ReadLine();
        }

        static void Main(string[] args)
        {

            string title = $"MHiRepacker Ver.1.4 By 皓月云 axibug.com";
            Console.Title = title;
            Console.WriteLine(title);

            if (args.Length < 3)
            {
                PrintHelperStr();
                return;
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding shiftencode = Encoding.GetEncoding("Shift-JIS");

            string modecmd = args[0];

            Console.WriteLine($"func=>{args[0].ToLower()}");

            string inputpath;
            string outputpath;
            inputpath = args[1];
            outputpath = args[2];
            Console.WriteLine($"inputpath=>{inputpath}");
            Console.WriteLine($"outputpath=>{outputpath}");

            if (bUnpack)
            {
                if (!File.Exists(inputpath))
                {
                    Console.WriteLine($"未找到文件，can not found file: {inputpath}");
                    PrintHelperStr();
                    return;
                }
            }
            else if (bUnpack)
            {
                if (!Directory.Exists(inputpath))
                {
                    Console.WriteLine($"未找到文件，can not found directory: {inputpath}");
                    PrintHelperStr();
                    return;
                }
            }

            if (modecmd.ToLower() == "unpack")
            {
                bUnpack = true;
                if (!FileHelper.LoadFile(inputpath, out byte[] inputbytes))
                {
                    Console.WriteLine($"读取失败:{inputpath}");
                    PrintHelperStr();
                    return;
                }
                List<uint> SizeList = new List<uint>();
                List<string> tempLog = new List<string>();
                uint WorkedDataLenght = 0;
                if (!TryGetheadWithCheck(inputbytes, 0, false, ref SizeList, ref tempLog, out int checkmode, ref WorkedDataLenght))
                {
                    Console.WriteLine($"读取失败,文件以0x00起始，:{inputpath}");
                    PrintHelperStr();
                    return;
                }
                mode = checkmode;
            }
            else if (modecmd.ToLower() == "repack")
            {
                bUnpack = false;
                string[] dirs = Directory.GetDirectories(inputpath).Select(w => new DirectoryInfo(w).Name).ToArray();
                if (dirs.Length == 4 &&
                    dirs.Contains("0000") &&
                    dirs.Contains("0001") &&
                    dirs.Contains("0002") &&
                    dirs.Contains("0003")
                    )
                    mode = 1;
                else
                    mode = 0;
            }
            else
            {
                PrintHelperStr();
                return;
            }


            //if (args.Length >= 4 && args[3].ToLower() == "mode1")
            //    mode = 1;
            //else
            //    mode = 0;

            Console.WriteLine($"audo mode=>mode{mode}");

            bool bResult = false;
#if !DEBUG
                    try
                    {
#endif
            switch (mode)
            {
                case 0:
                    if (bUnpack) bResult = Unpack_Mode0(inputpath, outputpath);
                    else bResult = Repack_Mode0(inputpath, outputpath);
                    break;
                case 1:
                    if (bUnpack) bResult = Unpack_Mode1(inputpath, outputpath);
                    else bResult = Repack_Mode1(inputpath, outputpath);
                    break;
            }
#if !DEBUG
}
                    catch (Exception ex)
                    {
                        Console.WriteLine("处理异常:" + ex.ToString());
                    }
#endif
            Console.WriteLine(bResult ? "处理成功" : "处理失败");
        }

        static bool Unpack_Mode0(string Unpack_Input, string Unpack_Output)
        {
            Console.WriteLine($"开始处理{Unpack_Input}");
            if (!Directory.Exists(Unpack_Output))
            {
                Directory.CreateDirectory(Unpack_Output);
                Console.WriteLine($"创建目录{Unpack_Output}");
            }
            if (!FileHelper.LoadFile(Unpack_Input, out byte[] inputbytes))
            {
                Console.WriteLine($"读取失败");
                return false;
            }

            return Unpack_Mode0_logic(inputbytes, 0, Unpack_Output, out uint WorkedDataLenght);
        }

        readonly static uint StartHead = 0x01;
        readonly static byte[] StartPKHead = { 0x50, 0x4B, 0x03, 0x04, 0x14 };//PK
        static bool CheckPKHead(byte[] temp, int startIdx)
        {
            for (int i = 0; i < StartPKHead.Length; i++)
            {
                startIdx++;
                if (startIdx < temp.Length && StartPKHead[i] != temp[startIdx])
                    return false;
            }
            return true;
        }
        static bool TryGetheadWithCheck(byte[] inputbytes, uint startPos, bool skipConsoleLog, ref List<uint> SizeList, ref List<string> tempLog, out int checkmode, ref uint WorkedDataLenght)
        {
            if (inputbytes[0] == 0x00)
            {
                checkmode = default;
                return false;
            }
            WorkedDataLenght += StartHead;
            uint pos = startPos + StartHead;
            //bool CheckStartEnd(byte[] temp, int startIdx)
            //{
            //    for (int i = 0; i < StartHeadEndArr.Length; i++)
            //    {
            //        if (StartHeadEndArr[i] != temp[startIdx++])
            //            return true;
            //    }
            //    return false;
            //}

            //int idx = 0;
            uint StartContentPtr = 0;
            string log;

            log = $"文件头读取：";
            if (!skipConsoleLog) Console.WriteLine(log);
            tempLog.Add(log);

            //while (CheckStartEnd(inputbytes, (int)pos) && CheckStartEnd(inputbytes, (int)pos + 1))
            for (int idx = 0; idx < inputbytes[0]; idx++)
            {
                uint size = HexHelper.bytesToUInt(inputbytes, 2, (int)pos);
                SizeList.Add(size);
                log = $"lenght:[{idx}]=>{size}({size.ToString("X")})| byte src: 0x{inputbytes[pos].ToString("X")} 0x{inputbytes[pos + 1].ToString("X")}";
                if (!skipConsoleLog) Console.WriteLine(log);
                tempLog.Add(log);
                pos += 2;
                WorkedDataLenght += 2;
            }
            log = $"读取完毕共{SizeList.Count}个";
            if (!skipConsoleLog) Console.WriteLine(log);
            tempLog.Add(log);

            long packagelenght = WorkedDataLenght + SizeList.Sum(w => w);
            checkmode = inputbytes.Length == packagelenght ? 0 : 1;
            return true;
        }
        static bool Unpack_Mode0_logic(byte[] inputbytes, uint startPos, string Unpack_Output, out uint WorkedDataLenght)
        {
            WorkedDataLenght = 0;
            uint StartHead = 0x01;
            WorkedDataLenght += StartHead;
            //byte[] StartHeadEndArr = { 0x50, 0x4B, 0x03, 0x04, 0x14 };//PK

            List<string> tempLog = new List<string>();
            uint pos = startPos + StartHead;
            List<uint> SizeList = new List<uint>();
            try
            {
                uint StartContentPtr = 0;
                string log;
                tempLog.Add("文件头读取：");
                //while (CheckStartEnd(inputbytes, (int)pos) && CheckStartEnd(inputbytes, (int)pos + 1))
                for (int idx = 0; idx < inputbytes[0]; idx++)
                {
                    uint size = HexHelper.bytesToUInt(inputbytes, 2, (int)pos);
                    SizeList.Add(size);
                    log = $"lenght:[{idx}]=>{size}({size.ToString("X")})| byte src: 0x{inputbytes[pos].ToString("X")} 0x{inputbytes[pos + 1].ToString("X")}";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                    pos += 2;
                    WorkedDataLenght += 2;
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
                        fs.Write(inputbytes, (int)StartContentPtr, (int)SizeList[i]);
                    }
                    StartContentPtr += SizeList[i];
                    WorkedDataLenght += SizeList[i];

                    log = $"写入{filename}";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                }


                log = $"Unpack完毕，共{SizeList.Count}个文件";
                Console.WriteLine(log);
                tempLog.Add(log);
                if (File.Exists(Unpack_Output + $"//filelist.txt"))
                    File.Delete(Unpack_Output + $"//filelist.txt");

                File.WriteAllLines(Unpack_Output + $"//filelist.txt", tempLog);

                return true;
            }
            catch (Exception e)
            {
                WorkedDataLenght = 0;
                return false;
            }
        }

        static bool Repack_Mode0(string Repack_Input, string Unpack_Output)
        {
            string outputdir = Path.GetDirectoryName(Unpack_Output);
            if (!Directory.Exists(outputdir))
            {
                Directory.CreateDirectory(outputdir);
                Console.WriteLine($"创建目录{outputdir}");
            }
            if (File.Exists(Unpack_Output + $".txt"))
                File.Delete(Unpack_Output + $".txt");
            List<string> tempLog = new List<string>();
            using (FileStream fs = new FileStream(Unpack_Output, FileMode.Create))
            {
                Repack_Mode0_logic(Repack_Input, fs, Unpack_Output, ref tempLog);
            }

            File.WriteAllLines(Unpack_Output + $".txt", tempLog);
            return true;
        }

        static bool Repack_Mode0_logic(string Repack_Input, FileStream fs, string Unpack_Output, ref List<string> tempLog, bool skip_y = false)
        {
            string[] zipfiles = FileHelper.GetDirFile(Repack_Input).Where(w => w.ToLower().EndsWith(".zip")).ToArray();
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

            if (!skip_y)
            {
                Console.WriteLine($"共{files.Count}个文件，是否处理? (y/n)");
                string yn = Console.ReadLine();
                if (yn.ToLower() != "y")
                    return false;
            }

            string log;
            fs.WriteByte((byte)files.Count);
            log = $"写入文件个数{files.Count}=>0x{files.Count.ToString("X")}";
            Console.WriteLine(log);
            tempLog.Add(log);

            for (int i = 0; i < files.Count; i++)
            {
                int lenght = files[i].Length;

                byte[] lenghtdata = HexHelper.uintToBytes(lenght);
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


            log = $"Repack完毕：{Unpack_Output}";
            Console.WriteLine(log);
            tempLog.Add(log);

            return true;
        }

        static bool Unpack_Mode1(string Unpack_Input, string Unpack_Output)
        {
            Console.WriteLine($"开始处理{Unpack_Input}");

            if (!Directory.Exists(Unpack_Output))
            {
                Directory.CreateDirectory(Unpack_Output);
                Console.WriteLine($"创建目录{Unpack_Output}");
            }


            if (!FileHelper.LoadFile(Unpack_Input, out byte[] fulldata))
            {
                Console.WriteLine($"读取失败");
                return false;
            }

            int subPackIdx = 0;
            uint Pos = 0;
            while (Pos < fulldata.Length)
            {
                Console.WriteLine($"----- 开始处理:SubPack[{subPackIdx}] -----");
                string SubPackOutDir = Path.Combine(Unpack_Output, subPackIdx.ToString().PadLeft(4, '0'));
                if (!Directory.Exists(SubPackOutDir))
                {
                    Directory.CreateDirectory(SubPackOutDir);
                    Console.WriteLine($"创建目录{SubPackOutDir}");
                }

                if (!Unpack_Mode0_logic(fulldata, Pos, SubPackOutDir, out uint WorkedDataLenght))
                {
                    Console.WriteLine($"SubPack[{subPackIdx}],处理失败");
                    return false;
                }
                Pos += WorkedDataLenght;

                //处理非0x00
                Console.WriteLine($"----- 处理:SubPack[{subPackIdx}]后续非0x00数据 -----");
                List<byte> temp_Not0 = new List<byte>();
                while (true)
                {
                    if (Pos + 1 < fulldata.Length && fulldata[Pos + 1] != 0x00)
                    {
                        Pos += 1;
                        temp_Not0.Add(fulldata[Pos]);
                    }
                    else
                    {
                        break;
                    }
                }

                string tail_not0 = Path.Combine(SubPackOutDir, "tail_not0");
                byte[] read_data_not0 = temp_Not0.ToArray();
                using (FileStream fs = new FileStream(tail_not0, FileMode.Create))
                {
                    fs.Write(read_data_not0, 0, read_data_not0.Length);
                }
                Console.WriteLine($"----- SubPack[{subPackIdx}]后续非0x00数据,Length:{read_data_not0.Length}存储到{tail_not0} -----");


                //处理0x00
                Console.WriteLine($"----- 处理:SubPack[{subPackIdx}]后续0x00数据 -----");
                List<byte> temp_0 = new List<byte>();
                while (true)
                {
                    if (Pos + 1 < fulldata.Length && fulldata[Pos + 1] == 0x00)
                    {
                        Pos += 1;
                        temp_0.Add(fulldata[Pos]);
                    }
                    else
                    {
                        break;
                    }
                }

                string tail_0 = Path.Combine(SubPackOutDir, "tail");
                byte[] read_data_0 = temp_0.ToArray();
                using (FileStream fs = new FileStream(tail_0, FileMode.Create))
                {
                    fs.Write(read_data_0, 0, read_data_0.Length);
                }

                Console.WriteLine($"----- SubPack[{subPackIdx}]后续0x00数据,Length:{read_data_0.Length}存储到{tail_0} -----");

                Console.WriteLine($"----- 处理完毕:SubPack[{subPackIdx}] -----");
                subPackIdx++;
                Pos += 1;
            }

            return true;

        }

        readonly static int[] subpacklenght = { 14336, 7600, 1500, 28400 };

        static bool Repack_Mode1(string Repack_Input, string Repack_Output)
        {
            string outputdir = Path.GetDirectoryName(Repack_Output);
            if (!Directory.Exists(outputdir))
            {
                Directory.CreateDirectory(outputdir);
                Console.WriteLine($"创建目录{outputdir}");
            }

            string[] dirs = Directory.GetDirectories(Repack_Input);
            for (int i = 0; i < dirs.Length; i++)
                Console.WriteLine($"发现目录:{dirs[i]}");

            Console.WriteLine($"共{dirs.Length}个目录，是否处理? (y/n)");
            string yn = Console.ReadLine();
            if (yn.ToLower() != "y")
                return false;

            List<string> tempLog = new List<string>();
            string log;
            if (File.Exists(Repack_Output + $".txt"))
                File.Delete(Repack_Output + $".txt");

            using (FileStream fs = new FileStream(Repack_Output, FileMode.Create))
            {
                for (int subpack_idx = 0; subpack_idx < dirs.Length; subpack_idx++)
                {
                    string subdir = dirs[subpack_idx];

                    log = $"-- 开始写入Subpack[{subpack_idx}]:下的zip包";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                    Repack_Mode0_logic(subdir, fs, Repack_Output, ref tempLog, true);
                    log = $"-- 完成写入Subpack[{subpack_idx}]:下的zip包";
                    Console.WriteLine(log);
                    tempLog.Add(log);

                    string tail_not0 = Path.Combine(subdir, "tail_not0");
                    log = $"-- 开始写入Subpack[{subpack_idx}]:下的非0x00数据:{tail_not0}";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                    if (!FileHelper.LoadFile(tail_not0, out byte[] inputbytes_not0))
                    {
                        Console.WriteLine($"读取失败");
                        return false;
                    }
                    fs.Write(inputbytes_not0, 0, inputbytes_not0.Length);
                    log = $"-- 完成写入Subpack[{subpack_idx}]:下的非0x00数据:length{inputbytes_not0.Length}";
                    Console.WriteLine(log);
                    tempLog.Add(log);


                    int targetSize = 0;
                    for (int sizeidx = 0; sizeidx <= subpack_idx; sizeidx++)
                        targetSize += subpacklenght[sizeidx];

                    long needAdd = (targetSize - fs.Length);

                    log = $"-- 需要补0x00长度:{needAdd}";
                    Console.WriteLine(log);
                    tempLog.Add(log);

                    if (needAdd < 0)
                    {
                        Console.WriteLine($"--Subpack[{subpack_idx}],超出MHi预设区间大小");
                        return false;
                    }

                    log = $"-- 开始写入Subpack[{subpack_idx}]:下的0x00数据";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                    for (int i = 0; i < needAdd; i++)
                        fs.WriteByte(0x00);
                    log = $"-- 完成写入Subpack[{subpack_idx}]:下的0x00数据:length{needAdd}";
                    Console.WriteLine(log);
                    tempLog.Add(log);
                }
            }
            File.WriteAllLines(Repack_Output + $".txt", tempLog);
            return true;
        }

    }
}
