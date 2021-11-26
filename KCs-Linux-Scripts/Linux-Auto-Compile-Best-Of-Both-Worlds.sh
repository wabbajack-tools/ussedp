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

if [ ! -d "/home/$(whoami)/ussedp/BestOfBothWorlds" ]
then
    7z x -y 'Patch Files-57618-1-5-1-1637899588.7z?token=KhZ-MZViNgaUYlpcobtoSQ&expires=1637920582&user_id=36400125&rip=75.130.137.33'
else
    echo "patch files already installed"
fi

echo $sudoPW | sudo -S rsync -avx /home/$(whoami)/ussedp/BestOfBothWorlds/ /home/$(whoami)/ussedp/Patcher/bin/Debug/net6.0/

if [ -d "/home/$(whoami)/Linux-ussedp-BestOfBothWorlds" ]
then
    echo $sudoPW | sudo -S rm -r /home/$(whoami)/Linux-ussedp-BestOfBothWorlds/
else
    echo "dir ready"
fi

echo $sudoPW | sudo -S rsync -avx /home/$(whoami)/ussedp/Patcher/bin/Debug/net6.0/ /home/$(whoami)/Linux-ussedp-BestOfBothWorlds/

if [ -f "/home/$(whoami)/ussedp-BestOfBothWorlds.sh" ]
then
    echo $sudoPW | sudo -S rm /home/$(whoami)/ussedp-BestOfBothWorlds.sh
else
    echo "script ready"
fi

printf "#/bin/bash \ncd /home/$(whoami)/Linux-ussedp-BestOfBothWorlds/\n./Patcher\n" > /home/$(whoami)/ussedp-BestOfBothWorlds.sh

echo $sudoPW | sudo -S chmod +x /home/$(whoami)/ussedp-BestOfBothWorlds.sh

/home/$(whoami)/ussedp-BestOfBothWorlds.sh
