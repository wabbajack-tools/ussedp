#/bin/bash

cd /home/$(whoami)/

git clone https://github.com/wabbajack-tools/ussedp

cd ussedp

read -s -p "Enter Password for sudo: " sudoPW
echo $sudoPW | sudo -S dotnet build


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

echo $sudoPW | sudo -S rsync -avx /home/$(whoami)/ussedp/FullPatcher/ /home/$(whoami)/ussedp/Patcher/bin/Debug/net6.0/

if [ -d "/home/$(whoami)/Linux-ussedp-FullPatcher" ]
then
    echo $sudoPW | sudo -S rm -r /home/$(whoami)/Linux-ussedp-FullPatcher/
else
    echo "dir ready"
fi

rsync -avx /home/$(whoami)/ussedp/Patcher/bin/Debug/net6.0/ /home/$(whoami)/Linux-ussedp-FullPatcher/

if [ -f "/home/$(whoami)/ussedp-FullPatcher.sh" ]
then
    echo $sudoPW | sudo -S rm /home/$(whoami)/ussedp-FullPatcher.sh
else
    echo "script ready"
fi

printf "#/bin/bash \ncd /home/$(whoami)/Linux-ussedp-FullPatcher/\n./Patcher\n" > /home/$(whoami)/ussedp-FullPatcher.sh

echo $sudoPW | sudo -S chmod +x /home/$(whoami)/ussedp-FullPatcher.sh

/home/$(whoami)/ussedp-FullPatcher.sh
