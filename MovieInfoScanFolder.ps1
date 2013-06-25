param(
	[String]	
	[ValidateScript({Test-Path $_ -PathType 'Container'})]
	[Parameter(Mandatory=$true)]
	$path,
	[String]	
	[ValidateScript({Test-Path $_ -PathType 'Leaf'})]
	[Parameter(Mandatory=$true)]
	$toolPath
)

$fileEntries = [IO.Directory]::GetFiles($path, "*.avi") 
foreach ($fileName in $fileEntries) 
{ 
	& $toolPath $fileName -nowait    
}  

$fileEntries = [IO.Directory]::GetFiles($path, "*.mp4") 
foreach ($fileName in $fileEntries) 
{ 
	& $toolPath $fileName -nowait    
}  

$fileEntries = [IO.Directory]::GetFiles($path, "*.mkv") 
foreach ($fileName in $fileEntries) 
{ 
	& $toolPath $fileName -nowait    
}  
