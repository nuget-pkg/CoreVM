#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
cd CoreVM.Demo
dotnet run --project CoreVM.Demo.csproj --framework net462 "$@"
