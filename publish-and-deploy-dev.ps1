param(
    [string]$NamedProfile = 'apfie'
)

$EnvType = 'dev'

./publish-and-deploy.ps1 -EnvType dev -StackName $EnvType-RTNS -NamedProfile $NamedProfile