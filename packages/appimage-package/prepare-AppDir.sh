#!/bin/sh

SCRIPT_DIR="$(dirname -- "$(readlink -f -- "${0}")")"
BIN_DIR="$(readlink -f -- "${1}")"

echo "script dir: $SCRIPT_DIR"
echo "bin dir: $BIN_DIR"

cp -a "$BIN_DIR/." "$SCRIPT_DIR/AppDir/usr/bin/"

chmod a+x "$SCRIPT_DIR/AppDir/AppRun"
chmod a+x "$SCRIPT_DIR/AppDir/usr/bin/eppie-console"
