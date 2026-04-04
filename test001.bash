#! /usr/bin/env bash
# -*- mode: sh -*-
script_dir="$(realpath `dirname "$0"`)"
cd "$script_dir"

dotnet test --filter Name~Test001 --logger "console;verbosity=detailed"
