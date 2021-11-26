#/bin/bash

cd ~

git clone https://github.com/wabbajack-tools/ussedp

cd ussedp

echo "Enter Your sudo Password Below:"

sudo dotnet build -c release

if [ ! -f "ussedp-patches.7z" ]
then
    wget --load-cookies /tmp/cookies.txt "https://docs.google.com/uc?export=download&confirm=$(wget --quiet --save-cookies /tmp/cookies.txt --keep-session-cookies --no-check-certificate 'https://docs.google.com/uc?export=download&id=1lDxHD_bDwltsp88fhazXTngVozr6TDXh' -O- | sed -rn 's/.*confirm=([0-9A-Za-z_]+).*/\1\n/p')&id=1lDxHD_bDwltsp88fhazXTngVozr6TDXh" -O ussedp-patches.7z && rm -rf /tmp/cookies.txt
else
    echo "patch files already downloaded"
fi

if [ ! -d "/home/$(whoami)/ussedp/BestOfBothWorlds" ]
then
    7z x -y 'ussedp-patches.7z'
else
    echo "patch files already installed"
fi

rsync -avx ~/ussedp/BestOfBothWorlds/ ~/ussedp/Patcher/bin/Release/net6.0/

if [ -d "/home/$(whoami)/Linux-ussedp-BestOfBothWorlds" ]
then
    rm -r ~/Linux-ussedp-BestOfBothWorlds/
else
    echo "dir ready"
fi

rsync -avx ~/ussedp/Patcher/bin/Release/net6.0/ ~/Linux-ussedp-BestOfBothWorlds/

if [ -f "/home/$(whoami)/ussedp-BestOfBothWorlds.sh" ]
then
    rm ~/ussedp-BestOfBothWorlds.sh
else
    echo "script ready"
fi

printf "#/bin/bash \ncd ~/Linux-ussedp-BestOfBothWorlds/\n./Patcher\n" > ~/ussedp-BestOfBothWorlds.sh

chmod +x ~/ussedp-BestOfBothWorlds.sh

~/ussedp-BestOfBothWorlds.sh
