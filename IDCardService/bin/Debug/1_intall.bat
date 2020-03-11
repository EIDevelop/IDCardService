%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe IDCardService.exe
Net Start IDCardService
sc config IDCardService start= auto
