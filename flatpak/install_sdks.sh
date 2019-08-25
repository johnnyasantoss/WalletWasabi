#!/bin/sh

set +xe

flatpak install -y flathub \
    org.freedesktop.Sdk.Extension.dotnet/x86_64/18.08 \
    org.gnome.Sdk//3.32
