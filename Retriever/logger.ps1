<#
Copyright (c) 2017, Artur Kaleta
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#>

## Co jeśli WindowsActivation miałoby dostęp do jakiegoś udziału sieciowego tylko do zapisu, w którym mogłoby przetrzymywać wspólny log dla wielu maszyn?

$log = "log.txt"

$slp_hash = @{
  0 = "Unlicensed";
  1 = "Licensed";
  2 = "OOB";
  3 = "OOT";
  4 = "Non Genuine";
  5 = "Notification";
  6 = "Extended"
}

$slp = gwmi SoftwareLicensingProduct -Filter "ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey != null"



#Date/Time
$line = [string](Get-Date -Format "yyyy.MM.dd HH:mm:ss")
$line += "`t"
#SN
$line += [string](gwmi Win32_BaseBoard).SerialNumber

$line += "`t"

#License
$line += [string]$slp_hash[[int]$slp.LicenseStatus] + " (" + (gwmi SoftwareLicensingService).OA3xOriginalProductKey + ")"

#Log
$line | Out-File -Append $log