#/bin/bash

cd ~

git clone https://github.com/wabbajack-tools/ussedp

cd ussedp

echo "Enter Your Password Below:"
sudo dotnet build -c release


if [ ! -f "Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33" ]
then
    wget "https://premium-b.nexus-cdn.com/1704/57618/Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33"
else
    echo "patch files already downloaded"
fi

if [ ! -d "/home/$(whoami)/ussedp/FullPatcher" ]
then
    7z x -y 'Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33'
else
    echo "patch files already installed"
fi

rsync -avx ~/ussedp/FullPatcher/ ~/ussedp/Patcher/bin/Debug/net6.0/

if [ -d "/home/$(whoami)/Linux-ussedp-FullPatcher" ]
then
    rm -r ~/Linux-ussedp-FullPatcher/
else
    echo "dir ready"
fi

rsync -avx ~/ussedp/Patcher/bin/Debug/net6.0/ ~/Linux-ussedp-FullPatcher/

if [ -f "/home/$(whoami)/ussedp-FullPatcher.sh" ]
then
    rm ~/ussedp-FullPatcher.sh
else
    echo "script ready"
fi

printf "#/bin/bash \ncd ~/Linux-ussedp-FullPatcher/\n./Patcher\n" > ~/ussedp-FullPatcher.sh

chmod +x ~/ussedp-FullPatcher.sh

~/ussedp-FullPatcher.sh
