param(
    [Parameter(Mandatory=$true)]
    [string]$NamedProfile
)

$EnvType = 'prod'
$templateFolder = ".\temp"
$templateFile = "$templateFolder\packaged-template.yaml"

$bucketName = 'eu-central-1'
$keyPrefix = 'rtns'

./publish-and-deploy.ps1 -EnvType $EnvType -StackName $EnvType-RTNS -NamedProfile $NamedProfile -justStorePackagedTemplate $true

$templates = Get-S3Object -BucketName $bucketName -KeyPrefix $keyPrefix -ProfileName $NamedProfile | Where-Object {$_.Key.EndsWith('.yaml')} | Select-Object -ExpandProperty Key

$versions = New-Object System.Collections.ArrayList
foreach($template in $templates) {
    $version = [int](($template.Split('_') | Select-Object -Last 1).Split('.')[0])
    $versions.Add($version)
}

$nextVersion = (($versions | Measure-Object -Maximum).Maximum) + 1
$newFileName = "packaged-template_$nextVersion.yaml"

Write-S3Object -BucketName $bucketName -Key $keyPrefix\$newFileName -File $templateFile -ProfileName $NamedProfile

Write-Host "File $newFileName uploaded"