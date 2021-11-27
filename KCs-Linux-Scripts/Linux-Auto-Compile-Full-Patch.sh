#/bin/bash

echo "Enter Your Sudo Password: "
read -s PassWD

while [ "$(echo "$PassWD" | sudo -S echo "true")" != "true" ]
do
    echo "..."
    sleep 5
    echo "Your Sudo Password Was Wrong Try Again: "
    read -s PassWD
done

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

if [ ! -d "/home/$(whoami)/ussedp/FullPatcher" ]
then
    7z x -y 'ussedp-patches.7z'
else
    echo "patch files already installed"
fi

echo "$PassWD" | sudo -S dotnet build -c release

echo "$PassWD" | sudo -S rsync -avx ~/ussedp/FullPatcher/ ~/ussedp/Patcher/bin/Release/net6.0/

if [ -d "/home/$(whoami)/ussedp-FullPatcher" ]
then
    rm -r ~/ussedp-FullPatcher/
else
    echo "dir ready"
fi

rsync -avx ~/ussedp/Patcher/bin/Release/net6.0/ ~/ussedp-FullPatcher/

if [ -f "/home/$(whoami)/ussedp-FullPatcher.sh" ]
then
    rm ~/ussedp-FullPatcher.sh
else
    echo "script ready"
fi

printf "#/bin/bash \ncd ~/ussedp-FullPatcher/\n./Patcher\n" > ~/ussedp-FullPatcher.sh

chmod +x ~/ussedp-FullPatcher.sh

~/ussedp-FullPatcher.sh
