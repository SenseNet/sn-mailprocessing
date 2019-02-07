$srcPath = [System.IO.Path]::GetFullPath(($PSScriptRoot + '\..\..\src'))
$installPackageFolder = "$srcPath\nuget\content\Admin\tools"
$installPackagePath = "$installPackageFolder\install-mailprocessing.zip"

# delete existing packages
Remove-Item $PSScriptRoot\*.nupkg

if (!(Test-Path $installPackageFolder))
{
	New-Item $installPackageFolder -Force -ItemType Directory
}

Compress-Archive -Path "$srcPath\nuget\snadmin\install-mailprocessing\*" -Force -CompressionLevel Optimal -DestinationPath $installPackagePath

nuget pack $srcPath\SenseNet.MailProcessing\SenseNet.MailProcessing.nuspec -properties Configuration=Release -OutputDirectory $PSScriptRoot
nuget pack $srcPath\SenseNet.MailProcessing\SenseNet.MailProcessing.Install.nuspec -properties Configuration=Release -OutputDirectory $PSScriptRoot
