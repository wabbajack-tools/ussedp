#/bin/bash

cd ~/

git clone https://github.com/wabbajack-tools/ussedp

cd ussedp

read -s -p "Enter Password for sudo: " sudoPW
echo $sudoPW | sudo -S dotnet build


if [ ! -f "Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33" ]
then
    wget "https://premium-b.nexus-cdn.com/1704/57618/Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33"
fi

7z x -y 'Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33'

rsync -avx ~/ussedp/FullPatcher/ ~/ussedp/Patcher/bin/Debug/net6.0/

mkdir Linux-ussedp-FullPatcher

rsync -avx ~/ussedp/Patcher/bin/Debug/net6.0/ ~/Linux-ussedp-FullPatcher/

ln -s ~/Linux-ussedp-FullPatcher/Patcher ~/ussedp-FullPatcher

cd ~/

./ussedp-FullPatcher
