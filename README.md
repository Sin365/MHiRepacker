# MHiRepacker

Monster Hunter i for SH Unpack/Repacker Tools
"怪物猎人i for 夏普"解包打包工具


感谢"猪突猛进锅"分析文件

## params:
```
[func] [inputpath] [outputpath] <mode>
func: unpack/repack
        unpack: [inputpath] is input File path ,[outputpath] is output Directory path
        repack: [inputpath] is input Directory path ,[outputpath] is output File path
mode: mode0/mode1
        mode0(default): Universal structure (look like exceldata.dat)
        mode1: Universal structure (look like syokibugu)


eg.

unpack G:\exceldata.dat G:\exceldata_unpacked
repack G:\exceldata_unpacked G:\exceldata.dat

unpack G:\syokibugu.dat G:\syokibugu_unpacked mode1
repack G:\syokibugu_unpacked G:\syokibugu mode1
```