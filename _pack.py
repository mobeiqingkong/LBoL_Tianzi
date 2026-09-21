# -*- coding: utf-8 -*-
r"""
_pack.py —— 打 thunderstore zip（替代 csproj 里会失败的 Compress-Archive）

为什么不用 PowerShell Compress-Archive：
  csproj 用 -Path '$(TargetPath)', '$(ProjectDir)DirResources\*', ... 展开成 50+ 个条目，
  命令行长度超过 Windows 上限（约 32767 字符）→ 必然 exit 1。
  （表现为 MSB3073，但 C# 编译本身是成功的。）

zip 规范：所有条目【平铺在根目录】，不带外层文件夹（thunderstore 要求）。

用法：
    python _pack.py <项目根目录> [输出zip路径]
"""
import os
import sys
import zipfile
import hashlib


def md5(p):
    h = hashlib.md5()
    with open(p, 'rb') as f:
        for b in iter(lambda: f.read(1 << 20), b''):
            h.update(b)
    return h.hexdigest()


def main():
    if len(sys.argv) < 2:
        print('用法: python _pack.py <项目根目录> [输出zip路径]')
        return 1
    proj = os.path.abspath(sys.argv[1])
    asm = 'TianziMod_windows'
    out = (os.path.abspath(sys.argv[2]) if len(sys.argv) > 2
           else os.path.join(proj, asm + '-thunderstore.zip'))
    dll = os.path.join(proj, 'bin', 'Debug', 'netstandard2.1', asm + '.dll')
    dd = os.path.join(proj, 'DirResources')

    entries = []
    if os.path.exists(dll):
        entries.append((dll, asm + '.dll'))
    else:
        print('!! 找不到 DLL:', dll)

    for name in ('manifest.json', 'README.md'):
        p = os.path.join(proj, name)
        if os.path.exists(p):
            entries.append((p, name))
        else:
            print('!! 找不到', name)

    if os.path.isdir(dd):
        for n in sorted(os.listdir(dd)):
            p = os.path.join(dd, n)
            if os.path.isfile(p):
                entries.append((p, n))      # 平铺，不带 DirResources/ 前缀
    else:
        print('!! 找不到 DirResources 目录:', dd)
        return 1

    if os.path.exists(out):
        os.remove(out)
    with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
        for src, arc in entries:
            z.write(src, arc)

    with zipfile.ZipFile(out) as z:
        names = z.namelist()
        bad = z.testzip()
        ok_flat = all('/' not in n for n in names)
        ok_dll = (asm + '.dll') in names
    print('[pack] %s' % out)
    print('[pack] 条目 %d  大小 %d  md5=%s' % (len(entries), os.path.getsize(out), md5(out)[:12]))
    print('[pack] 完整性=%s  平铺=%s  DLL=%s' % (
        'OK' if bad is None else ('BAD:' + bad), ok_flat, ok_dll))
    return 0


if __name__ == '__main__':
    sys.exit(main())
