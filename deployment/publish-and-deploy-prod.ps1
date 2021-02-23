param(
    [Parameter(Mandatory=$true)]
    [string]$NamedProfile = 'apfie'
)

$EnvType = 'prod'

./publish-and-deploy.ps1 -EnvType $EnvType -StackName $EnvType-RTNS -NamedProfile $NamedProfile