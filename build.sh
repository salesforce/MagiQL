#!/bin/bash

if [ ! -f ./src/.paket/packet.exe ]; then
    mono ./src/.paket/paket.bootstrapper.exe
fi

pushd ./src
mono ./.paket/paket.exe restore group Build
popd

mono ./tools/FAKE/Fake.exe $@
