param(
	[ValidateSet('dev', 'qa', 'prod')]
	[Parameter(Mandatory=$true)]
	[string]$EnvType = 'dev',

	[Parameter(Mandatory=$true)]
	[string]$StackName = 'RTNS',
	
	[Parameter(Mandatory=$true)]
	[string]$NamedProfile = 'apfie',

	[bool]$justStorePackagedTemplate = $false
)

$buckets = @{
	dev = 'apfie-dev-artifacts';
	qa = 'apfie-dev-artifacts';
	prod = 'apfie-dev-artifacts'
}

$expectedRegions = @{
	dev = 'eu-central-1';
	qa = 'eu-central-1';
	prod = 'eu-central-1';
}

$expectedRegion = $expectedRegions[$EnvType]
$currentRegion = aws configure get region  --profile $NamedProfile

if ($currentRegion -ne $expectedRegion) {
	throw "Your current user should be a $expectedRegion user! It is $currentRegion instead."
}

dotnet publish ../src/RTNS.AWS.Subscriptions/RTNS.AWS.Subscriptions.csproj -c Release

dotnet publish ../src/RTNS.AWS.Notifications/RTNS.AWS.Notifications.csproj -c Release

$tempFolder = "./temp"
If(!(test-path $tempFolder)) {    New-Item -ItemType Directory -Force -Path $tempFolder}

aws cloudformation package --template-file cloudformation.template.yaml --output-template-file $tempFolder/packaged-template.yaml --s3-bucket $buckets[$EnvType] --s3-prefix RTNS --profile $NamedProfile

if ($justStorePackagedTemplate) {
	Write-Host "Packaged template stored in $tempFolder. No deployment done."
	exit 0;
}

aws cloudformation deploy --template-file $tempFolder/packaged-template.yaml --stack-name $StackName --capabilities CAPABILITY_NAMED_IAM --s3-bucket $buckets[$EnvType]  --s3-prefix RTNS --parameter-overrides EnvType=$EnvType EnvName=$EnvName --profile $NamedProfile

Remove-Item $tempFolder -Recurse