#!/usr/bin/env bash
#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

set -e

SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
DIR="$( cd -P "$( dirname "$SOURCE" )" && pwd )"
REPOROOT="$( cd -P "$DIR/../.." && pwd )"

source "$DIR/../_common.sh"

[ ! -z "$CONFIGURATION" ] || die "Missing required environment variable CONFIGURATION"
[ ! -z "$OUTPUT_DIR" ] || die "Missing required environment variable OUTPUT_DIR"

PROJECTS=( \
    Microsoft.DotNet.ProjectModel \
    Microsoft.DotNet.ProjectModel.Workspaces \
)

# Put stage2 on the path
export PATH=$STAGE2_DIR/bin:$PATH
for project in ${PROJECTS[@]}
do
    dotnet pack --output "$OUTPUT_DIR" --configuration "$CONFIGURATION" "$REPOROOT/src/$project" --version "$DOTNETCLI_BUILD_VERSION"
done
