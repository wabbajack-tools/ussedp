#!/bin/bash

Choice="$(cat Choice-Linux)"

runuser -u $(logname) -- bash<<_
    cd ~

    git clone https://github.com/wabbajack-tools/ussedp temp
    rsync -avx ~/temp/ ~/ussedp/
    rm -rf temp

    cd ~/ussedp

    if [ ! -f "/home/$(logname)/ussedp/ussedp-patches.7z" ]
    then
        echo "Downloading Patch Files From Nexus..."
        wget --load-cookies /tmp/cookies.txt "https://docs.google.com/uc?export=download&confirm=$(wget --quiet --save-cookies /tmp/cookies.txt --keep-session-cookies --no-check-certificate 'https://docs.google.com/uc?export=download&id=1lDxHD_bDwltsp88fhazXTngVozr6TDXh' -O- | sed -rn 's/.*confirm=([0-9A-Za-z_]+).*/\1\n/p')&id=1lDxHD_bDwltsp88fhazXTngVozr6TDXh" -O ussedp-patches.7z 2>&1 |
        sed -u 's/.* \([0-9]\+%\)\ \+\([0-9.]\+.\) \(.*\)/\1\n# Downloading Patch Files... \2\/s, ETA \3/' |
        zenity --progress --auto-close --no-cancel --title="Downloading..."
        rm -rf /tmp/cookies.txt
    else
        echo "patch files already downloaded"
    fi

    if [ ! -d "/home/$(logname)/ussedp/$Choice" ]
    then
        echo "Extracting patch files from 7z"
        7z x -y 'ussedp-patches.7z' | 
        grep -oE '100' |
        zenity --progress --no-cancel --auto-close --percentage=0 --title="Extracting..." --text="Extracting Patch Files From ussedp-patches.7z..."
    else
        echo "patch files already installed"
    fi
_

dotnet build -c release /home/$(logname)/ussedp

rsync -avx /home/$(logname)/ussedp/BestOfBothWorlds/ /home/$(logname)/ussedp/Patcher/bin/Release/net6.0/

runuser -u $(logname) -- bash<<_
    if [ -d "/home/$(logname)/ussedp-$Choice" ]
    then
        rm -r ~/ussedp-$Choice/
    else
        echo "dir ready"
    fi

    rsync -avx ~/ussedp/Patcher/bin/Release/net6.0/ ~/ussedp-$Choice/

    if [ -f "/home/$(logname)/ussedp-$Choice.sh" ]
    then
        rm ~/ussedp-$Choice.sh
    else
        echo "script ready"
    fi

    printf '#!/bin/bash \ncd ~/ussedp-$Choice/\n./Patcher\n' > ~/ussedp-$Choice.sh

    chmod +x ~/ussedp-$Choice.sh

    ~/ussedp-$Choice.sh
_
