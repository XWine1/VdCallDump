# VdCallDump
This tool can be used to dump `VdEra`/`VwEra` system call IDs from the Xbox ERA operating system.

![](/screenshot.png)

# Usage
To dump system call IDs, pass the path of any of the following ERA system DLLs to VdCallDump:

* `d3d11_x.dll`
* `d3d12_x.dll`
* `umd.dll`
* `umd_d.dll`
* `umd_i.dll`
* `umd_v.dll`
* `umd12.dll`
* `umd12_d.dll`
* `umd12_i.dll`
* `xhit.dll`
* `xhit_s.dll`

If a program database (PDB) file is available, its path can be specified as the second argument.
A PDB is not required, but will increase the likelihood of successfully determining system call IDs.
