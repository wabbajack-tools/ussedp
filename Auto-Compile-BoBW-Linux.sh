#/bin/bash

runuser -u $(logname) -- bash<<_
    cd ~

    git clone https://github.com/wabbajack-tools/ussedp temp
    rsync -avx ~/temp/ ~/ussedp/
    rm -rf temp

    cd ~/ussedp

    if [ ! -f "/home/$(logname)/ussedp/ussedp-patches.7z" ]
    then
        wget "$(cat patch-files-download-link)" -O ussedp-patches.7z
    else
        echo "patch files already downloaded"
    fi

    if [ ! -d "/home/$(logname)/ussedp/BestOfBothWorlds" ]
    then
        7z x -y 'ussedp-patches.7z'
    else
        echo "patch files already installed"
    fi

    sudo dotnet build -c release

    sudo rsync -avx /home/$(logname)/ussedp/BestOfBothWorlds/ /home/$(logname)/ussedp/Patcher/bin/Release/net6.0/

    if [ -d "/home/$(logname)/ussedp-BestOfBothWorlds" ]
    then
        rm -r ~/ussedp-BestOfBothWorlds/
    else
        echo "dir ready"
    fi

    rsync -avx ~/ussedp/Patcher/bin/Release/net6.0/ ~/ussedp-BestOfBothWorlds/

    if [ -f "/home/$(logname)/ussedp-BestOfBothWorlds.sh" ]
    then
        rm ~/ussedp-BestOfBothWorlds.sh
    else
        echo "script ready"
    fi

    printf "#/bin/bash \ncd ~/ussedp-BestOfBothWorlds/\n./Patcher\n" > ~/ussedp-BestOfBothWorlds.sh

    chmod +x ~/ussedp-BestOfBothWorlds.sh

    ~/ussedp-BestOfBothWorlds.sh
_