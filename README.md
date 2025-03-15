# MHiRepacker

## Monster Hunter i for SH Unpack/Repacker Tools ("怪物猎人i for 夏普"解包打包工具)

感谢"猪突猛进锅"分析文件

## params:
```
[func] [inputpath] [outputpath]
func: unpack/repack
        unpack: [inputpath] is input File path ,[outputpath] is output Directory path
        repack: [inputpath] is input Directory path ,[outputpath] is output File path

eg.

unpack G:\exceldata.dat G:\exceldata_unpacked
repack G:\exceldata_unpacked G:\exceldata.dat

unpack G:\syokibugu.dat G:\syokibugu_unpacked
repack G:\syokibugu_unpacked G:\syokibugu
```


# update:

### 1.4
    
抛弃mode参数，现在由程序自动选择mode1或者mode2，用户不用关心

There is no need for param:'mode' now; Automatically determine mode 1 or mode 2;

auto mode: 

        mode0(default): Universal structure (look like exceldata.dat)
        mode1: Universal structure (look like syokibugu)

解决 idata.dat 不能解包的问题

Resolve the issue of idata.dat not being able to unpack

### 1.3

解决 exceldata.dat 不能解包的问题

Resolve the issue of exceldata.dat not being able to unpack
