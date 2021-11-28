#/bin/bash

runuser -u $(logname) -- bash<<_
    cd ~

    git clone https://github.com/wabbajack-tools/ussedp temp
    rsync -avx ~/temp/ ~/ussedp/
    rm -rf temp

    cd ~/ussedp

    if [ ! -f "/home/$(logname)/ussedp/ussedp-patches.7z" ]
    then
        wget "https://premium-b.nexus-cdn.com/1704/57618/Patch%20Files-57618-1-5-1-1637899588.7z?token=SkOMWYT9c36hUces-zOADA&expires=1637992338&user_id=36400125" -O ussedp-patches.7z
    else
        echo "patch files already downloaded"
    fi

    if [ ! -d "/home/$(logname)/ussedp/FullPatcher" ]
    then
        7z x -y 'ussedp-patches.7z'
    else
        echo "patch files already installed"
    fi

    sudo dotnet build -c release

    sudo rsync -avx /home/$(logname)/ussedp/FullPatcher/ /home/$(logname)/ussedp/Patcher/bin/Release/net6.0/

    if [ -d "/home/$(logname)/ussedp-FullPatcher" ]
    then
        rm -r ~/ussedp-FullPatcher/
    else
        echo "dir ready"
    fi

    rsync -avx ~/ussedp/Patcher/bin/Release/net6.0/ ~/ussedp-FullPatcher/

    if [ -f "/home/$(logname)/ussedp-FullPatcher.sh" ]
    then
        rm ~/ussedp-FullPatcher.sh
    else
        echo "script ready"
    fi

    printf "#/bin/bash \ncd ~/ussedp-FullPatcher/\n./Patcher\n" > ~/ussedp-FullPatcher.sh

    chmod +x ~/ussedp-FullPatcher.sh

    ~/ussedp-FullPatcher.sh
_