#!/bin/sh

set +xe

./flatpak-builder-tools/dotnet/flatpak-dotnet-generator.py --runtime linux-x64 --destdir ./nuget ./nuget-sources.json ../WalletWasabi.Gui/WalletWasabi.Gui.csproj
