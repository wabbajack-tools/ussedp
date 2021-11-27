#/bin/bash

echo "Enter Your sudo Password: "
read -s PassWD

cd ~

git clone https://github.com/wabbajack-tools/ussedp

cd ussedp

if [ ! -f "/home/$(whoami)/ussedp/ussedp-patches.7z" ]
then
    wget "https://premium-b.nexus-cdn.com/1704/57618/Patch%20Files-57618-1-5-1-1637899588.7z?token=SkOMWYT9c36hUces-zOADA&expires=1637992338&user_id=36400125" -O ussedp-patches.7z
    # for testing only
    # wget --load-cookies /tmp/cookies.txt "https://docs.google.com/uc?export=download&confirm=$(wget --quiet --save-cookies /tmp/cookies.txt --keep-session-cookies --no-check-certificate 'https://docs.google.com/uc?export=download&id=1lDxHD_bDwltsp88fhazXTngVozr6TDXh' -O- | sed -rn 's/.*confirm=([0-9A-Za-z_]+).*/\1\n/p')&id=1lDxHD_bDwltsp88fhazXTngVozr6TDXh" -O ussedp-patches.7z && rm -rf /tmp/cookies.txt
else
    echo "patch files already downloaded"
fi

if [ ! -d "/home/$(whoami)/ussedp/BestOfBothWorlds" ]
then
    7z x -y 'ussedp-patches.7z'
else
    echo "patch files already installed"
fi

echo "$PassWD" | sudo -S dotnet build -c release

echo "$PassWD" | sudo -S rsync -avx ~/ussedp/BestOfBothWorlds/ ~/ussedp/Patcher/bin/Release/net6.0/

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
