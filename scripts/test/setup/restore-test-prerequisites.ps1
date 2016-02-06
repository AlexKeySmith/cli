#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

". $PSScriptRoot\..\..\common\_common.ps1"

# Restore packages
if ($Offline){
    header "Skipping test prerequisite package restore"
}
else {
    header "Restoring test prerequisite packages"
    
    & dotnet restore "$RepoRoot\test\TestPackages" --quiet --runtime "$Rid"

    $oldErrorAction=$ErrorActionPreference
    $ErrorActionPreference="SilentlyContinue"
    & dotnet restore "$RepoRoot\testapp" "$Rid" 2>&1 | Out-Null
    $ErrorActionPreference=$oldErrorAction
}