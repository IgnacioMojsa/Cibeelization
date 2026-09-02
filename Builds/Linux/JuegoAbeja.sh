#!/bin/sh
printf '\033c\033]0;%s\a' JuegoAbeja
base_path="$(dirname "$(realpath "$0")")"
"$base_path/JuegoAbeja.x86_64" "$@"
