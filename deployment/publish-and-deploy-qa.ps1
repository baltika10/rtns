param(
    [Parameter(Mandatory=$true)]
    [string]$NamedProfile = 'apfie'
)

$EnvType = 'qa'

./publish-and-deploy.ps1 -EnvType $EnvName -StackName $EnvType-RTNS -NamedProfile $NamedProfile